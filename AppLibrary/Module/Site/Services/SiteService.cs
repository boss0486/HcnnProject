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
using Helper.File;

namespace WebCore.Services
{
    public interface ISiteService : IEntityService<Site> { }
    public class SiteService : EntityService<Site>, ISiteService
    {
        public SiteService() : base() { }
        public SiteService(System.Data.IDbConnection db) : base(db) { }
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
            string sqlQuery = @"SELECT * FROM App_Site WHERE dbo.Uni2NONE(Title) LIKE N'%'+ @Query +'%'" + whereCondition + " ORDER BY [CreatedDate]";
            List<SiteResult> dtList = _connection.Query<SiteResult>(sqlQuery, new { Query = Helper.Page.Library.FormatNameToUni2NONE(query) }).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            List<SiteResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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
        public ActionResult Create(SiteCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            // 
            string parentId = model.ParentID;
            string title = model.Title;
            string codeID = string.Empty;
            string summary = model.Summary;
            string iconFile = model.IconFile;
            string imageFile = model.ImageFile;
            string email = model.Email;
            string fax = model.Fax;
            string phone = model.Phone;
            string tel = model.Tel;
            string workTime = model.WorkTime;
            string address = model.Address;
            string gmaps = model.Gmaps;
            int type = model.Type;
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
            SiteService siteService = new SiteService(_connection);
            Site site = siteService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower()).FirstOrDefault();
            if (site != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            //
            if (!string.IsNullOrWhiteSpace(parentId))
            {
                Site siteParent = siteService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (siteParent == null)
                    return Notifization.Invalid("Danh mục không hợp lệ");
                //
            }
            //  
            SiteTypeOption siteTypeOption = SiteService.TypeData().Where(m => m.ID == type).FirstOrDefault();
            if (siteTypeOption == null)
                return Notifization.Invalid("Loại tổ chức không hợp lệ");
            // 
            string id = siteService.Create<string>(new Site()
            {
                ParentID = parentId,
                Title = title,
                Alias = Helper.Page.Library.FormatToUni2NONE(title),
                Summary = summary,
                IconFile = iconFile,
                ImageFile = imageFile,
                Email = email,
                Fax = fax,
                Phone = phone,
                Tel = tel,
                WorkTime = workTime,
                Address = address,
                Gmaps = gmaps,
                Type = type,
                Enabled = enabled
            });
            // update part
            string strPath = string.Empty;
            Site workPath = siteService.GetAlls(m => m.ID == parentId).FirstOrDefault();
            if (workPath != null)
                strPath = workPath.Path + "/" + id;
            else
                strPath = "/" + id;
            //  
            site = siteService.GetAlls(m => m.ID == id).FirstOrDefault();
            site.Path = strPath;
            siteService.Update(site);
            return Notifization.Success(MessageText.CreateSuccess);
        }
        //##############################################################################################################################################################################################################################################################
        public ActionResult Update(SiteUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid();
            // 
            string id = model.ID;
            string title = model.Title;
            string codeID = string.Empty;
            string summary = model.Summary;
            string iconFile = model.IconFile;
            string imageFile = model.ImageFile;
            string email = model.Email;
            string fax = model.Fax;
            string phone = model.Phone;
            string tel = model.Tel;
            string workTime = model.WorkTime;
            string address = model.Address;
            string gmaps = model.Gmaps;
            int type = model.Type;
            string parentId = model.ParentID;
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
            SiteService siteService = new SiteService(_connection);
            Site site = siteService.GetAlls(m => m.ID == id).FirstOrDefault();
            if (site == null)
                return Notifization.Invalid(MessageText.Invalid);
            // 
            string sitePath = site.Path;
            if (!string.IsNullOrWhiteSpace(parentId))
            {
                if (parentId == id)
                    return Notifization.Invalid("Danh mục không hợp lệ");
                // 
                Site siteParent = siteService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (siteParent == null)
                    return Notifization.Invalid("Danh mục không hợp lệ");
                //
                Site mPath = siteService.GetAlls(m => m.ID == parentId).FirstOrDefault();
                if (mPath != null)
                    sitePath = mPath.Path + "/" + id;
                // 
            }
            else
            {
                parentId = null;
                sitePath = "/" + id;
            }
            //  
            SiteTypeOption siteTypeOption = SiteService.TypeData().Where(m => m.ID == type).FirstOrDefault();
            if (siteTypeOption == null)
                return Notifization.Invalid("Loại tổ chức không hợp lệ");
            //
            Site siteTitle = siteService.GetAlls(m => !string.IsNullOrWhiteSpace(m.Title) && m.Title.ToLower() == title.ToLower() && m.ID != id).FirstOrDefault();
            if (siteTitle != null)
                return Notifization.Invalid("Tiêu đề đã được sử dụng");
            // 
            site.ParentID = parentId;
            site.Title = title;
            site.Alias = Helper.Page.Library.FormatToUni2NONE(title);
            site.Summary = summary;
            site.IconFile = iconFile;
            site.ImageFile = imageFile;
            site.Email = email;
            site.Fax = fax;
            site.Phone = phone;
            site.Tel = tel;
            site.WorkTime = workTime;
            site.Address = address;
            site.Gmaps = gmaps;
            site.Type = type;
            site.Path = sitePath;
            site.Enabled = enabled;
            siteService.Update(site);
            return Notifization.Success(MessageText.UpdateSuccess);
        }
        public Site GetSiteByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Site WHERE ID = @Query";
                return _connection.Query<Site>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        public SiteResult ViewSiteByID(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;
                //
                string query = string.Empty;
                string langID = Helper.Current.UserLogin.LanguageID;
                string sqlQuery = @"SELECT TOP (1) * FROM App_Site WHERE ID = @Query";
                return _connection.Query<SiteResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(SiteIDModel model)
        {
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string id = model.ID;
                    if (string.IsNullOrWhiteSpace(id))
                        return Notifization.Invalid(MessageText.Invalid);
                    // 
                    WorkService workService = new WorkService(_connection);
                    Work work = workService.GetAlls(m => m.SiteID == id, transaction: _transaction).FirstOrDefault();
                    if (work != null)
                        return Notifization.Invalid("Xóa hết công việc trước");
                    //  
                    string query = $@"with TbTemp as(select ID from App_Site Where ID = @ID union all select c.ID from App_Site c inner join TbTemp mn on c.ParentID = mn.ID)SELECT ID FROM App_Site  WHERE ID IN (SELECT ID FROM TbTemp)";
                    List<string> lstSiteId = _connection.Query<string>(query, new { ID = id }, transaction: _transaction).ToList();
                    if (lstSiteId.Count == 0)
                        return Notifization.NotFound(MessageText.NotFound);
                    // delete App_Site
                    List<string> userIds = _connection.Query<string>("SELECT ID FROM View_User WHERE SiteID IN ('" + String.Join("','", lstSiteId) + "')", transaction: _transaction).ToList();
                    AttachmentService attachmentService = new AttachmentService();
                    if (userIds.Count > 0)
                    {
                        _connection.Execute("DELETE UserRole WHERE UserID IN ('" + String.Join("','", userIds) + "') ", transaction: _transaction);
                        _connection.Execute("DELETE UserInfo WHERE UserID IN ('" + String.Join("','", userIds) + "') ", transaction: _transaction);
                        _connection.Execute("DELETE UserSetting WHERE UserID IN ('" + String.Join("','", userIds) + "') ", transaction: _transaction);
                        _connection.Execute("DELETE UserLogin WHERE ID IN ('" + String.Join("','", userIds) + "') ", transaction: _transaction);
                    }
                    //
                    _connection.Execute("DELETE AttachmentCategory WHERE SiteID IN ('" + String.Join("','", lstSiteId) + "')", transaction: _transaction);
                    List<Attachment> attachments = _connection.Query<Attachment>("SELECT ID FROM Attachment WHERE SiteID IN ('" + String.Join("','", lstSiteId) + "')", transaction: _transaction).ToList();
                    if (attachments.Count > 0)
                        AttachmentFile.DeleteMultiFile(attachments, transaction: _transaction, _connection);
                    //  
                    string queryDelete = $@"with TbTemp as(select ID from App_Site Where ID = @ID union all select c.ID from App_Site c inner join TbTemp mn on c.ParentID = mn.ID)DELETE App_Site  WHERE ID IN (SELECT ID FROM TbTemp)";
                    _connection.Query(queryDelete, new { ID = id }, transaction: _transaction);
                    _transaction.Commit();
                    return Notifization.Success(MessageText.DeleteSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.TEST("::" + ex);
                }
            }


        }
        //##############################################################################################################################################################################################################################################################
        public List<SiteOption> DataOption(string languageId)
        {
            string sqlQuery = @"SELECT * FROM App_Site WHERE Enabled = 1 ORDER BY Title ASC";
            return _connection.Query<SiteOption>(sqlQuery, new { LangID = languageId }).ToList();
        }
        public static string DropdownList(string id)
        {
            string result = string.Empty;
            using (var SiteInfomationService = new SiteService())
            {
                List<SiteOption> dtList = SiteInfomationService.DataOption(null);
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
         
        public static string DistrictDropdownList(string id)
        {
            string result = string.Empty;
            using (var SiteInfomationService = new SiteService())
            {
                List<SiteOption> dtList = SiteInfomationService.DataOption(null).Where(m => string.IsNullOrWhiteSpace(m.ParentID)).ToList();
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
        //##############################################################################################################################################################################################################################################################
        public static string GetSiteNameByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return string.Empty;
            //
            id = id.ToLower();
            using (var service = new SiteService())
            {
                var site = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (site == null)
                    return string.Empty;
                //
                return site.Title;
            }
        }
        //##############################################################################################################################################################################################################################################################       
        public static List<SiteTypeOption> TypeData()
        {
            List<SiteTypeOption> menuTypeOptionModels = new List<SiteTypeOption>{
                new SiteTypeOption{
                    ID = 1,
                    Title = "Thường trực",
                },
                 new SiteTypeOption{
                    ID = 2,
                    Title = "Văn phòng",
                },
                 new SiteTypeOption{
                    ID = 3,
                    Title = "Ban đảng",
                },
                  new SiteTypeOption{
                    ID = 4,
                    Title = "Chi bộ đảng",
                },
            };
            return menuTypeOptionModels;
        }
        public static string SiteTypeDropdownList(int id)
        {
            string result = string.Empty;
            List<SiteTypeOption> dtList = SiteService.TypeData();
            if (dtList.Count > 0)
            {
                foreach (var item in dtList)
                {
                    string select = string.Empty;
                    if (item.ID == id)
                        select = "selected";
                    result += "<option value='" + item.ID + "'" + select + ">" + item.Title + "</option>";
                }
            }
            return result;
        }
        internal static string GetTypeName(int type)
        {
            SiteTypeOption typeOption = SiteService.TypeData().Where(m => m.ID == type).FirstOrDefault();
            if (typeOption != null)
                return typeOption.Title;
            return "Không xác định";
        }

    }
}
