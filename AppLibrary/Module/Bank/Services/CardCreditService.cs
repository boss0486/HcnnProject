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
using WebCore.Model.Enum;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Services;
using Helper.Page;
using WebCore.Model.Entities;

namespace WebCore.Services
{
    public interface ICardCreditService : IEntityService<CardCredit> { }
    public class CardCreditService : EntityService<CardCredit>, ICardCreditService
    {
        public CardCreditService() : base() { }
        public CardCreditService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = @"SELECT * FROM App_CardCredit WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%' " + whereCondition + " ORDER BY [CreatedDate]";
            List<CardCreditResult> dtList = _connection.Query<CardCreditResult>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<CardCreditResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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
        public ActionResult Create(CardCreditCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string title = model.Title;
            string summary = model.Summary;
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống tiêu đề");
            //
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
            CardCreditService cardCreditService = new CardCreditService(_connection);
            CardCredit cardCredit = cardCreditService.GetAlls(m => !string.IsNullOrWhiteSpace(title) && m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (cardCredit != null)
                return Notifization.Invalid("Tên thẻ tín dụng đã được sử dụng");
            //
            cardCreditService.Create<string>(new CardCredit()
            {
                Title = model.Title,
                Alias = Helper.Page.Library.FormatToUni2NONE(model.Title),
                Summary = model.Summary,
                LanguageID = Helper.Current.UserLogin.LanguageID,
                Enabled = model.Enabled,
            });
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(CardCreditUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid();

            string title = model.Title;
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
            CardCreditService cardCreditService = new CardCreditService(_connection);
            string id = model.ID.ToLower();
            CardCredit cardCredit = cardCreditService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (cardCredit == null)
                return Notifization.NotFound(MessageText.NotFound);
            //
            CardCredit cardCreditTitle = cardCreditService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (cardCreditTitle != null)
                return Notifization.Invalid("Tên thẻ tín dụng đã được sử dụng");
            // update user information
            cardCredit.Title = title;
            cardCredit.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            cardCredit.Summary = model.Summary;
            cardCredit.Enabled = model.Enabled;
            cardCreditService.Update(cardCredit);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public CardCredit GetCreditByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            string query = string.Empty;
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_CardCredit WHERE ID = @Query";
            return _connection.Query<CardCredit>(sqlQuery, new { Query = id }).FirstOrDefault();
        }
        public CardCreditResult ViewCreditByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            string query = string.Empty;
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_CardCredit WHERE ID = @Query";
            return _connection.Query<CardCreditResult>(sqlQuery, new { Query = id }).FirstOrDefault();
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound();
            //
            id = id.ToLower();
            CardCreditService cardCreditService = new CardCreditService(_connection);
            var cardCredit = cardCreditService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (cardCredit == null)
                return Notifization.NotFound();
            //
            cardCreditService.Remove(id);
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static string DropdownList(string id)
        {
            string result = string.Empty;
            using (var service = new CardCreditService())
            {
                var dtList = service.DataOption(null);
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
        public List<CardCreditOption> DataOption(string languageId)
        {
            string sqlQuery = @"SELECT * FROM App_CardCredit WHERE Enabled = 1 ORDER BY Title ASC";
            return _connection.Query<CardCreditOption>(sqlQuery, new { LangID = languageId }).ToList();
        }
        //##############################################################################################################################################################################################################################################################
    }
}
