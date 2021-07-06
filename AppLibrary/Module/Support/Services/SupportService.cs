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

namespace WebCore.Services
{
    public interface ISupportService : IEntityService<Support> { }
    public class SupportService : EntityService<Support>, ISupportService
    {
        public SupportService() : base() { }
        public SupportService(System.Data.IDbConnection db) : base(db) { }
        //##############################################################################################################################################################################################################################################################
        public ActionResult DataList(SearchModel model)
        {
            #region
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
                AreaID = model.AreaID,
                TimeZoneLocal = model.TimeZoneLocal
            });
            if (searchResult != null)
            {
                if (searchResult.Status == 1)
                    whereCondition = searchResult.Message;
                else
                    return Notifization.Invalid(searchResult.Message);
            }
            #endregion
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT * FROM App_Support WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%'" + whereCondition + " ORDER BY [CreatedDate]";
            var dtList = _connection.Query<SupportResult>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(query) }).ToList();
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
        public ActionResult Create(SupportCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string title = model.Title;
            string summary = model.Summary;
            int enabled = model.Enabled;
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
            SupportService bankService = new SupportService(_connection);
            Support support = bankService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (support != null)
                return Notifization.Invalid("Tên tiêu đề đã được sử dụng");
            //
            bankService.Create<string>(new Support()
            {
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                Enabled = enabled,
            });
            string temp = string.Empty;
            //sort
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(SupportUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid();

            string title = model.Title;
            string summary = model.Summary;
            int enabled = model.Enabled;
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
            SupportService bankService = new SupportService(_connection);
            string id = model.ID.ToLower();
            Support support = bankService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (support == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            Support supportTitle = bankService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (supportTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // update user information
            support.Title = title;
            support.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            support.Summary = summary;
            support.Enabled = enabled;
            bankService.Update(support);
            //
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public Support GetSupportByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string sqlQuery = @"SELECT TOP (1) * FROM App_Support WHERE ID = @Query";
            return _connection.Query<Support>(sqlQuery, new { Query = id }).FirstOrDefault();
        }
        public SupportResult ViewSupportByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string sqlQuery = @"SELECT TOP (1) * FROM App_Support WHERE ID = @Query";
            return _connection.Query<SupportResult>(sqlQuery, new { Query = id }).FirstOrDefault();
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound();
            //
            id = id.ToLower();
            SupportService supportService = new SupportService(_connection);
            Support support = supportService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (support == null)
                return Notifization.NotFound();
            //
            supportService.Remove(id);
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static string DropdownList(string id)
        {
            string result = string.Empty;
            using (var SupportService = new SupportService())
            {
                var dtList = SupportService.DataOption(null);
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
        public List<SupportOption> DataOption(string languageId)
        {
            string sqlQuery = @"SELECT * FROM App_Support WHERE Enabled = 1 ORDER BY Title ASC";
            return _connection.Query<SupportOption>(sqlQuery, new { LangID = languageId }).ToList();
        }
        //##############################################################################################################################################################################################################################################################
        public static string GetSupportNameByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;
            //
            id = id.ToLower();
            using (var service = new SupportService())
            {
                var bank = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (bank == null)
                    return string.Empty;
                //
                return bank.Title;
            }
        }
        //##############################################################################################################################################################################################################################################################
        public static List<Support> GetSupportList()
        {
            using (var service = new SupportService())
            {
                string sqlQuery = @"SELECT TOP 5 * FROM App_Support WHERE Enabled = @Enabled ORDER BY Title";
                List<Support> items = service.Query<Support>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED }).ToList();
                return items;
            }
        }
    }
}
