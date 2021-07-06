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
using WebCore.Model.Entities;
using Helper.Page;
using WebCore.Model.Enum;

namespace WebCore.Services
{
    public interface IProjectGroupService : IEntityService<ProjectGroup> { }
    public class ProjectGroupService : EntityService<ProjectGroup>, IProjectGroupService
    {
        public ProjectGroupService() : base() { }
        public ProjectGroupService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = $@"SELECT * FROM App_ProjectGroup WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%' {whereCondition} ORDER BY [CreatedDate]";


            List<ProjectGroupResult> dtList = _connection.Query<ProjectGroupResult>(sqlQuery, new { Query = Helper.Page.Library.FormatToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<ProjectGroupResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
            if (result.Count == 0 && page > 1)
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
            return Notifization.Data(MessageText.Success + "::" + sqlQuery, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
        }

        //##############################################################################################################################################################################################################################################################
        public ActionResult Create(ProjectGroupCreateModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    if (model == null)
                        return Notifization.Invalid();

                    string title = model.Title;
                    string summary = model.Summary;
                    if (string.IsNullOrEmpty(title))
                        return Notifization.Invalid("Không được để trống tiêu đề");
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tiêu đề không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tiêu đề giới hạn [2-80] ký tự");
                    // summary valid               
                    if (!string.IsNullOrEmpty(summary))
                    {
                        summary = summary.Trim();
                        if (!Validate.TestText(summary))
                            return Notifization.Invalid("Mô tả không hợp lệ");
                        //
                        if (summary.Length < 1 || summary.Length > 120)
                            return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
                    }
                    ProjectGroupService projectGroupService = new ProjectGroupService(_connection);
                    ProjectGroup projectGroupTitle = projectGroupService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == model.Title.ToLower(), transaction: _transaction).FirstOrDefault();
                    if (projectGroupTitle != null)
                        return Notifization.Invalid("Tiêu đề đã được sử dụng");
                    //
                    string id = projectGroupService.Create<string>(new ProjectGroup()
                    {
                        Title = title,
                        Alias = Helper.Page.Library.FormatToUni2NONE(title),
                        Summary = summary,
                        LanguageID = Helper.Current.UserLogin.LanguageID,
                        Enabled = model.Enabled,
                    }, transaction: _transaction);
                    string temp = string.Empty;

                    //sort
                    _transaction.Commit();
                    return Notifization.Success(MessageText.CreateSuccess);
                }
                catch
                {
                    _transaction.Rollback();
                    return Notifization.NotService;
                }
            }
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(ProjectGroupUpdateModel model)
        {

            if (model == null)
                return Notifization.Invalid();
            //
            string id = model.ID.ToLower();
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
            if (!string.IsNullOrEmpty(summary))
            {
                summary = summary.Trim();
                if (!Validate.TestText(summary))
                    return Notifization.Invalid("Mô tả không hợp lệ");
                if (summary.Length < 1 || summary.Length > 120)
                    return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
            }
            ProjectGroupService projectGroupService = new ProjectGroupService(_connection);

            ProjectGroup projectGroup = projectGroupService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (projectGroup == null)
                return Notifization.NotFound(MessageText.NotFound);
            //
            ProjectGroup projectGroupTitle = projectGroupService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && projectGroup.ID != id).FirstOrDefault();
            if (projectGroupTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // update user information
            projectGroup.Title = title;
            projectGroup.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            projectGroup.Summary = model.Summary;
            projectGroup.Enabled = model.Enabled;
            projectGroupService.Update(projectGroup);
            return Notifization.Success(MessageText.UpdateSuccess);

        }

        public ProjectGroup GetProjectGroupByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_ProjectGroup WHERE ID = @Query";
                return _connection.Query<ProjectGroup>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public ProjectGroupResult ViewProjectGroupByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_ProjectGroup WHERE ID = @Query";
                return _connection.Query<ProjectGroupResult>(sqlQuery, new { Query = id }).FirstOrDefault();
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
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    ProjectGroupService projectGroupService = new ProjectGroupService(_connection);
                    ProjectGroup projectGroup = projectGroupService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (projectGroup == null)
                        return Notifization.NotFound();
                    //
                    projectGroupService.Remove(projectGroup.ID, transaction: _transaction);
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
        //##############################################################################################################################################################################################################################################################
        public ActionResult Details(string Id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Id))
                    return Notifization.NotFound(MessageText.Invalid);
                //
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT * FROM App_ProjectGroup WHERE ID = @ID";
                ProjectGroupResult item = _connection.Query<ProjectGroupResult>(sqlQuery, new { ID = Id }).FirstOrDefault();
                if (item == null)
                    return Notifization.NotFound(MessageText.NotFound);
                //
                return Notifization.Data(MessageText.Success, data: item, role: null, paging: null);
            }
            catch
            {
                return Notifization.NotService;
            }
        }
        //##############################################################################################################################################################################################################################################################
        public static string DDLProjectGroup(string id)
        {
            try
            {
                string result = string.Empty;
                using (var service = new ProjectGroupService())
                {
                    List<ProjectGroupOption> dtList = service.DataOption(id);
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
            catch
            {
                return string.Empty;
            }
        }
        public List<ProjectGroupOption> DataOption(string langID)
        {
            try
            {
                string sqlQuery = @"SELECT * FROM App_ProjectGroup ORDER BY Title ASC";
                return _connection.Query<ProjectGroupOption>(sqlQuery, new { LangID = langID }).ToList();
            }
            catch
            {
                return new List<ProjectGroupOption>();
            }
        }

        public static string GetProjectGroupName(string id)
        {
            using (var service = new ProjectGroupService())
            {
                ProjectGroup ProjectGroup = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (ProjectGroup == null)
                    return string.Empty;
                // 
                return ProjectGroup.Title;
            }
        }

        //##############################################################################################################################################################################################################################################################
        public static List<ProjectGroup> GetProjectGroupList()
        {
            using (var service = new ProjectGroupService())
            {
                string sqlQuery = @"SELECT TOP 5 * FROM App_ProjectGroup WHERE Enabled = @Enabled ORDER BY Title";
                List<ProjectGroup> items = service.Query<ProjectGroup>(sqlQuery, new { Enabled = (int)ModelEnum.State.ENABLED }).ToList();
                return items;
            }
        }

        public static List<ProjectGroup> GetProjectGroupForHome()
        {
            using (var service = new ProjectGroupService())
            {
                string sqlQuery = @"SELECT TOP (5) * FROM App_ProjectGroup WHERE IsHome = 1 ORDER BY Title";
                List<ProjectGroup> items = service.Query<ProjectGroup>(sqlQuery).ToList();
                return items;
            }
        }
        public static ProjectGroup GetProjectGroupByAlias(string alias)
        {
            using (var service = new ProjectGroupService())
            {
                string sqlQuery = @"SELECT TOP 1 * FROM App_ProjectGroup WHERE Alias = @Alias";
                ProjectGroup item = service.Query<ProjectGroup>(sqlQuery, new { Alias = alias }).FirstOrDefault();
                return item;
            }
        }
    }
}
