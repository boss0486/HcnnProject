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
    public interface IAboutService : IEntityService<About> { }
    public class AboutService : EntityService<About>, IAboutService
    {
        public AboutService() : base() { }
        public AboutService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = $@"SELECT * FROM App_About WHERE dbo.Uni2NONE(Title) LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY [CreatedDate]";
            var dtList = _connection.Query<AboutResult>(sqlQuery, new { Query = query }).ToList();
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
        public ActionResult Create(AboutCreateModel model)
        {
            string menuId = model.MenuID;
            string title = model.Title;
            string htmlText = model.HtmlText;
            string strData = model.ViewDate;
            int viewTotal = model.ViewTotal;
            string summary = model.Summary;
            int isShow = model.IsShow;
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
                    return Notifization.Invalid("Nội dung giới hạn từ 0-> 150 000 ký tự");
            }
            DateTime? viewDate = null;
            //
            if (!string.IsNullOrWhiteSpace(strData))
            {
                if (!Validate.TestDate(strData))
                    return Notifization.Invalid("Ngày hiển thị không hợp lệ");
                //
                viewDate = Helper.TimeData.TimeFormat.FormatToServerDateTime(strData);
            }
            //
            if (viewTotal < 0 || viewTotal > 1000000)
                return Notifization.Invalid("Lượt xem giới hạn từ [2-1 000 000]");
            //
            if (string.IsNullOrWhiteSpace(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            // 
            AboutService aboutService = new AboutService(_connection);
            About aboutTitle = aboutService.GetAlls(m => m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (aboutTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            string imgFile = model.ImageFile;
            var id = aboutService.Create<string>(new About()
            {
                MenuID = menuId,
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                ImageFile = imgFile,
                HtmlNote = string.Empty,
                HtmlText = htmlText,
                Tag = model.Tag,
                ViewTotal = viewTotal,
                ViewDate = viewDate,
                IsShow = Convert.ToBoolean(isShow),
                Enabled = model.Enabled,
            });
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string> { imgFile }, connection: _connection);
            //
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(AboutUpdateModel model)
        {
            string id = model.ID;
            string menuId = model.MenuID;
            string title = model.Title;
            string htmlText = model.HtmlText;
            string strData = model.ViewDate;
            int viewTotal = model.ViewTotal;
            int isShow = model.IsShow;
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
                    return Notifization.Invalid("Nội dung giới hạn từ 0-> 150 000 ký tự");
            }
            DateTime? viewDate = null;
            //
            if (!string.IsNullOrWhiteSpace(strData))
            {
                if (!Validate.TestDate(strData))
                    return Notifization.Invalid("Ngày hiển thị không hợp lệ");
                //
                viewDate = Helper.TimeData.TimeFormat.FormatToServerDateTime(strData);
            }
            //
            if (viewTotal < 0 || viewTotal > 1000000)
                return Notifization.Invalid("Lượt xem giới hạn từ [2-1 000 000]");
            // 
            if (string.IsNullOrWhiteSpace(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            // 
            AboutService aboutService = new AboutService(_connection);
            About about = aboutService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (about == null)
                return Notifization.Invalid(MessageText.Invalid);

            About aboutTitle = aboutService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
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
            about.MenuID = menuId;
            about.TextID = "";
            about.Title = title;
            about.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            about.Summary = summary;
            about.ImageFile = imgFile;
            about.HtmlNote = "";
            about.HtmlText = htmlText;
            about.Tag = "";
            about.ViewTotal = viewTotal;
            about.ViewDate = viewDate;
            about.IsShow = Convert.ToBoolean(isShow);
            about.Enabled = model.Enabled;
            aboutService.Update(about);
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string> { imgFile }, connection: _connection);
            //
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public About GetAboutByID(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_About WHERE ID = @Query";
                About item = _connection.Query<About>(sqlQuery, new { Query = id }).FirstOrDefault();
                return item;
            }
            catch
            {
                return null;
            }
        }
        public AboutResult ViewAboutByID(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_About WHERE ID = @Query";
            AboutResult AboutResult = _connection.Query<AboutResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (AboutResult == null)
                return null;
            //
            return AboutResult;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            AboutService aboutService = new AboutService();
            About about = aboutService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (about == null)
                return Notifization.NotFound();
            // delete 
            aboutService.Remove(about.ID);
            // delete file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.RemoveAllFileByForID(id, connection: _connection);
            //
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static List<AboutHome> GetAboutForHome(string id)
        {
            using (var service = new BannerService())
            {
                string sqlQuery = @"SELECT TOP (10) * FROM App_About ORDER BY ViewDate DESC";
                List<AboutHome> items = service.Query<AboutHome>(sqlQuery).ToList();
                return items;
            }
        }
        public static AboutResult GetAboutDefault()
        {
            using (var service = new AboutService())
            {
                string sqlQuery = @"SELECT TOP (1) * FROM App_About WHERE IsShow = 1 AND Enabled = @Enabled";
                AboutResult item = service.Query<AboutResult>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED }).FirstOrDefault();
                return item;
            }
        }
        public static IEnumerable<AboutHome> GetAboutByMenu(string menuId, string id)
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
                string sqlQuery = $@"SELECT TOP (10) * FROM App_About WHERE Enabled = @Enabled {whereCondition} ORDER BY ViewDate DESC";
                IEnumerable<AboutHome> items = service.Query<AboutHome>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED, MenuID = menuId, ID = id }).OrderBy(m => m.ViewDate).ToList();
                return items;
            }
        }

        public static AboutResult GetAboutByAlias(string alias)
        {

            if (string.IsNullOrWhiteSpace(alias))
                return new AboutResult();
            //
            using (var service = new AboutService())
            {
                string sqlQuery = @"SELECT TOP (1) * FROM App_About WHERE Alias = @Alias";
                AboutResult item = service.Query<AboutResult>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

        public static List<AboutHome> GetAboutOther(string menuId, string id)
        {
            using (var service = new AboutService())
            {
                List<AboutHome> AboutHomes = new List<AboutHome>();
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(menuId))
                    whereCondition += " AND MenuID = @MenuID";
                //
                if (!string.IsNullOrWhiteSpace(id))
                    whereCondition += " AND ID != @ID";
                //
                string sqlQueryAll = $@"SELECT TOP(20) * FROM App_About WHERE ID IS NOT NULL {whereCondition} ORDER BY CategoryID, ViewDate";
                AboutHomes = service.Query<AboutHome>(sqlQueryAll, new { MenuID = menuId, ID = id }).ToList();

                return AboutHomes;
            }
        }
    }
}