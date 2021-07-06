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
    public interface IDepartmentPartService : IEntityService<DepartmentPart> { }
    public class DepartmentPartService : EntityService<DepartmentPart>, IDepartmentPartService
    {
        public DepartmentPartService() : base() { }
        public DepartmentPartService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = @"SELECT * FROM App_DepartmentPart WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%'" + whereCondition + " ORDER BY [CreatedDate]";
            List<DepartmentPartResult> dtList = _connection.Query<DepartmentPartResult>(sqlQuery, new
            {
                Query = Helper.Page.Library.FormatNameToUni2NONE(query),
                SiteID = Helper.Current.UserLogin.SiteID
            }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<DepartmentPartResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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
        public ActionResult Create(DepartmentPartCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            //
            string title = model.Title;
            string summary = model.Summary;
            int enabled = model.Enabled;
            string departmentId = model.DepartmentID;
            //
            if (string.IsNullOrWhiteSpace(departmentId))
                return Notifization.Invalid("Vui lòng chọn phòng ban");
            //
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống bộ phận");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Bộ phận không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Bộ phận giới hạn [2-80] ký tự");
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
            DepartmentPartService _departmentPartService = new DepartmentPartService(_connection);
            DepartmentService _departmentService = new DepartmentService(_connection);
            Department department = _departmentService.GetAlls(m => m.ID == departmentId).FirstOrDefault();
            if (department == null)
                return Notifization.Invalid("Phòng ban không hợp lệ");
            //
            DepartmentPart _departmentPart = _departmentPartService.GetAlls(m =>
            !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower()
            && m.DepartmentID == departmentId
            && m.SiteID == Helper.Current.UserLogin.SiteID).FirstOrDefault();
            if (_departmentPart != null)
                return Notifization.Invalid("Tên Bộ phận đã được sử dụng");
            //
            _departmentPartService.Create<string>(new DepartmentPart()
            {
                DepartmentID = departmentId,
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                Enabled = enabled,
            });
            //sort
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(DepartmentPartUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid();

            string title = model.Title;
            string summary = model.Summary;
            int enabled = model.Enabled;
            string departmentId = model.DepartmentID;
            //
            if (string.IsNullOrWhiteSpace(departmentId))
                return Notifization.Invalid("Vui lòng chọn phòng ban");
            //
            if (string.IsNullOrWhiteSpace(title))
                return Notifization.Invalid("Không được để trống bộ phận");
            title = title.Trim();
            if (!Validate.TestText(title))
                return Notifization.Invalid("Bộ phận không hợp lệ");
            if (title.Length < 2 || title.Length > 80)
                return Notifization.Invalid("Bộ phận giới hạn [2-80] ký tự");
            // summary valid               
            if (!string.IsNullOrWhiteSpace(summary))
            {
                summary = summary.Trim();
                if (!Validate.TestText(summary))
                    return Notifization.Invalid("Mô tả không hợp lệ");
                if (summary.Length < 1 || summary.Length > 120)
                    return Notifization.Invalid("Mô tả giới hạn [1-120] ký tự");
            }
            DepartmentPartService departmentPartService = new DepartmentPartService(_connection);
            DepartmentService _departmentService = new DepartmentService(_connection);
            Department department = _departmentService.GetAlls(m => m.ID == departmentId).FirstOrDefault();
            if (department == null)
                return Notifization.Invalid("Phòng ban không hợp lệ");
            //
            string id = model.ID.ToLower();
            DepartmentPart departmentPart = departmentPartService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (departmentPart == null)
                return Notifization.NotFound(MessageText.NotFound);
            //
            DepartmentPart _departmentPartTitle = departmentPartService.GetAlls(m =>
            !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower()
            && m.DepartmentID == departmentId
            && m.SiteID == Helper.Current.UserLogin.SiteID
            && m.ID != id).FirstOrDefault();
            if (_departmentPartTitle != null)
                return Notifization.Invalid("Bộ phận đã được sử dụng");
            // update user information
            departmentPart.DepartmentID = departmentId;
            departmentPart.Title = title;
            departmentPart.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            departmentPart.Summary = summary;
            departmentPart.Enabled = enabled;
            departmentPartService.Update(departmentPart);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public DepartmentPart GetDepartmentPartByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_DepartmentPart WHERE ID = @Query";
                return _connection.Query<DepartmentPart>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public DepartmentPartResult ViewDepartmentPartByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_DepartmentPart WHERE ID = @Query";
                return _connection.Query<DepartmentPartResult>(sqlQuery, new { Query = id }).FirstOrDefault();
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
            DepartmentPartService _departmentPartService = new DepartmentPartService(_connection);
            DepartmentPart _departmentPart = _departmentPartService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (_departmentPart == null)
                return Notifization.NotFound();
            //
            _departmentPartService.Remove(_departmentPart.ID);
            return Notifization.Success(MessageText.DeleteSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public static string DropdownList(string id)
        {
            string result = string.Empty;
            using (var service = new DepartmentPartService())
            {
                List<DepartmentPartOption> dtList = service.DataOption(null);
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
        public List<DepartmentPartOption> DataOption(string languageId)
        {
            string sqlQuery = @"SELECT * FROM App_DepartmentPart WHERE SiteID = @SiteID AND Enabled =1 ORDER BY Title ASC";
            return _connection.Query<DepartmentPartOption>(sqlQuery, new
            {
                LangID = languageId,
                SiteID = Helper.Current.UserLogin.SiteID
            }).ToList();
        }
        //##############################################################################################################################################################################################################################################################
        public List<DepartmentPartOption> GetOptionByCate(string departmentId)
        {
            if (string.IsNullOrWhiteSpace(departmentId))
                return null;
            //
            departmentId = departmentId.ToLower();
            string sqlQuery = @"SELECT * FROM App_DepartmentPart WHERE DepartmentID = @DepartmentID AND SiteID = @SiteID AND Enabled =1";
            List<DepartmentPartOption> departmentParts = _connection.Query<DepartmentPartOption>(sqlQuery, new
            {
                DepartmentID = departmentId,
                SiteID = Helper.Current.UserLogin.SiteID
            }).ToList();
            if (departmentParts.Count == 0)
                return null;
            //
            return departmentParts;
        }

        public static string GetDepartmentPartNameByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;
            //
            id = id.ToLower();
            using (var service = new DepartmentPartService())
            {
                DepartmentPart item = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (item == null)
                    return string.Empty;
                //
                return item.Title;
            }
        }
        //##############################################################################################################################################################################################################################################################
    }
}
