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
    public interface IDepartmentService : IEntityService<Department> { }
    public class DepartmentService : EntityService<Department>, IDepartmentService
    {
        public DepartmentService() : base() { }
        public DepartmentService(System.Data.IDbConnection db) : base(db) { }
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
            whereCondition += " AND SiteID = @SiteID ";
            #endregion
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT * FROM App_Department WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%'" + whereCondition + " ORDER BY [CreatedDate]";
            List<DepartmentResult> dtList = _connection.Query<DepartmentResult>(sqlQuery, new
            {
                Query = Helper.Page.Library.FormatNameToUni2NONE(query),
                SiteID = Helper.Current.UserLogin.SiteID
            }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<DepartmentResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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
        public ActionResult Create(DepartmentCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string title = model.Title;
            string summary = model.Summary;
            int enabled = model.Enabled;
            //
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống phòng ban");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Phòng ban không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Phòng ban giới hạn [2-80] ký tự");
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
            DepartmentService _departmentService = new DepartmentService(_connection);
            Department _department = _departmentService.GetAlls(m =>
            !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower()
            && m.SiteID == Helper.Current.UserLogin.SiteID).FirstOrDefault();
            if (_department != null)
                return Notifization.Invalid("Tên phòng ban đã được sử dụng");
            //
            _departmentService.Create<string>(new Department()
            {
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                Enabled = enabled,
            });
            //sort
            return Notifization.Success(MessageText.CreateSuccess);
        }
        public ActionResult Update(DepartmentUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid();

            string title = model.Title;
            string summary = model.Summary;
            int enabled = model.Enabled;
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống phòng ban");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Phòng ban không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Phòng ban giới hạn [2-80] ký tự");
            // summary valid               
            if (!string.IsNullOrWhiteSpace(summary))
            {
                summary = summary.Trim();
                if (!Validate.TestText(summary))
                    return Notifization.Invalid("Mô tả không hợp lệ");
                if (summary.Length < 1 || summary.Length > 120)
                    return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
            }
            DepartmentService _departmentService = new DepartmentService(_connection);
            string id = model.ID.ToLower();
            Department _department = _departmentService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (_department == null)
                return Notifization.NotFound(MessageText.NotFound);
            //
            Department _departmentTitle = _departmentService.GetAlls(m =>
            !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower()
            && m.SiteID == Helper.Current.UserLogin.SiteID
            && m.ID != id).FirstOrDefault();
            if (_departmentTitle != null)
                return Notifization.Invalid("Phòng ban đã được sử dụng");
            // update user information
            _department.Title = title;
            _department.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            _department.Summary = summary;
            _department.Enabled = enabled;
            _departmentService.Update(_department);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Notifization.NotFound();
            //
            id = id.ToLower();
            DepartmentService _departmentService = new DepartmentService(_connection);
            Department _department = _departmentService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (_department == null)
                return Notifization.NotFound();
            //
            _departmentService.Remove(_department.ID);
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        public Department GetDepartmentByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Department WHERE ID = @Query";
                return _connection.Query<Department>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public DepartmentResult ViewDepartmentByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Department WHERE ID = @Query";
                return _connection.Query<DepartmentResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        //##############################################################################################################################################################################################################################################################
        public static string DropdownList(string id)
        {
            string result = string.Empty;
            using (var service = new DepartmentService())
            {
                List<DepartmentOption> dtList = service.DataOption(null);
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
        public static string DDLDepartMentForReport(string id)
        {
            string result = string.Empty;
            string whereCondition = string.Empty;
            using (var service = new DepartmentService())
            {
                if (Helper.Current.UserLogin.IsAdminInApplication)
                {
                    // something here
                }
                else
                { 
                    UserSettingService userSettingService = new UserSettingService();
                    UserSetting userSetting = userSettingService.GetAlls(m => m.UserID == Helper.Current.UserLogin.IdentifierID).FirstOrDefault();
                    if (userSetting == null)
                        return string.Empty;
                    // 
                    whereCondition += $" AND DepartmentID = '{userSetting.DepartmentID}'";
                }
                string sqlQuery = $@"SELECT * FROM App_Department WHERE Enabled = 1 {whereCondition} AND SiteID = @SiteID ORDER BY Title ASC";

                List<DepartmentOption> dtList = service.Query<DepartmentOption>(sqlQuery, new
                {
                    SiteID = Helper.Current.UserLogin.SiteID
                }).ToList(); 
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
        public List<DepartmentOption> DataOption(string languageId)
        {
            string sqlQuery = @"SELECT * FROM App_Department WHERE Enabled = 1 AND SiteID = @SiteID ORDER BY Title ASC";
            return _connection.Query<DepartmentOption>(sqlQuery, new
            {
                LangID = languageId,
                SiteID = Helper.Current.UserLogin.SiteID
            }).ToList();
        }
        //##############################################################################################################################################################################################################################################################
        public static string GetDepartmentNameByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;
            //
            id = id.ToLower();
            using (var service = new DepartmentService())
            {
                Department item = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (item == null)
                    return string.Empty;
                //
                return item.Title;
            }
        }

        public List<DepartmentLevelOption> DepartmentLevelOption()
        {
            List<DepartmentLevelOption> dtList = new List<DepartmentLevelOption>
                {
                    new DepartmentLevelOption()
                     {
                        ID = 1,
                        Title ="Trưởng phòng"
                     },
                    new DepartmentLevelOption()
                     {
                        ID = 2,
                        Title ="Phó phòng",
                     },
                    new DepartmentLevelOption()
                     {
                        ID = 3,
                        Title ="Nhân viên",
                     }
                };
            return dtList;
        }
        public static string DpmPosition(int id)
        {
            string result = string.Empty;
            using (var service = new DepartmentService())
            {
                List<DepartmentLevelOption> dtList = service.DepartmentLevelOption();
                if (dtList.Count > 0)
                {
                    foreach (var item in dtList)
                    {
                        string select = string.Empty;
                        if (item.ID == id)
                            select = "selected";
                        //
                        result += $"<option value= '{item.ID}' {select}>{item.Title}</option>";
                    }
                }
                return result;
            }
        }
        //##############################################################################################################################################################################################################################################################
    }
}
