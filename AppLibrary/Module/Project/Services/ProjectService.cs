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
    public interface IProjectService : IEntityService<Project> { }
    public class ProjectService : EntityService<Project>, IProjectService
    {
        public ProjectService() : base() { }
        public ProjectService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = $@"SELECT * FROM App_Project WHERE dbo.Uni2NONE(Title) LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY [CreatedDate]";
            List<ProjectResult> dtList = _connection.Query<ProjectResult>(sqlQuery, new { Query = query }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ProjectResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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
        public ActionResult Create(ProjectCreateModel model)
        {
            string menuId = model.MenuID;
            string groupId = model.GroupID;
            string title = model.Title;
            string htmlText = model.HtmlText;
            string strData = model.ViewDate;
            int viewTotal = model.ViewTotal;
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
            if (string.IsNullOrEmpty(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            //
            if (string.IsNullOrEmpty(groupId))
                return Notifization.Invalid("Vui lòng chọn nhóm dự án");
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
            ProjectService projectService = new ProjectService(_connection);
            Project project = projectService.GetAlls(m => m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (project != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            ProjectGroupService projectGroupService = new ProjectGroupService(_connection);
            ProjectGroup projectGroupTitle = projectGroupService.GetAlls(m => m.ID == groupId).FirstOrDefault();
            if (projectGroupTitle == null)
                return Notifization.Invalid("Nhóm tin không hợp lệ");
            // 
            string imgFile = model.ImageFile;
            var id = projectService.Create<string>(new Project()
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
        public ActionResult Update(ProjectUpdateModel model)
        {
            string id = model.ID;
            string menuId = model.MenuID;
            string categoryId = model.GroupID;
            string title = model.Title;
            string htmlText = model.HtmlText;
            string strData = model.ViewDate;
            int viewTotal = model.ViewTotal;
            //
            if (string.IsNullOrEmpty(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            //
            if (string.IsNullOrEmpty(categoryId))
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
            if (string.IsNullOrEmpty(menuId))
                return Notifization.Invalid("Vui lòng chọn danh mục");
            //
            if (string.IsNullOrEmpty(categoryId))
                return Notifization.Invalid("Vui lòng chọn nhóm dự án");
            // 
            ProjectService projectService = new ProjectService(_connection);
            Project project = projectService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (project == null)
                return Notifization.Invalid(MessageText.Invalid);

            Project projectTitle = projectService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && project.ID != id).FirstOrDefault();
            if (projectTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            //
            ProjectGroupService projectGroupService = new ProjectGroupService(_connection);
            ProjectGroup projectGroup = projectGroupService.GetAlls(m => m.ID == categoryId).FirstOrDefault();
            if (projectGroup == null)
                return Notifization.Invalid(MessageText.Invalid);
            // 
            string imgFile = project.ImageFile;
            if (!string.IsNullOrWhiteSpace(model.ImageFile))
            {
                if (model.ImageFile.Length != 36)
                    return Notifization.Invalid("Hình ảnh không hợp lệ");
                //
                imgFile = model.ImageFile;
            }
            //
            project.MenuID = menuId;
            project.GroupID = categoryId;
            project.TextID = "";
            project.Title = title;
            project.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            project.Summary = summary;
            project.ImageFile = imgFile;
            project.HtmlNote = "";
            project.HtmlText = htmlText;
            project.Tag = "";
            project.ViewTotal = viewTotal;
            project.ViewDate = viewDate;
            project.Enabled = model.Enabled;
            projectService.Update(project);
            // file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.ALONE, id, new List<string> { imgFile }, connection: _connection);
            //
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public Project GetProjectByID(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Project WHERE ID = @Query";
                Project item = _connection.Query<Project>(sqlQuery, new { Query = id }).FirstOrDefault();
                return item;
            }
            catch
            {
                return null;
            }
        }
        public ProjectResult ViewProjectByID(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_Project WHERE ID = @Query";
            ProjectResult projectResult = _connection.Query<ProjectResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (projectResult == null)
                return null;
            //
            return projectResult;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.Invalid(MessageText.Invalid);
            //
            id = id.ToLower();
            ProjectService projectService = new ProjectService(_connection);
            Project project = projectService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (project == null)
                return Notifization.NotFound();
            // delete 
            projectService.Remove(project.ID);
            // delete file  
            AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
            attachmentIngredientService.RemoveAllFileByForID(id, connection: _connection);
            //
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static List<ProjectHome> GetProjectForHome(string id)
        {
            using (var service = new BannerService())
            {
                string sqlQuery = @"SELECT TOP (12) * FROM App_Project ORDER BY ViewDate DESC";
                List<ProjectHome> items = service.Query<ProjectHome>(sqlQuery).ToList();
                return items;
            }
        }
        public static IEnumerable<ProjectHome> GetProjectHomeList(string alias, int page = 1)
        {
            string menuId = string.Empty;
            string categoryId = string.Empty;
            string whereCondition = string.Empty;
            //
            ProjectService service = new ProjectService();
            MenuBar menuBar = MenuService.GetMenuBarByAlias(alias);
            if (menuBar != null)
            {
                menuId = menuBar.ID;
                if (!string.IsNullOrWhiteSpace(alias))
                    whereCondition = " AND MenuID = @MenuID";
            }
            //

            //
            string sqlQuery = $@"SELECT TOP (15) * FROM App_Project WHERE ID IS NOT NULL {whereCondition} ORDER BY ViewDate DESC";
            IEnumerable<ProjectHome> items = service.Query<ProjectHome>(sqlQuery, new { MenuID = menuId, CategoryID = categoryId }).ToList();
            items = items.ToPagedList(page, 5);
            return items;

        }

        public static ProjectResult GetProjectByAlias(string alias)
        {

            if (string.IsNullOrWhiteSpace(alias))
                return new ProjectResult();
            //
            using (var service = new ProjectService())
            {
                string sqlQuery = @"SELECT TOP (1) * FROM App_Project WHERE Alias = @Alias";
                ProjectResult item = service.Query<ProjectResult>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }

        public static List<ProjectHome> GetProjectOther(string categoryId, string id)
        {
            using (var service = new ProjectService())
            {
                List<ProjectHome> ProjectHomes = new List<ProjectHome>();
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(categoryId))
                    whereCondition += " AND CategoryID = @CategoryID";
                //
                if (!string.IsNullOrWhiteSpace(id))
                    whereCondition += " AND ID != @ID";
                //
                string sqlQueryAll = $@"SELECT TOP(20) * FROM App_Project WHERE ID IS NOT NULL {whereCondition} ORDER BY CategoryID, ViewDate";
                ProjectHomes = service.Query<ProjectHome>(sqlQueryAll, new { CategoryID = categoryId, ID = id }).ToList();

                return ProjectHomes;
            }
        }
    }
    //##############################################################################################################################################################################################################################################################
}