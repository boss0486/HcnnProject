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
using WebCore.Model.Enum;
using Helper.Page;
using WebCore.Model.Entities;

namespace WebCore.Services
{
    public interface IBannerService : IEntityService<Banner> { }
    public class BannerService : EntityService<Banner>, IBannerService
    {
        public BannerService() : base() { }
        public BannerService(System.Data.IDbConnection db) : base(db) { }
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
                Status = model.Status,
                TimeExpress = model.TimeExpress,
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
            string sqlQuery = @"SELECT * FROM App_Banner WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%' " + whereCondition + " ORDER BY [CreatedDate]";
            List<BannerResult> dtList = _connection.Query<BannerResult>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<BannerResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            if (result.Count == 0 && page > 1)
            {
                page -= 1;
                result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            }
            if (result.Count == 0)
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
        public ActionResult Create(BannerCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string title = model.Title;
            string summary = model.Summary;
            string imgFile = model.ImageFile;
            int loactionId = model.LocationID;
            string backLink = model.BackLink;
            int enabled = model.Enabled;
            //
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống tiêu đề");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Tiêu đề không hợp lệ");
            //
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
                //
            }
            //  
            BannerService bannerService = new BannerService(_connection);
            Banner banner = bannerService.GetAlls(m => !string.IsNullOrWhiteSpace(m.ID) && m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (banner != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");

            var id = bannerService.Create<string>(new Banner()
            {
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                ImageFile = imgFile,
                LocationID = loactionId,
                BackLink = backLink,
                Enabled = enabled,
            });
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string> { imgFile }, connection: _connection);
            //
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(BannerUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string id = model.ID;
            string title = model.Title;
            string summary = model.Summary;
            string imgFile = model.ImageFile;
            int loactionId = model.LocationID;
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
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound(MessageText.NotFound);
            //
            id = id.ToLower();
            BannerService bannerService = new BannerService(_connection);
            Banner banner = bannerService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (banner == null)
                return Notifization.NotFound(MessageText.NotFound);
            //
            Banner bannerTitle = bannerService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (bannerTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // update user information
            banner.Title = title;
            banner.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            banner.Summary = summary;
            banner.ImageFile = imgFile;
            banner.LocationID = loactionId;
            banner.BackLink = backLink;
            banner.Enabled = enabled;
            bannerService.Update(banner); 
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string> { imgFile }, connection: _connection);
            //
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public Banner GetBannerByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string sqlQuery = @"SELECT TOP (1) * FROM App_Banner WHERE ID = @Query";
            Banner banner = _connection.Query<Banner>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (banner == null)
                return null;
            //
            return banner;
        }
        public BannerResult ViewBannerByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string sqlQuery = @"SELECT TOP (1) * FROM App_Banner WHERE ID = @Query";
            BannerResult viewBanner = _connection.Query<BannerResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (viewBanner == null)
                return null;
            //
            return viewBanner;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            BannerService bannerService = new BannerService(_connection);
            Banner banner = bannerService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (banner == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            bannerService.Remove(banner.ID);
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static string DDLBanner(string id)
        {
            string result = string.Empty;
            using (var BannerService = new BannerService())
            {
                List<BannerOption> dtList = BannerService.DataOption();
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
        public List<BannerOption> DataOption()
        {
            string sqlQuery = @"SELECT * FROM App_Banner ORDER BY Title ASC";
            return _connection.Query<BannerOption>(sqlQuery, new { }).ToList();
        }
        public List<BannerType> BannerTypeData()
        {
            List<BannerType> bannerTypes = new List<BannerType>{
                    new BannerType(1, "Banner chính 1000x450"),
                    new BannerType(2, "Nội dung trên 1000x450"),
                    new BannerType(3, "Nội dung dưới 1000x450"),
                    new BannerType(4, "Nội dung trái 1000x450"),
                    new BannerType(5, "Nội dung phải 1000x450"),
                    new BannerType(6, "Dọc bên trái 1000x450"),
                    new BannerType(7, "Dọc bên phải 1000x450"),
                };
            return bannerTypes;
        }
        //##############################################################################################################################################################################################################################################################
        public static List<BannerHome> GetBannerForHome(string siteId)
        {
            using (var service = new BannerService())
            {
                string sqlQuery = @"SELECT Top (5) * FROM App_Banner WHERE Enabled = @Enabled AND LocationID = @LocationID ORDER BY [CreatedDate]";
                List<BannerHome> banners = service.Query<BannerHome>(sqlQuery, new { Enabled = Model.Enum.ModelEnum.State.ENABLED, LocationID = 1 }).ToList();
                if (banners.Count() == 0)
                    return new List<BannerHome>();
                //
                return banners;
            }
        }
    }
}
