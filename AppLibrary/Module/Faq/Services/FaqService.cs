using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using System;
using Dapper;
using System.Linq;
using PagedList;
using System.Web.Mvc;
using System.Collections.Generic;
using Helper;
using System.Web;
using WebCore.Entities;
using WebCore.Model.Entities;
using Helper.Page;
using WebCore.Model.Enum;
using Helper.Pagination;

namespace WebCore.Services
{
    public interface IFaqService : IEntityService<Faq> { }
    public class FaqService : EntityService<Faq>, IFaqService
    {
        public FaqService() : base() { }
        public FaqService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################
        public ActionResult DataList(SearchModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            int page = model.Page;
            string query = model.Query;
            if (string.IsNullOrWhiteSpace(query))
                query = "";
            //
            string whereCondition = string.Empty;
            //
            SearchResult searchResult = WebCore.Model.Services.ModelService.SearchDefault(new SearchModel
            {
                Query = model.Query,
                TimeExpress = model.TimeExpress,
                Status = model.Status,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                Page = model.Page,
                TimeZoneLocal = model.TimeZoneLocal
            });
            if (searchResult != null)
            {
                if (searchResult.Status == 1)
                    whereCondition = searchResult.Message;
                else
                    return Notifization.Invalid(searchResult.Message);
            }
            // 
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = $@"SELECT * FROM App_Faq WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%' {whereCondition} ORDER BY [CreatedDate]";


            List<FaqResult> dtList = _connection.Query<FaqResult>(sqlQuery, new { Query = Helper.Page.Library.FormatToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<FaqResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            if (result.Count == 0 && page > 1)
            {
                page -= 1;
                result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            }
            if (result.Count <= 0)
                return Notifization.NotFound(MessageText.NotFound);

            Helper.Pagination.PagingModel pagingModel = new Helper.Pagination.PagingModel
            {
                PageSize = Helper.Pagination.Paging.PAGESIZE,
                Total = dtList.Count,
                Page = page
            };
            return Notifization.Data(MessageText.Success + "::" + sqlQuery, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
        }

        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(FaqCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string menuId = model.MenuID;
            string title = model.Title;
            string summary = model.Summary;
            //
            if (string.IsNullOrEmpty(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            // 
            if (string.IsNullOrEmpty(title))
                return Notifization.Invalid("Không được để trống câu hỏi");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Câu hỏi không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Câu hỏi giới hạn [2-80] ký tự");
            // summary valid               
            if (!string.IsNullOrEmpty(summary))
            {
                summary = summary.Trim();
                if (!Validate.TestText(summary))
                    return Notifization.Invalid("Giải đáp không hợp lệ");
                //
                if (summary.Length < 1 || summary.Length > 500)
                    return Notifization.Invalid("Giải đáp giới hạn từ [1-500] ký tự");
            }
            FaqService FaqService = new FaqService(_connection);
            Faq Faq = FaqService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == model.Title.ToLower()).FirstOrDefault();
            if (Faq != null)
                return Notifization.Invalid("Câu hỏi đã được sử dụng");
            //
            string Id = FaqService.Create<string>(new Faq()
            {
                MenuID = menuId,
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                LanguageID = Helper.Current.UserLogin.LanguageID,
                Enabled = model.Enabled,
            });
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(FaqUpdateModel model)
        {

            if (model == null)
                return Notifization.Invalid();
            //
            string id = model.ID.ToLower();
            string menuId = model.MenuID;
            string title = model.Title;
            string summary = model.Summary;
            //
            if (string.IsNullOrEmpty(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            //  
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống câu hỏi");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Câu hỏi không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Câu hỏi giới hạn [2-80] ký tự");
            // summary valid               
            if (!string.IsNullOrEmpty(summary))
            {
                summary = summary.Trim();
                if (!Validate.TestText(summary))
                    return Notifization.Invalid("Giải đáp không hợp lệ");
                if (summary.Length < 1 || summary.Length > 500)
                    return Notifization.Invalid("Giải đáp giới hạn [1-500] ký tự");
            }
            FaqService FaqService = new FaqService(_connection);

            Faq faq = FaqService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (faq == null)
                return Notifization.NotFound(MessageText.NotFound);
            //
            Faq FaqTitle = FaqService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && faq.ID != id).FirstOrDefault();
            if (FaqTitle != null)
                return Notifization.Invalid("Câu hỏi đã được sử dụng");
            // update user information
            faq.MenuID = menuId;
            faq.Title = title;
            faq.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            faq.Summary = model.Summary;
            faq.Enabled = model.Enabled;
            FaqService.Update(faq);
            return Notifization.Success(MessageText.UpdateSuccess);

        }

        public Faq GetFaqByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Faq WHERE ID = @Query";
                return _connection.Query<Faq>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public FaqResult ViewFaqByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Faq WHERE ID = @Query";
                return _connection.Query<FaqResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound();
            // 
            id = id.ToLower();
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    FaqService FaqService = new FaqService(_connection);
                    Faq Faq = FaqService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (Faq == null)
                        return Notifization.NotFound();
                    //
                    FaqService.Remove(Faq.ID, transaction: _transaction);
                    // remover seo
                    _transaction.Commit();
                    return Notifization.Success(MessageText.DeleteSuccess);
                }
                catch
                {
                    _transaction.Rollback();
                    return Notifization.NotService;
                }
            }
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Details(string Id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Id))
                    return Notifization.NotFound(MessageText.Invalid);
                //
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT * FROM App_Faq WHERE ID = @ID";
                var item = _connection.Query<FaqResult>(sqlQuery, new { ID = Id }).FirstOrDefault();
                if (item == null)
                    return Notifization.NotFound(MessageText.NotFound);
                //
                return Notifization.Data(MessageText.Success, data: item, role: null, paging: null);
            }
            catch
            {
                return Notifization.NotService;
            }
        }
        //##############################################################################################################################################################################################################################################################
        public static string DDLFaq(string id)
        {
            try
            {
                string result = string.Empty;
                using (var service = new FaqService())
                {
                    var dtList = service.DataOption(id);
                    if (dtList.Count > 0)
                    {
                        foreach (var item in dtList)
                        {
                            string select = string.Empty;
                            if (!string.IsNullOrWhiteSpace(id) && item.ID == id.ToLower())
                                select = "selected";
                            result += "<option value='" + item.ID + "'" + select + ">" + item.Title + "</option>";
                        }
                    }
                    return result;
                }
            }
            catch
            {
                return string.Empty;
            }
        }
        public List<FaqOption> DataOption(string langID)
        {
            try
            {
                string sqlQuery = @"SELECT * FROM App_Faq ORDER BY Title ASC";
                return _connection.Query<FaqOption>(sqlQuery, new { LangID = langID }).ToList();
            }
            catch
            {
                return new List<FaqOption>();
            }
        }

        public static string GetFaqName(string id)
        {
            using (var service = new FaqService())
            {
                Faq Faq = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (Faq == null)
                    return string.Empty;
                // 
                return Faq.Title;
            }
        }

        //##############################################################################################################################################################################################################################################################
        public static List<Faq> GetFaqList()
        {
            using (var service = new FaqService())
            {
                string sqlQuery = @"SELECT TOP 5 * FROM App_Faq WHERE Enabled = @Enabled ORDER BY Title";
                List<Faq> items = service.Query<Faq>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED }).ToList();
                return items;
            }
        }

        public static IEnumerable<FaqResult> GetFaqByMenu(string menuId, int page = 1)
        {
            using (var service = new ProductService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(menuId))
                    whereCondition = " AND MenuID = @MenuID";
                //
                string sqlQuery = $@"SELECT TOP (30) * FROM App_Faq WHERE Enabled = @Enabled {whereCondition} ORDER BY CreatedDate DESC";
                IEnumerable<FaqResult> items = service.Query<FaqResult>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED, MenuID = menuId }).OrderBy(m => m.Title).ToList();
                return items.ToPagedList(page, PagedRender.FAQ_PAGENUMBER);
            }
        }
        public static FaqResult GetFaqByAlias(string alias)
        {

            if (string.IsNullOrWhiteSpace(alias))
                return new FaqResult();
            //
            using (var service = new FaqService())
            {
                string sqlQuery = @"SELECT TOP (1) * FROM App_Faq WHERE Alias = @Alias";
                FaqResult item = service.Query<FaqResult>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        } 
    }
}
