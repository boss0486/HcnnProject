using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using System;
using Dapper;
using System.Linq;
using PagedList;
using Helper;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web;
using System.IdentityModel.Tokens.Jwt;
using WebCore.Model.Enum;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Services;
using Helper.Page;
using WebCore.Model.Entities;
using Helper.Pagination;

namespace WebCore.Services
{
    public interface IQuotationService : IEntityService<Quotation> { }
    public class QuotationService : EntityService<Quotation>, IQuotationService
    {
        public QuotationService() : base() { }
        public QuotationService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = $@"SELECT * FROM App_Quotation WHERE dbo.Uni2NONE(Title) LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY [CreatedDate]";
            var dtList = _connection.Query<QuotationResult>(sqlQuery, new { Query = query }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            var result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            if (result.Count <= 0 && page > 1)
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
            //
            return Notifization.Data(MessageText.Success, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(QuotationCreateModel model)
        {
            string menuId = model.MenuID;
            string title = model.Title;
            string htmlText = model.HtmlText;
            string summary = model.Summary;
            IEnumerable<string> arrFile = model.Files;
            // 
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống tiêu đề");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Tiêu đề không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Tiêu đề giới hạn [2-80] ký tự");
            // summary valid               
            if (!string.IsNullOrWhiteSpace(summary))
            {
                summary = summary.Trim();
                if (!Validate.TestText(summary))
                    return Notifization.Invalid("Mô tả không hợp lệ");
                if (summary.Length < 1 || summary.Length > 120)
                    return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
            }
            //
            if (!string.IsNullOrWhiteSpace(htmlText))
            {
                htmlText = htmlText.Trim();
                if (!Validate.TestText(htmlText))
                    return Notifization.Invalid("Nội dung không hợp lệ");
                if (htmlText.Length < 1 || htmlText.Length > 150000)
                    return Notifization.Invalid("Nội dung giới hạn từ [0-> 150 000] ký tự");
            }
            //
            if (arrFile != null && arrFile.Count() > 5)
                return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
            //
            QuotationService aboutService = new QuotationService(_connection);
            Quotation aboutTitle = aboutService.GetAlls(m => m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (aboutTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            string imgFile = model.ImageFile;
            var id = aboutService.Create<string>(new Quotation()
            {
                MenuID = menuId,
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                ImageFile = imgFile,
                HtmlText = htmlText,
                Enabled = model.Enabled,
            });
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string>{
                imgFile
            }, connection: _connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, connection: _connection);
            //
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(QuotationUpdateModel model)
        {
            string id = model.ID;
            string menuId = model.MenuID;
            string title = model.Title;
            string htmlText = model.HtmlText;
            IEnumerable<string> arrFile = model.Files;
            // 
            string summary = model.Summary;
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống tiêu đề");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Tiêu đề không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Tiêu đề giới hạn [2-80] ký tự");
            // summary valid               
            if (!string.IsNullOrWhiteSpace(summary))
            {
                summary = summary.Trim();
                if (!Validate.TestText(summary))
                    return Notifization.Invalid("Mô tả không hợp lệ");
                if (summary.Length < 1 || summary.Length > 120)
                    return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
            }
            //
            if (!string.IsNullOrWhiteSpace(htmlText))
            {
                htmlText = htmlText.Trim();
                if (!Validate.TestText(htmlText))
                    return Notifization.Invalid("Nội dung không hợp lệ");
                if (htmlText.Length < 1 || htmlText.Length > 150000)
                    return Notifization.Invalid("Nội dung giới hạn từ [0-> 150 000] ký tự");
            }
            //
            if (string.IsNullOrWhiteSpace(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            // 
            QuotationService aboutService = new QuotationService(_connection);
            Quotation about = aboutService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (about == null)
                return Notifization.Invalid(MessageText.Invalid);

            Quotation aboutTitle = aboutService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (aboutTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            //
            string imgFile = about.ImageFile;
            if (!string.IsNullOrWhiteSpace(model.ImageFile))
            {
                if (model.ImageFile.Length != 36)
                    return Notifization.Invalid("Hình ảnh không hợp lệ");
                //
                imgFile = model.ImageFile;
            }
            //
            if (arrFile != null && arrFile.Count() > 5)
                return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
            //
            about.MenuID = menuId;
            about.Title = title;
            about.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            about.Summary = summary;
            about.ImageFile = imgFile;
            about.HtmlText = htmlText;
            about.Enabled = model.Enabled;
            aboutService.Update(about);
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string> {
                imgFile
            }, connection: _connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, connection: _connection);
            // 
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public Quotation GetQuotationByID(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Quotation WHERE ID = @Query";
                Quotation item = _connection.Query<Quotation>(sqlQuery, new { Query = id }).FirstOrDefault();
                return item;
            }
            catch
            {
                return null;
            }
        }
        public QuotationResult ViewQuotationByID(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_Quotation WHERE ID = @Query";
            QuotationResult QuotationResult = _connection.Query<QuotationResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (QuotationResult == null)
                return null;
            //
            return QuotationResult;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            QuotationService quotationService = new QuotationService(_connection);
            Quotation quotation = quotationService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (quotation == null)
                return Notifization.NotFound();
            // delete 
            quotationService.Remove(quotation.ID);
            // delete file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.RemoveAllFileByForID(id, connection: _connection);
            //
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static List<QuotationHome> GetQuotationForHome(string id)
        {
            using (var service = new BannerService())
            {
                string sqlQuery = @"SELECT TOP (10) * FROM App_Quotation ORDER BY ViewDate DESC";
                List<QuotationHome> items = service.Query<QuotationHome>(sqlQuery).ToList();
                return items;
            }
        }
        public static QuotationResult GetQuotationDefault()
        {
            using (var service = new QuotationService())
            {
                string sqlQuery = @"SELECT TOP (1) * FROM App_Quotation WHERE IsShow = 1 AND Enabled = @Enabled";
                QuotationResult item = service.Query<QuotationResult>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED }).FirstOrDefault();
                return item;
            }
        }
        public static IEnumerable<QuotationHome> GetQuotationByMenu(string menuId, string id)
        {
            using (var service = new ProductService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(menuId))
                    whereCondition = " AND MenuID = @MenuID";
                //
                if (!string.IsNullOrWhiteSpace(menuId))
                    whereCondition = " AND ID != @ID";
                //
                string sqlQuery = $@"SELECT TOP (10) * FROM App_Quotation WHERE Enabled = @Enabled {whereCondition} ORDER Title  DESC";
                IEnumerable<QuotationHome> items = service.Query<QuotationHome>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED, MenuID = menuId, ID = id }).OrderBy(m => m.Title).ToList();
                return items;
            }
        }

        public static QuotationResult GetQuotationByAlias(string alias)
        {

            if (string.IsNullOrWhiteSpace(alias))
                return new QuotationResult();
            //
            using (var service = new QuotationService())
            {
                string sqlQuery = @"SELECT TOP (1) * FROM App_Quotation WHERE Alias = @Alias";
                QuotationResult item = service.Query<QuotationResult>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

        public static List<QuotationHome> GetQuotationOther(string menuId, string id)
        {
            using (var service = new QuotationService())
            {
                List<QuotationHome> QuotationHomes = new List<QuotationHome>();
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(menuId))
                    whereCondition += " AND MenuID = @MenuID";
                //
                if (!string.IsNullOrWhiteSpace(id))
                    whereCondition += " AND ID != @ID";
                //
                string sqlQueryAll = $@"SELECT TOP(20) * FROM App_Quotation WHERE ID IS NOT NULL {whereCondition} ORDER BY CategoryID, ViewDate";
                QuotationHomes = service.Query<QuotationHome>(sqlQueryAll, new { MenuID = menuId, ID = id }).ToList();

                return QuotationHomes;
            }
        }
    }
}