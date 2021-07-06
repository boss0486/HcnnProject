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
    public interface IBankService : IEntityService<Bank> { }
    public class BankService : EntityService<Bank>, IBankService
    {
        public BankService() : base() { }
        public BankService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = @"SELECT * FROM App_Bank WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%'" + whereCondition + " ORDER BY [CreatedDate]";
            List<BankResult> dtList = _connection.Query<BankResult>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<BankResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            if (result.Count <= 0 && page > 1)
            {
                page -= 1;
                result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            }
            if (result.Count <= 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
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
        public ActionResult Create(BankCreateModel model)
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
                return Notifization.Invalid("Tiêu đề giới hạn [[2-80]] ký tự");
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
            BankService bankService = new BankService(_connection);
            Bank bank = bankService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (bank != null)
                return Notifization.Invalid("Tên ngân hàng đã được sử dụng");
            //
            bankService.Create<string>(new Bank()
            {
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                Enabled = enabled,
            });
            //sort
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(BankUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid();

            string title = model.Title;
            string summary = model.Summary;
            int enabled = model.Enabled;
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
            BankService bankService = new BankService(_connection);
            string id = model.ID.ToLower();
            Bank bank = bankService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (bank == null)
                return Notifization.NotFound(MessageText.NotFound);
            //
            Bank bankTitle = bankService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (bankTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // update user information
            bank.Title = title;
            bank.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            bank.Summary = summary;
            bank.Enabled = enabled;
            bankService.Update(bank);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public Bank GetBankByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Bank WHERE ID = @Query";
                return _connection.Query<Bank>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public BankResult ViewBankByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Bank WHERE ID = @Query";
                return _connection.Query<BankResult>(sqlQuery, new { Query = id }).FirstOrDefault();
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
            BankService bankService = new BankService(_connection);
            var bank = bankService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (bank == null)
                return Notifization.NotFound();
            //
            bankService.Remove(bank.ID);
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static string DropdownList(string id)
        {
            string result = string.Empty;
            using (var BankService = new BankService())
            {
                List<BankOption> dtList = BankService.DataOption(null);
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
        public List<BankOption> DataOption(string languageId)
        {
            string sqlQuery = @"SELECT * FROM App_Bank WHERE Enabled = 1 ORDER BY Title ASC";
            return _connection.Query<BankOption>(sqlQuery, new { LangID = languageId }).ToList();
        }
        //##############################################################################################################################################################################################################################################################
        public static string GetBankNameByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;
            //
            id = id.ToLower();
            using (var service = new BankService())
            {
                var bank = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (bank == null)
                    return string.Empty;
                //
                return bank.Title;
            }
        } 
        //##############################################################################################################################################################################################################################################################
    }
}
