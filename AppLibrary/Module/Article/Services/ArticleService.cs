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
    public interface IArticleService : IEntityService<Article> { }
    public class ArticleService : EntityService<Article>, IArticleService
    {
        public ArticleService() : base() { }
        public ArticleService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = $@"SELECT * FROM App_Article WHERE dbo.Uni2NONE(Title) LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY [CreatedDate]";
            var dtList = _connection.Query<ArticleResult>(sqlQuery, new { Query = query }).ToList();
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
        public ActionResult Create(ArticleCreateModel model)
        {
            string menuId = model.MenuID;
            string groupId = model.GroupID;
            string title = model.Title;
            string htmlText = model.HtmlText;
            string strData = model.ViewDate;
            int viewTotal = model.ViewTotal;
            //
            if (string.IsNullOrEmpty(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            //
            if (string.IsNullOrEmpty(groupId))
                return Notifization.Invalid("Vui lòng chọn nhóm bài viết");
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
                if (htmlText.Length < 1 || htmlText.Length > 5000)
                    return Notifization.Invalid("Nội dung giới hạn từ 0-> 5000 ký tự");
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
            ArticleService articleService = new ArticleService(_connection);
            Article article = articleService.GetAlls(m => m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (article != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            ArticleGroupService articleGroupService = new ArticleGroupService(_connection);
            ArticleGroup articleGroup = articleGroupService.GetAlls(m => m.ID == groupId).FirstOrDefault();
            if (articleGroup == null)
                return Notifization.Invalid("Nhóm tin không hợp lệ");
            // 
            string imgFile = model.ImageFile;
            var id = articleService.Create<string>(new Article()
            {
                MenuID = menuId,
                GroupID = groupId,
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                ImageFile = imgFile,
                HtmlNote = string.Empty,
                HtmlText = htmlText,
                Tag = model.Tag,
                ViewTotal = viewTotal,
                ViewDate = viewDate,
                Enabled = model.Enabled,
            });
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string> { imgFile }, connection: _connection);
            //
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(ArticleUpdateModel model)
        {
            string id = model.ID;
            string menuId = model.MenuID;
            string groupId = model.GroupID;
            string title = model.Title;
            string htmlText = model.HtmlText;
            string strData = model.ViewDate;
            int viewTotal = model.ViewTotal;
            //
            if (string.IsNullOrEmpty(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            //
            if (string.IsNullOrEmpty(groupId))
                return Notifization.Invalid("Vui lòng chọn nhóm bài viết");
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
                if (htmlText.Length < 1 || htmlText.Length > 15000)
                    return Notifization.Invalid("Nội dung giới hạn từ 0-> 5000 ký tự");
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
            ArticleService articleService = new ArticleService(_connection);
            Article article = articleService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (article == null)
                return Notifization.Invalid(MessageText.Invalid);

            Article articleTitle = articleService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && article.ID != id).FirstOrDefault();
            if (articleTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            //
            ArticleGroupService articleGroupService = new ArticleGroupService(_connection);
            var articleGroup = articleGroupService.GetAlls(m => m.ID == groupId).FirstOrDefault();
            if (articleGroup == null)
                return Notifization.Invalid(MessageText.Invalid);
            // 
            string imgFile = article.ImageFile;
            if (!string.IsNullOrWhiteSpace(model.ImageFile))
            {
                if (model.ImageFile.Length != 36)
                    return Notifization.Invalid("Hình ảnh không hợp lệ");
                //
                imgFile = model.ImageFile;
            }
            //
            article.MenuID = menuId;
            article.GroupID = groupId;
            article.TextID = "";
            article.Title = title;
            article.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            article.Summary = summary;
            article.ImageFile = imgFile;
            article.HtmlNote = "";
            article.HtmlText = htmlText;
            article.Tag = "";
            article.ViewTotal = viewTotal;
            article.ViewDate = viewDate;
            article.Enabled = model.Enabled;
            articleService.Update(article);
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string> { imgFile }, connection: _connection);
            //
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public Article GetArticleByID(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Article WHERE ID = @Query";
                Article item = _connection.Query<Article>(sqlQuery, new { Query = id }).FirstOrDefault();
                return item;
            }
            catch
            {
                return null;
            }
        }
        public ArticleResult ViewArticleByID(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_Article WHERE ID = @Query";
            ArticleResult articleResult = _connection.Query<ArticleResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (articleResult == null)
                return null;
            //
            return articleResult;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            ArticleService articleService = new ArticleService(_connection);
            Article article = articleService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (article == null)
                return Notifization.NotFound();
            // delete 
            articleService.Remove(article.ID);
            // delete file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.RemoveAllFileByForID(id, connection: _connection);
            //
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static List<ArticleHome> GetArticleForHome(string id)
        {
            using (var service = new BannerService())
            {
                string sqlQuery = @"SELECT TOP (10) * FROM App_Article ORDER BY ViewDate DESC";
                List<ArticleHome> items = service.Query<ArticleHome>(sqlQuery).ToList();
                return items;
            }
        }
        public static IEnumerable<ArticleHome> GetArticleByCategory(string categoryId, int page = 1)
        {
            using (var service = new ArticleService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(categoryId))
                    whereCondition = " AND CategoryID = @CategoryID";
                //
                string sqlQuery = $@"SELECT TOP (15) * FROM App_Article WHERE ID IS NOT NULL {whereCondition} ORDER BY ViewDate DESC";
                IEnumerable<ArticleHome> items = service.Query<ArticleHome>(sqlQuery, new { CategoryID = categoryId }).ToList();
                items = items.ToPagedList(page, 5);
                return items;
            }
        }
        public static IEnumerable<ArticleHome> GetArticleByMenu(string menuId, int page = 1)
        {
            using (var service = new ProductService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(menuId))
                    whereCondition = " AND MenuID = @MenuID";
                //
                string sqlQuery = $@"SELECT TOP (30) * FROM App_Article WHERE Enabled = @Enabled {whereCondition} ORDER BY ViewDate DESC";
                IEnumerable<ArticleHome> items = service.Query<ArticleHome>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED, MenuID = menuId }).OrderBy(m => m.ViewDate).ToList();
                return items.ToPagedList(page, PagedRender.PAGENUMBER);
            }
        }
        public static ArticleResult GetArticleByAlias(string alias)
        {

            if (string.IsNullOrWhiteSpace(alias))
                return new ArticleResult();
            //
            using (var service = new ArticleService())
            {
                string sqlQuery = @"SELECT TOP (1) * FROM App_Article WHERE Alias = @Alias";
                ArticleResult item = service.Query<ArticleResult>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

        public static List<ArticleHome> GetArticleOther(string categoryId, string id)
        {
            using (var service = new ArticleService())
            {
                List<ArticleHome> articleHomes = new List<ArticleHome>();
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(categoryId))
                    whereCondition += " AND CategoryID = @CategoryID";
                //
                if (!string.IsNullOrWhiteSpace(id))
                    whereCondition += " AND ID != @ID";
                //
                string sqlQueryAll = $@"SELECT TOP(20) * FROM App_Article WHERE ID IS NOT NULL {whereCondition} ORDER BY CategoryID, ViewDate";
                articleHomes = service.Query<ArticleHome>(sqlQueryAll, new { CategoryID = categoryId, ID = id }).ToList();

                return articleHomes;
            }
        }
    }
    //##############################################################################################################################################################################################################################################################
}