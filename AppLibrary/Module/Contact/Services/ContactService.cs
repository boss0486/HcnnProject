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
using WebCore.ENM;

namespace WebCore.Services
{
    public interface IContactService : IEntityService<Contact> { }
    public partial class ContactService : EntityService<Contact>, IContactService
    {
        public ContactService() : base() { }
        public ContactService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = @"SELECT * FROM App_Contact WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%'" + whereCondition + " ORDER BY [CreatedDate]";
            List<ContactResult> dtList = _connection.Query<ContactResult>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ContactResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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
        public ActionResult Send(ContactCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string title = model.Title;
            string subject = model.Subject;
            string content = model.Content;
            string email = model.Email;
            string phone = model.Phone;

            if (string.IsNullOrWhiteSpace(subject))
                return Notifization.Invalid("Vui lòng chọn chủ đề tư vấn");
            //
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống họ tên");
            //
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Họ tên không hợp lệ");
            //
            if (title.Length < 1 || title.Length > 50)
                return Notifization.Invalid("Tiêu đề giới hạn [1-50] ký tự");
            //  
            if (!string.IsNullOrWhiteSpace(phone))
            {

                if (!Validate.TestPhone(phone))
                    return Notifization.Invalid("Số đ.thoại không hợp lệ");
                if (phone.Length < 1 || phone.Length > 20)
                    return Notifization.Invalid("Số đ.thoại giới hạn [1-20] ký tự");
            }
            //
            if (!string.IsNullOrWhiteSpace(email))
            {
                if (!Validate.TestEmail(email))
                    return Notifization.Invalid("Địa chỉ e-mail không hợp lệ");
                if (phone.Length < 1 || phone.Length > 20)
                    return Notifization.Invalid("E-mail giới hạn [1-120] ký tự");
            }
            //
            if (string.IsNullOrWhiteSpace(content))
                return Notifization.Invalid("Không được để trống nội dung'");
            //
            content = content.Trim();
            if (!Validate.TestText(content))
                return Notifization.Invalid("Nội dung không hợp lệ");
            //
            if (content.Length < 1 || content.Length > 120)
                return Notifization.Invalid("Nội dung giới hạn [1-120] ký tự");
            //  
            //
            ContactService bankService = new ContactService(_connection);
            bankService.Create<string>(new Contact()
            {
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Subject = subject,
                Email = email,
                Phone = phone,
                Content = content,
                State = (int)ContactEnum.ContactState.None,
                Enabled = (int)ModelEnum.State.ENABLED
            });
            //sort
            return Notifization.Success(MessageText.Sent);
        }
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound();
            //
            id = id.ToLower();
            using (var _connectDb = DbConnect.Connection.CMS)
            {
                _connectDb.Open();
                using (var _transaction = _connectDb.BeginTransaction())
                {
                    try
                    {
                        ContactService ContactService = new ContactService(_connectDb);
                        var bank = ContactService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                        if (bank == null)
                            return Notifization.NotFound();
                        //
                        ContactService.Remove(bank.ID, transaction: _transaction);
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
        }

        //##############################################################################################################################################################################################################################################################
        public Contact GetContactByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Contact WHERE ID = @Query";
                return _connection.Query<Contact>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public ContactResult ViewContactByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Contact WHERE ID = @Query";
                return _connection.Query<ContactResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public static string ViewContactState(bool state)
        {
            string result = string.Empty;
            switch (state)
            {
                case true:
                    result = "Đã xử lý";
                    break;
                case false:
                    result = "Chưa xử lý";
                    break;
                default:
                    break;
            }
            return result;
        }
    }
}