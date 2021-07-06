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
    public interface ISocialNetworkService : IEntityService<SocialNetwork> { }
    public class SocialNetworkService : EntityService<SocialNetwork>, ISocialNetworkService
    {
        public SocialNetworkService() : base() { }
        public SocialNetworkService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = @"SELECT * FROM App_SocialNetwork WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%'" + whereCondition + " ORDER BY [CreatedDate]";
            List<SocialNetworkResult> dtList = _connection.Query<SocialNetworkResult>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(query) }).ToList();
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
        public ActionResult Create(SocialNetworkCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string title = model.Title;
            string summary = model.Summary;
            int iconId = model.IconID;
            string backLink = model.BackLink;
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
            SocialNetworkService socialNetworkService = new SocialNetworkService(_connection);
            SocialNetwork socialNetwork = socialNetworkService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (socialNetwork != null)
                return Notifization.Invalid("Tên tiêu đề đã được sử dụng");
            //
            socialNetworkService.Create<string>(new SocialNetwork()
            {
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                IconID = iconId,
                BackLink = backLink,
                Enabled = enabled,
            });
            string temp = string.Empty;
            //sort
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(SocialNetworkUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string title = model.Title;
            string summary = model.Summary;
            int iconId = model.IconID;
            string backLink = model.BackLink;
            int enabled = model.Enabled;
            //
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
            SocialNetworkService socialNetworkService = new SocialNetworkService(_connection);
            string id = model.ID.ToLower();
            SocialNetwork socialNetwork = socialNetworkService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (socialNetwork == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            SocialNetwork supportTitle = socialNetworkService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (supportTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // update user information
            socialNetwork.Title = title;
            socialNetwork.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            socialNetwork.Summary = summary;
            socialNetwork.IconID = iconId;
            socialNetwork.BackLink = backLink;
            socialNetwork.Enabled = enabled;
            socialNetworkService.Update(socialNetwork);
            //
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public SocialNetwork GetSocialNetworkByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string sqlQuery = @"SELECT TOP (1) * FROM App_SocialNetwork WHERE ID = @Query";
            return _connection.Query<SocialNetwork>(sqlQuery, new { Query = id }).FirstOrDefault();
        }
        public SocialNetworkResult ViewSocialNetworkByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string sqlQuery = @"SELECT TOP (1) * FROM App_SocialNetwork WHERE ID = @Query";
            return _connection.Query<SocialNetworkResult>(sqlQuery, new { Query = id }).FirstOrDefault();
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound();
            //
            id = id.ToLower();
            SocialNetworkService socialNetworkService = new SocialNetworkService(_connection);
            SocialNetwork socialNetwork = socialNetworkService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (socialNetwork == null)
                return Notifization.NotFound();
            //
            socialNetworkService.Remove(id);
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static string DropdownList(string id)
        {
            string result = string.Empty;
            using (var service = new SocialNetworkService())
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
        public List<SocialNetworkOption> DataOption(string languageId)
        {
            string sqlQuery = @"SELECT * FROM App_SocialNetwork WHERE Enabled = 1 ORDER BY Title ASC";
            return _connection.Query<SocialNetworkOption>(sqlQuery, new { LangID = languageId }).ToList();
        }
        //##############################################################################################################################################################################################################################################################
        public static string GetSocialNetworkNameByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;
            //
            id = id.ToLower();
            using (var service = new SocialNetworkService())
            {
                var socialNetwork = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (socialNetwork == null)
                    return string.Empty;
                //
                return socialNetwork.Title;
            }
        }

        //##############################################################################################################################################################################################################################################################
        public List<SocialNetworkIcon> SocialNetworkIconData()
        {
            List<SocialNetworkIcon> socialNetworkIcons = new List<SocialNetworkIcon>{
                    new SocialNetworkIcon(1,"Facebook", "/files/icon/facebook-32.png"),
                    new SocialNetworkIcon(2,"Google plus", "/files/icon/google-plus-32.png"),
                    new SocialNetworkIcon(3,"Instagram", "/files/icon/instagram-32.png"),
                    new SocialNetworkIcon(4,"Linkedin", "/files/icon/linkedin-32.png"),
                    new SocialNetworkIcon(5,"Pinterest", "/files/icon/pinterest-32.png"),
                    new SocialNetworkIcon(6,"Tinder", "/files/icon/tinder-32.png"),
                    new SocialNetworkIcon(7,"Twitter", "/files/icon/twitter-32.png"),
                    new SocialNetworkIcon(8,"Youtube", "/files/icon/youtube-32.png"),
                    new SocialNetworkIcon(9,"Youtube squared", "/files/icon/youtube-squared-32.png"),
                    new SocialNetworkIcon(10,"Zalo", "/files/icon/zalo-32.png")
                };
            return socialNetworkIcons;
        }


        public static string DropdownListIcon(int id)
        {
            string result = string.Empty;
            using (var service = new SocialNetworkService())
            {
                List<SocialNetworkIcon> dtList = service.SocialNetworkIconData();
                foreach (var item in dtList)
                {
                    string select = string.Empty;
                    if (item.ID == id)
                        select = "selected";
                    //
                    result += "<option value='" + item.ID + "'" + select + ">" + item.Title + "</option>";
                }
                return result;
            }
        }
        //##############################################################################################################################################################################################################################################################
        public static List<SocialNetworkIcon> SocialNetworkIcon()
        {
            SocialNetworkService socialNetworkService = new SocialNetworkService();
            return socialNetworkService.SocialNetworkIconData();
        }
        public static List<SocialNetwork> GetSocialNetworkForHome(string id)
        {
            //if (string.IsNullOrWhiteSpace(id))
            //    return string.Empty;
            //
            //id = id.ToLower();
            using (var service = new SocialNetworkService())
            {
                List<SocialNetwork> socialNetworks = service.GetAlls().Take(5).ToList();
                if (socialNetworks.Count() == 0)
                    return new List<SocialNetwork>();
                //
                return socialNetworks;
            }
        }
        //##############################################################################################################################################################################################################################################################
    }
}
