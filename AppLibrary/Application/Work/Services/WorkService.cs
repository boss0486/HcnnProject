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
using WebCore.ENM;
using WebCore.Model.Enum;
using Helper.Page;

namespace WebCore.Services
{
    public interface IWorkService : IEntityService<Work> { }
    public class WorkService : EntityService<Work>, IWorkService
    {
        public WorkService() : base() { }
        public WorkService(System.Data.IDbConnection db) : base(db) { }
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
            string departmentId = "";
            UserSettingService userSettingService = new UserSettingService();
            UserSetting userSetting = userSettingService.GetAlls(m => m.UserID == Helper.Current.UserLogin.IdentifierID).FirstOrDefault();
            if (userSetting != null)
                departmentId = userSetting.DepartmentID;
            // 
            whereCondition = "";
            whereCondition += $@" AND (AssignTo = @SiteID OR AssignTo = @DepartmentID OR CreatedBy = @UserID)";
            string sqlQuery = $@"SELECT * FROM App_Work WHERE  ID IS NOT NULL {whereCondition} ORDER BY OrderID ASC ";
            //
            List<WorkResult> dataAll = _connection.Query<WorkResult>(sqlQuery, new
            {
                DepartmentID = departmentId,
                Query = Helper.Page.Library.FormatNameToUni2NONE(query),
                SiteID = Helper.Current.UserLogin.SiteID,
                UserID = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            //
            if (dataAll.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            // 
            List<WorkResult> dtList = dataAll.Where(m =>
            string.IsNullOrWhiteSpace(m.ParentID)
            || (!string.IsNullOrWhiteSpace(m.AssignTo) && m.AssignType == (int)WorkEnum.AssignType.IsChild && m.CreatedBy != Helper.Current.UserLogin.IdentifierID)).ToList();
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            //
            foreach (var item in dtList)
            {
                item.SubData = SubDataList(item.ID, dataAll);
            }
            //
            List<WorkResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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
                Total = dataAll.Count,
                Page = page
            };
            //
            return Notifization.Data(MessageText.Success, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
        }
        public ActionResult GetWorkOption(int showLevel = 0)
        {
            WorkService WorkService = new WorkService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string whereCondition = $@" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
            string sqlQuery = $"SELECT * FROM App_Work WHERE Enabled = @Enabled {whereCondition} ORDER BY ParentID, OrderID ASC";
            List<WorkOptionModel> workOptionModels = _connection.Query<WorkOptionModel>(sqlQuery, new
            {
                Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED,
                SiteID = Helper.Current.UserLogin.SiteID,
                CreatedBy = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            if (workOptionModels.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            // 
            return Notifization.Data(MessageText.Success, workOptionModels);
        }
        public ActionResult Create(WorkCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string title = model.Title;
                    string htmlText = model.HtmlText;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    IEnumerable<string> arrFile = model.Files;
                    int state = model.State;
                    //
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Không được để trống công việc");
                    //
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tên công việc không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tên công việc giới hạn [2-80] ký tự");
                    // summary valid               
                    if (string.IsNullOrWhiteSpace(htmlText))
                        return Notifization.Invalid("Không được để trống nội dung");
                    //
                    if (!Validate.TestText(htmlText))
                        return Notifization.Invalid("Nội dung không hợp lệ");
                    if (htmlText.Length < 1 || htmlText.Length > 5000)
                        return Notifization.Invalid("Nội dung giới hạn [1-5000] ký tự");
                    //  
                    if (string.IsNullOrWhiteSpace(executeDate))
                        return Notifization.Invalid("Nhập ngày thực hiện");
                    //
                    if (!Validate.TestDate(executeDate))
                        return Notifization.Invalid("Ngày thực hiện không hợp lệ");
                    //     
                    if (string.IsNullOrWhiteSpace(deadline))
                        return Notifization.Invalid("Nhập ngày kết thúc");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày kết thúc không hợp lệ");
                    // 
                    if (arrFile != null && arrFile.Count() > 5)
                        return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
                    //

                    WorkService workService = new WorkService(_connection);
                    Work workTitle = workService.GetAlls(m => m.Title.ToLower() == title.ToLower(), transaction: _transaction).FirstOrDefault();
                    if (workTitle != null)
                        return Notifization.Invalid("Tên công việc đã được sử dụng");
                    // 
                    WorkStateOptionModel workStateOption = workService.WorkStateOption().Where(m => m.ID == state).FirstOrDefault();
                    if (workStateOption == null)
                        return Notifization.Invalid("Trạng thái không hợp lệ");
                    //int orderId = 1;
                    //if (sortType == (int)AttachmentEnum.Sort.FIRST)
                    //{
                    //    List<Work> attachmentCategories = workService.GetAlls(transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                    //    if (attachmentCategories.Count > 0)
                    //    {
                    //        int cnt = 2;
                    //        foreach (var item in attachmentCategories)
                    //        {
                    //            item.OrderID = cnt;
                    //            workService.Update(item, transaction: _transaction);
                    //            cnt++;
                    //        }
                    //    }
                    //    //
                    //}
                    //else
                    //{
                    //    orderId = workService.GetAlls(transaction: _transaction).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
                    //}
                    //
                    string parentId = null;
                    string id = workService.Create<string>(new Work()
                    {
                        ParentID = parentId,
                        AssignTo = null,
                        AssignDate = null,
                        ReceptionType = (int)WorkEnum.ReceptionType.None,
                        Title = title,
                        Alias = Helper.Page.Library.FormatToUni2NONE(title),
                        HtmlText = htmlText,
                        ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate),
                        Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline),
                        State = state,
                        Enabled = (int)ModelEnum.State.ENABLED,
                    }, transaction: _transaction);
                    // update part
                    string strPath = string.Empty;
                    Work workPath = workService.GetAlls(m => m.ID == parentId, transaction: _transaction).FirstOrDefault();
                    if (workPath != null)
                        strPath = workPath.Path + "/" + id;
                    else
                        strPath = "/" + id;
                    // 
                    Work work = workService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    work.Path = strPath;
                    workService.Update(work, transaction: _transaction);
                    // file  
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, transaction: _transaction, connection: _connection);
                    //
                    WorkNotifyService workNotifyService = new WorkNotifyService();

                    //Add work notify
                    workNotifyService.AddNotify(new WorkNotify
                    {
                        WorkID = id,
                        IsShow = true
                    }, _transaction, _connection);

                    _transaction.Commit();
                    return Notifization.Success(MessageText.CreateSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.TEST("::" + ex);

                }
            }
        }
        public ActionResult Update(WorkUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string parentId = model.CategoryID;
                    string id = model.ID;
                    string title = model.Title;
                    string htmlText = model.HtmlText;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    IEnumerable<string> arrFile = model.Files;
                    int state = model.State;
                    // 
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Không được để trống công việc");
                    //
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tên công việc không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tên công việc giới hạn [2-80] ký tự");
                    // summary valid               
                    if (string.IsNullOrWhiteSpace(htmlText))
                        return Notifization.Invalid("Không được để trống nội dung");
                    //
                    if (!Validate.TestText(htmlText))
                        return Notifization.Invalid("Nội dung không hợp lệ");
                    if (htmlText.Length < 1 || htmlText.Length > 5000)
                        return Notifization.Invalid("Nội dung giới hạn [1-5000] ký tự");
                    // 
                    if (string.IsNullOrWhiteSpace(executeDate))
                        return Notifization.Invalid("Nhập ngày thực hiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày thực hiện không hợp lệ");
                    //     
                    if (string.IsNullOrWhiteSpace(deadline))
                        return Notifization.Invalid("Nhập ngày kết thúc");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày kết thúc không hợp lệ");
                    //    
                    if (arrFile != null && arrFile.Count() > 5)
                        return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
                    //
                    WorkService workService = new WorkService(_connection);
                    Work work = workService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (work == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    Work WorkTitle = workService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ID != id, transaction: _transaction).FirstOrDefault();
                    if (WorkTitle != null)
                        return Notifization.Invalid("Tên công việc đã được sử dụng");
                    // 
                    WorkStateOptionModel workStateOption = workService.WorkStateOption().Where(m => m.ID == state).FirstOrDefault();
                    if (workStateOption == null)
                        return Notifization.Invalid("Trạng thái không hợp lệ");
                    //
                    string workPath = work.Path;
                    if (!string.IsNullOrWhiteSpace(parentId))
                    {
                        if (parentId == id)
                            return Notifization.Invalid("Danh mục không hợp lệ");
                        // 
                        Work siteParent = workService.GetAlls(m => m.ID == parentId, transaction: _transaction).FirstOrDefault();
                        if (siteParent == null)
                            return Notifization.Invalid("Danh mục không hợp lệ");
                        //
                        Work mPath = workService.GetAlls(m => m.ID == parentId, transaction: _transaction).FirstOrDefault();
                        if (mPath != null)
                            workPath = mPath.Path + "/" + id;
                        // 
                    }
                    else
                    {
                        parentId = null;
                        workPath = "/" + id;
                    }
                    // update content 
                    work.ParentID = parentId;
                    work.Title = title;
                    work.Alias = Helper.Page.Library.FormatToUni2NONE(title);
                    work.HtmlText = htmlText;
                    work.Path = workPath;
                    work.ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate);
                    work.Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline);
                    work.State = state;
                    workService.Update(work, transaction: _transaction);
                    // file  
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, transaction: _transaction, connection: _connection);
                    // 
                    _transaction.Commit();
                    return Notifization.Success(MessageText.UpdateSuccess);
                }
                catch (Exception)
                {
                    _transaction.Rollback();
                    return Notifization.Invalid(MessageText.NotService);
                }
            }
        }
        public ActionResult Delete(WorkIDModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
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
                    Work work = workService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (work == null)
                        return Notifization.NotFound();
                    //
                    _connection.Query(@"DELETE App_WorkUser WHERE WorkID = @WorkID", new { WorkID = id }, transaction: _transaction);
                    workService.Remove(id, transaction: _transaction);
                    _transaction.Commit();
                    return Notifization.Success(MessageText.DeleteSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.NotService;
                }
            }
        }
        public Work GetWorkByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_Work WHERE ID = @Query";
            Work Work = _connection.Query<Work>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (Work == null)
                return null;
            //
            return Work;
        }

        public ViewWorkResult ViewWorkByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_Work WHERE ID = @ID";
            ViewWorkResult viewWork = _connection.Query<ViewWorkResult>(sqlQuery, new { ID = id }).FirstOrDefault();
            if (viewWork == null)
                return null;
            //
            viewWork.Attachments = AttachmentIngredientService.GetFileByForID((int)ModelEnum.FileType.MULTI, id);
            WorkUserService workUserService = new WorkUserService(_connection);
            List<WorkUser> workUsers = workUserService.GetAlls(m => m.WorkID == id).ToList();
            viewWork.UserExecutes = workUsers.Where(m => m.UserType == (int)WorkEnum.UserType.UserExecute).Select(m => m.UserID).ToList();
            viewWork.UserFollows = workUsers.Where(m => m.UserType == (int)WorkEnum.UserType.UserFollows).Select(m => m.UserID).ToList();
            return viewWork;
        }

        public WorkAssign GetWorkForAssign(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) w.* ,(CASE WHEN (select COUNT(ID) from App_Work where ParentID = w.ID) > 0 OR w.State != @State THEN 1 ELSE 0 END) IsAssigned FROM App_Work as w WHERE w.ID = @ID";
            WorkAssign work = _connection.Query<WorkAssign>(sqlQuery, new
            {
                ID = id,
                State = WorkEnum.State.Assigned
            }).FirstOrDefault();
            if (work == null)
                return null;
            //
            return work;
        }
        public ActionResult AssignList(WorkAssignSearchModel model)
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
            string sqlQuery = $@"SELECT * FROM App_Work WHERE ParentID = @ParentID AND Enabled = 1 ORDER BY ParentID, Title ASC";
            //
            List<WorkResult> dtList = _connection.Query<WorkResult>(sqlQuery, new
            {
                ParentID = model.WorkID
            }).ToList();
            //
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            // 
            List<WorkResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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
            return Notifization.Data(MessageText.Success, data: result, role: null, paging: pagingModel);
        }

        //public ActionResult AssignList(WorkAssignSearchModel model)
        //{
        //    if (model == null)
        //        return Notifization.Invalid(MessageText.Invalid);
        //    //
        //    int page = model.Page;
        //    string query = model.Query;
        //    if (string.IsNullOrWhiteSpace(query))
        //        query = "";
        //    //
        //    string whereCondition = string.Empty;
        //    //
        //    //SearchResult searchResult = WebCore.Model.Services.ModelService.SearchDefault(new SearchModel
        //    //{
        //    //    Query = model.Query,
        //    //    TimeExpress = model.TimeExpress,
        //    //    Status = model.Status,
        //    //    StartDate = model.StartDate,
        //    //    EndDate = model.EndDate,
        //    //    Page = model.Page,
        //    //    TimeZoneLocal = model.TimeZoneLocal
        //    //});
        //    //if (searchResult != null)
        //    //{
        //    //    if (searchResult.Status == 1)
        //    //        whereCondition = searchResult.Message;
        //    //    else
        //    //        return Notifization.Invalid(searchResult.Message);
        //    //}
        //    //  
        //    string departmentId = "";
        //    UserSettingService userSettingService = new UserSettingService(_connection);
        //    UserSetting userSetting = userSettingService.GetAlls(m => m.UserID == Helper.Current.UserLogin.IdentifierID).FirstOrDefault();
        //    if (userSetting != null)
        //        departmentId = userSetting.DepartmentID;
        //    //
        //    if (string.IsNullOrWhiteSpace(model.WorkID))
        //        return Notifization.Invalid(MessageText.Invalid);
        //    // 
        //    string sqlQuery = $@"SELECT * FROM App_Work WHERE ParentID = @ParentID AND Enabled = 1 ORDER BY ParentID, Title ASC ";
        //    //
        //    List<WorkResult> dataAll = _connection.Query<WorkResult>(sqlQuery, new
        //    {
        //        ParentID = model.WorkID
        //    }).ToList();
        //    //
        //    if (dataAll.Count == 0)
        //        return Notifization.NotFound(MessageText.NotFound);
        //    // 
        //    List<WorkResult> dtList = dataAll.Where(m => string.IsNullOrWhiteSpace(m.ParentID)).ToList();
        //    if (dtList.Count == 0)
        //        return Notifization.NotFound(MessageText.NotFound);
        //    //
        //    foreach (var item in dtList)
        //    {
        //        item.SubData = SubDataList(item.ID, dataAll);
        //    }

        //    List<WorkResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
        //    if (result.Count <= 0 && page > 1)
        //    {
        //        page -= 1;
        //        result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
        //    }
        //    if (result.Count <= 0)
        //        return Notifization.NotFound(MessageText.NotFound);
        //    //
        //    Helper.Pagination.PagingModel pagingModel = new Helper.Pagination.PagingModel
        //    {
        //        PageSize = Helper.Pagination.Paging.PAGESIZE,
        //        Total = dtList.Count,
        //        Page = page
        //    };
        //    //
        //    return Notifization.Data(MessageText.Success, data: result, role: null, paging: pagingModel);
        //}
        public List<WorkResult> SubDataList(string parentId, List<WorkResult> menuResults)
        {
            List<WorkResult> dtList = menuResults.Where(m => m.ParentID == parentId).ToList();
            if (dtList.Count == 0)
                return null;
            //
            foreach (var item in dtList)
            {
                item.IsSub = true;
                item.SubData = SubDataList(item.ID, menuResults);
            }
            return dtList;
        }

        //##############################################################################################################################################################################################################################################################
        public ActionResult AssignFast(WorkAssignFastModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string workId = model.WorkID;
                    string assignTo = model.AssignTo;
                    int receptionType = model.ReceptionType;
                    //
                    if (string.IsNullOrWhiteSpace(workId))
                        return Notifization.Invalid("Công việc không xác định");
                    // 
                    //if (string.IsNullOrWhiteSpace(assignTo))
                    //    return Notifization.Invalid("Chọn bộ phận tiếp nhận");
                    ////  
                    WorkService workService = new WorkService(_connection);
                    Work work = workService.GetAlls(m => m.ID == workId, transaction: _transaction).FirstOrDefault();
                    if (work == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    if (work.State != (int)WorkEnum.State.Assigned)
                        return Notifization.Invalid("Bộ phận cấp dưới đang xử lý");
                    //  
                    Work workChild = workService.GetAlls(m => m.ParentID == workId, transaction: _transaction).FirstOrDefault();
                    if (workChild != null)
                        return Notifization.Invalid("Xóa công việc con trước");
                    //  
                    if (!string.IsNullOrWhiteSpace(assignTo))
                    {
                        if (!WorkAssignType().Contains(receptionType))
                            return Notifization.Invalid(MessageText.Invalid);
                        // 
                        if (receptionType == (int)WorkEnum.ReceptionType.Department)
                        {
                            DepartmentService departmentService = new DepartmentService(_connection);
                            Department department = departmentService.GetAlls(m => m.ID == assignTo, transaction: _transaction).FirstOrDefault();
                            if (department == null)
                                return Notifization.Invalid("Bộ phận tiếp nhận không xác định");
                            //
                        }
                        if (receptionType == (int)WorkEnum.ReceptionType.OutSite)
                        {
                            SiteService siteService = new SiteService(_connection);
                            Site site = siteService.GetAlls(m => m.ID == assignTo, transaction: _transaction).FirstOrDefault();
                            if (site == null)
                                return Notifization.Invalid("Bộ phận tiếp nhận không xác định");
                            //
                        }
                    }
                    else
                    {
                        assignTo = null;
                        receptionType = (int)WorkEnum.ReceptionType.None;
                    }
                    //
                    work.AssignTo = assignTo;
                    work.ReceptionType = receptionType;
                    work.AssignDate = DateTime.Now;
                    work.AssignType = (int)WorkEnum.AssignType.Direct;
                    workService.Update(work, transaction: _transaction);
                    // logged
                    //
                    _transaction.Commit();
                    return Notifization.Success("Đã giao thành công");
                }
                catch (Exception)
                {
                    _transaction.Rollback();
                    return Notifization.Invalid(MessageText.NotService);
                }
            }
        }
        public ActionResult AssignAdd(WorkAssignCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string parentId = model.CategoryID;
                    string assignTo = model.AssignTo;
                    int receptionType = model.ReceptionType;
                    string title = model.Title;
                    string htmlText = model.HtmlText;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    IEnumerable<string> arrFile = model.Files;
                    int state = model.State;
                    // 
                    if (string.IsNullOrWhiteSpace(parentId))
                        return Notifization.Invalid("Công việc không hợp lệ");
                    //
                    string path = string.Empty;
                    WorkService workService = new WorkService(_connection);
                    Work workParent = workService.GetAlls(m => m.ID == parentId, transaction: _transaction).FirstOrDefault();
                    if (workParent == null)
                        return Notifization.Invalid("Công việc không hợp lệ");
                    //
                    if (!string.IsNullOrWhiteSpace(workParent.AssignTo))
                        return Notifization.Invalid("Bộ phận tiếp nhận đang xử lý");
                    //   
                    if (!WorkAssignType().Contains(receptionType))
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Không được để trống công việc");
                    //
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tên công việc không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tên công việc giới hạn [2-80] ký tự");
                    // summary valid               
                    if (string.IsNullOrWhiteSpace(htmlText))
                        return Notifization.Invalid("Không được để trống nội dung");
                    //
                    if (!Validate.TestText(htmlText))
                        return Notifization.Invalid("Nội dung không hợp lệ");
                    if (htmlText.Length < 1 || htmlText.Length > 5000)
                        return Notifization.Invalid("Nội dung giới hạn [1-5000] ký tự");
                    //  
                    if (string.IsNullOrWhiteSpace(executeDate))
                        return Notifization.Invalid("Nhập ngày thực hiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày thực hiện không hợp lệ");
                    //     
                    if (string.IsNullOrWhiteSpace(deadline))
                        return Notifization.Invalid("Nhập ngày kết thúc");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày kết thúc không hợp lệ");
                    //  
                    if (arrFile != null && arrFile.Count() > 5)
                        return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
                    //
                    WorkStackService workStackService = new WorkStackService(_connection);
                    Work workTitle = workService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ParentID == parentId, transaction: _transaction).FirstOrDefault();
                    if (workTitle != null)
                        return Notifization.Invalid("Tên công việc đã được sử dụng");
                    //   
                    WorkStateOptionModel workStateOption = workService.WorkStateOption().Where(m => m.ID == state).FirstOrDefault();
                    if (workStateOption == null)
                        return Notifization.Invalid("Trạng thái không hợp lệ");
                    //
                    string id = workService.Create<string>(new Work()
                    {
                        ParentID = parentId,
                        Title = title,
                        Alias = Helper.Page.Library.FormatToUni2NONE(title),
                        HtmlText = htmlText,
                        ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate),
                        Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline),
                        //
                        AssignTo = assignTo,
                        ReceptionType = receptionType,
                        AssignDate = DateTime.Now,
                        AssignType = (int)WorkEnum.AssignType.IsChild,
                        //
                        State = state,
                        Enabled = (int)ModelEnum.State.ENABLED,
                    }, transaction: _transaction);
                    // update part 
                    Work work = workService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    work.Path = workParent.Path + "/" + id;
                    workService.Update(workParent, transaction: _transaction);
                    // file  
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, transaction: _transaction, connection: _connection);
                    // 
                    _transaction.Commit();
                    return Notifization.Success(MessageText.CreateSuccess);
                }
                catch (Exception)
                {
                    _transaction.Rollback();
                    return Notifization.Invalid(MessageText.NotService);

                }
            }
        }
        public ActionResult AssignUpdate(WorkAssignUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string id = model.ID;
                    string assignTo = model.AssignTo;
                    int receptionType = model.ReceptionType;
                    string title = model.Title;
                    string htmlText = model.HtmlText;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    IEnumerable<string> arrFile = model.Files;
                    int state = model.State;
                    // 
                    if (string.IsNullOrWhiteSpace(id))
                        return Notifization.Invalid("Công việc không hợp lệ");
                    //
                    WorkService workService = new WorkService(_connection);
                    Work work = workService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (work == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    //  
                    Work workGroup = workService.GetAlls(m => m.ParentID == id, transaction: _transaction).FirstOrDefault();
                    if (workGroup != null)
                        return Notifization.Invalid("Bộ phận cấp dưới đang xử lý");
                    //  
                    if (!WorkAssignType().Contains(receptionType))
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Không được để trống công việc");
                    //
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tên công việc không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tên công việc giới hạn [2-80] ký tự");
                    // summary valid               
                    if (string.IsNullOrWhiteSpace(htmlText))
                        return Notifization.Invalid("Không được để trống nội dung");
                    //
                    if (!Validate.TestText(htmlText))
                        return Notifization.Invalid("Nội dung không hợp lệ");
                    if (htmlText.Length < 1 || htmlText.Length > 5000)
                        return Notifization.Invalid("Nội dung giới hạn [1-5000] ký tự");
                    //  
                    if (string.IsNullOrWhiteSpace(executeDate))
                        return Notifization.Invalid("Nhập ngày thực hiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày thực hiện không hợp lệ");
                    //     
                    if (string.IsNullOrWhiteSpace(deadline))
                        return Notifization.Invalid("Nhập ngày kết thúc");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày kết thúc không hợp lệ");
                    //   
                    if (arrFile != null && arrFile.Count() > 5)
                        return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
                    //
                    Work workTitle = workService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ParentID == work.ParentID && m.ID != id, transaction: _transaction).FirstOrDefault();
                    if (workTitle != null)
                        return Notifization.Invalid("Tên công việc đã được sử dụng");
                    //  
                    work.Title = title;
                    work.Alias = Helper.Page.Library.FormatToUni2NONE(title);
                    work.HtmlText = htmlText;
                    work.ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate);
                    work.Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline);
                    //
                    work.AssignTo = assignTo;
                    work.ReceptionType = receptionType;
                    work.State = state;
                    workService.Update(work, transaction: _transaction);
                    // file
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, transaction: _transaction, connection: _connection);
                    _transaction.Commit();
                    return Notifization.Success(MessageText.UpdateSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.Invalid(MessageText.NotService + ex);

                }
            }
        }

        //##############################################################################################################################################################################################################################################################
        public ActionResult ProcessAdd(WorkProcessCreateModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string categoryId = model.CategoryID;
                    string title = model.Title;
                    string htmlText = model.HtmlText;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    IEnumerable<string> arrFile = model.Files;
                    List<string> userExecutes = model.UserExecutes;
                    List<string> userFollows = model.UserFollows;
                    string htmlNote = model.HtmlNote;
                    //
                    if (string.IsNullOrWhiteSpace(categoryId))
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Không được để trống công việc");
                    //
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tên công việc không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tên công việc giới hạn [2-80] ký tự");
                    // summary valid               
                    if (string.IsNullOrWhiteSpace(htmlText))
                        return Notifization.Invalid("Không được để trống nội dung");
                    //
                    if (!Validate.TestText(htmlText))
                        return Notifization.Invalid("Nội dung không hợp lệ");
                    if (htmlText.Length < 1 || htmlText.Length > 5000)
                        return Notifization.Invalid("Nội dung giới hạn [1-5000] ký tự");
                    //  
                    if (string.IsNullOrWhiteSpace(executeDate))
                        return Notifization.Invalid("Nhập ngày thực hiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày thực hiện không hợp lệ");
                    //     
                    if (string.IsNullOrWhiteSpace(deadline))
                        return Notifization.Invalid("Nhập ngày kết thúc");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày kết thúc không hợp lệ");
                    //   
                    if (arrFile != null && arrFile.Count() > 5)
                        return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
                    //
                    if (userExecutes.Count == 0)
                        return Notifization.Invalid("Chọn nhóm/người thực hiện");
                    //
                    if (userFollows.Count == 0)
                        return Notifization.Invalid("Chọn nhóm/người theo dõi");
                    // 
                    if (!string.IsNullOrWhiteSpace(htmlNote))
                    {
                        htmlNote = htmlNote.Trim();
                        if (!Validate.TestText(htmlNote))
                            return Notifization.Invalid("Lưu ý không hợp lệ");
                        if (htmlNote.Length < 1 || htmlNote.Length > 120)
                            return Notifization.Invalid("Lưu ý giới hạn [1-120] ký tự");
                    }
                    WorkService workService = new WorkService(_connection);
                    Work workParent = workService.GetAlls(m => m.ID == categoryId, transaction: _transaction).FirstOrDefault();
                    if (workParent == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    Work workTitle = workService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ParentID == categoryId, transaction: _transaction).FirstOrDefault();
                    if (workTitle != null)
                        return Notifization.Invalid("Tên công việc đã được sử dụng");
                    //  
                    string id = workService.Create<string>(new Work
                    {
                        ParentID = categoryId,
                        Title = title,
                        Alias = Helper.Page.Library.FormatToUni2NONE(title),
                        HtmlText = htmlText,
                        ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate),
                        Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline),
                        HtmlNote = htmlNote,
                        //
                        AssignTo = workParent.AssignTo,
                        ReceptionType = (int)WorkEnum.ReceptionType.UserOfDepartment,
                        AssignDate = DateTime.Now,
                        AssignType = (int)WorkEnum.AssignType.IsChild,
                        //
                        State = workParent.State,
                        Enabled = (int)ModelEnum.State.ENABLED,
                    }, transaction: _transaction);
                    // update part
                    string strPath = string.Empty;
                    Work workPath = workService.GetAlls(m => m.ID == categoryId, transaction: _transaction).FirstOrDefault();
                    if (workPath != null)
                        strPath = workPath.Path + "/" + id;
                    else
                        strPath = "/" + id;
                    //  
                    Work work = workService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    work.Path = strPath;
                    workService.Update(work, transaction: _transaction);
                    // work user
                    WorkUserService workUserService = new WorkUserService(_connection);
                    foreach (var item in userExecutes)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;
                        //
                        workUserService.Create<string>(new WorkUser
                        {
                            WorkID = id,
                            UserID = item,
                            UserType = (int)WorkEnum.UserType.UserExecute,
                        }, transaction: _transaction);
                    }
                    foreach (var item in userFollows)
                    {
                        if (string.IsNullOrWhiteSpace(item))
                            continue;
                        //
                        workUserService.Create<string>(new WorkUser
                        {
                            WorkID = id,
                            UserID = item,
                            UserType = (int)WorkEnum.UserType.UserFollows,
                        }, transaction: _transaction);
                    }
                    // file  
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, transaction: _transaction, connection: _connection);
                    _transaction.Commit();
                    return Notifization.Success(MessageText.CreateSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.Invalid(MessageText.NotService + ex);

                }
            }
        }
        public ActionResult ProcessUpdate(WorkProcessUpdateModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string id = model.ID;
                    string title = model.Title;
                    string htmlText = model.HtmlText;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    IEnumerable<string> arrFile = model.Files;
                    List<string> userExecutes = model.UserExecutes;
                    List<string> userFollows = model.UserFollows;
                    string htmlNote = model.HtmlNote;
                    //
                    if (string.IsNullOrWhiteSpace(id))
                        return Notifization.Invalid(MessageText.Invalid);
                    // 
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Không được để trống công việc");
                    //
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tên công việc không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tên công việc giới hạn [2-80] ký tự");
                    // summary valid               
                    if (string.IsNullOrWhiteSpace(htmlText))
                        return Notifization.Invalid("Không được để trống nội dung");
                    //
                    if (!Validate.TestText(htmlText))
                        return Notifization.Invalid("Nội dung không hợp lệ");
                    if (htmlText.Length < 1 || htmlText.Length > 5000)
                        return Notifization.Invalid("Nội dung giới hạn [1-5000] ký tự");
                    //  
                    if (string.IsNullOrWhiteSpace(executeDate))
                        return Notifization.Invalid("Nhập ngày thực hiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày thực hiện không hợp lệ");
                    //     
                    if (string.IsNullOrWhiteSpace(deadline))
                        return Notifization.Invalid("Nhập ngày kết thúc");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày kết thúc không hợp lệ");
                    //   
                    if (arrFile != null && arrFile.Count() > 5)
                        return Notifization.Invalid("Tệp tin đính kèm giới hạn [1-5]");
                    //
                    if (userExecutes.Count == 0)
                        return Notifization.Invalid("Chọn nhóm/người thực hiện");
                    //
                    if (userFollows.Count == 0)
                        return Notifization.Invalid("Chọn nhóm/người theo dõi");
                    // 
                    if (!string.IsNullOrWhiteSpace(htmlNote))
                    {
                        htmlNote = htmlNote.Trim();
                        if (!Validate.TestText(htmlNote))
                            return Notifization.Invalid("Mô tả không hợp lệ");
                        if (htmlNote.Length < 1 || htmlNote.Length > 120)
                            return Notifization.Invalid("Lưu ý giới hạn [1-120] ký tự");
                    }
                    WorkService workService = new WorkService(_connection);
                    Work work = workService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (work == null)
                        return Notifization.Invalid("Công việc không hợp lệ");
                    //
                    Work workTitle = workService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.ParentID == work.ParentID && m.ID != id, transaction: _transaction).FirstOrDefault();
                    if (workTitle != null)
                        return Notifization.Invalid("Tên công việc đã được sử dụng");
                    //  
                    work.Title = title;
                    work.Alias = Helper.Page.Library.FormatToUni2NONE(title);
                    work.HtmlText = htmlText;
                    work.ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate);
                    work.Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline);
                    work.HtmlNote = htmlNote;
                    workService.Update(work, transaction: _transaction);
                    // 
                    WorkUserService workUserService = new WorkUserService(_connection);
                    List<WorkUser> WorkUser = workUserService.GetAlls(m => m.WorkID == id, transaction: _transaction).ToList();
                    if (userExecutes.Count > 0)
                    {
                        List<string> userExecutesInDb = WorkUser.Where(m => m.UserType == (int)WorkEnum.UserType.UserExecute).Select(m => m.UserID).ToList();
                        List<string> arrNew = userExecutes.Except<string>(userExecutesInDb).ToList();
                        List<string> arrDeletes = userExecutesInDb.Except<string>(userExecutes).ToList();
                        foreach (var item in arrNew)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            //
                            workUserService.Create<string>(new WorkUser
                            {
                                WorkID = id,
                                UserID = item,
                                UserType = (int)WorkEnum.UserType.UserExecute,
                            }, transaction: _transaction);
                        }
                        workUserService.Execute("DELETE App_WorkUser WHERE WorkID = @WorkID AND UserID IN ('" + String.Join("','", arrDeletes) + "')", new
                        {
                            WorkID = id
                        }, transaction: _transaction);
                    }
                    else
                    {
                        // add 
                        foreach (var item in userExecutes)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            //
                            WorkUser workUser = workUserService.GetAlls(m => m.WorkID == id && m.UserID == item && m.UserType == (int)WorkEnum.UserType.UserExecute, transaction: _transaction).FirstOrDefault();
                            if (workUser == null)
                            {
                                string guid = workUserService.Create<string>(new WorkUser()
                                {
                                    WorkID = id,
                                    UserID = item,
                                    UserType = (int)WorkEnum.UserType.UserExecute,
                                }, transaction: _transaction);
                            }
                        }
                    }

                    if (userFollows.Count > 0)
                    {
                        List<string> userFollowsInDb = WorkUser.Where(m => m.UserType == (int)WorkEnum.UserType.UserFollows).Select(m => m.UserID).ToList();
                        List<string> arrNew = userFollows.Except<string>(userFollowsInDb).ToList();
                        List<string> arrDeletes = userFollowsInDb.Except<string>(userFollows).ToList();
                        foreach (var item in arrNew)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            //
                            workUserService.Create<string>(new WorkUser
                            {
                                WorkID = id,
                                UserID = item,
                                UserType = (int)WorkEnum.UserType.UserFollows,
                            }, transaction: _transaction);
                        }
                        workUserService.Execute("DELETE App_WorkUser WHERE WorkID = @WorkID AND UserID IN ('" + String.Join("','", arrDeletes) + "')", new
                        {
                            WorkID = id
                        }, transaction: _transaction);
                    }
                    else
                    {
                        // add 
                        foreach (var item in userFollows)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            //
                            WorkUser workUser = workUserService.GetAlls(m => m.WorkID == id && m.UserID == item && m.UserType == (int)WorkEnum.UserType.UserFollows, transaction: _transaction).FirstOrDefault();
                            if (workUser == null)
                            {
                                string guid = workUserService.Create<string>(new WorkUser()
                                {
                                    WorkID = id,
                                    UserID = item,
                                    UserType = (int)WorkEnum.UserType.UserFollows,
                                }, transaction: _transaction);
                            }
                        }
                    }
                    // file  
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    attachmentIngredientService.UpdateFile((int)ModelEnum.FileType.MULTI, id, arrFile, transaction: _transaction, connection: _connection);
                    _transaction.Commit();
                    return Notifization.Success(MessageText.UpdateSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.Invalid(MessageText.NotService + ex);

                }
            }
        }
        public ActionResult ProcessFastAssign(WorkProcessAssignModel model)
        {
            if (model == null)
                return Notifization.Invalid(MessageText.Invalid);
            //
            _connection.Open();
            using (var _transaction = _connection.BeginTransaction())
            {
                try
                {
                    string id = model.ID;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    List<string> userExecutes = model.UserExecutes;
                    List<string> userFollows = model.UserFollows;
                    string htmlNote = model.HtmlNote;
                    //
                    if (string.IsNullOrWhiteSpace(id))
                        return Notifization.Invalid(MessageText.Invalid);
                    //  
                    WorkService workService = new WorkService(_connection);
                    Work work = workService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (work == null)
                        return Notifization.Invalid("Công việc không hợp lệ");
                    // 
                    work.ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate);
                    work.Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline);
                    work.HtmlNote = htmlNote;
                    workService.Update(work, transaction: _transaction);
                    // 
                    WorkUserService workUserService = new WorkUserService(_connection);
                    List<WorkUser>  workUsers = workUserService.GetAlls(m => m.WorkID == id, transaction: _transaction).ToList();
                    if (userExecutes.Count > 0)
                    {
                        List<string> userExecutesInDb = workUsers.Where(m => m.UserType == (int)WorkEnum.UserType.UserExecute).Select(m => m.UserID).ToList();
                        List<string> arrNew = userExecutes.Except<string>(userExecutesInDb).ToList();
                        List<string> arrDeletes = userExecutesInDb.Except<string>(userExecutes).ToList();
                        foreach (var item in arrNew)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            //
                            workUserService.Create<string>(new WorkUser
                            {
                                WorkID = id,
                                UserID = item,
                                UserType = (int)WorkEnum.UserType.UserExecute,
                            }, transaction: _transaction);
                        }
                        workUserService.Execute("DELETE App_WorkUser WHERE WorkID = @WorkID AND UserID IN ('" + String.Join("','", arrDeletes) + "') AND UserType = @Type", new
                        {
                            WorkID = id,
                            Type = (int)WorkEnum.UserType.UserExecute
                        }, transaction: _transaction);
                    }
                    else
                    {
                        // add 
                        foreach (var item in userExecutes)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            //
                            WorkUser workUser = workUserService.GetAlls(m => m.WorkID == id && m.UserID == item && m.UserType == (int)WorkEnum.UserType.UserExecute, transaction: _transaction).FirstOrDefault();
                            if (workUser == null)
                            {
                                string guid = workUserService.Create<string>(new WorkUser()
                                {
                                    WorkID = id,
                                    UserID = item,
                                    UserType = (int)WorkEnum.UserType.UserExecute,
                                }, transaction: _transaction);
                            }
                        }
                    }

                    if (userFollows.Count > 0)
                    {
                        List<string> userFollowsInDb = workUsers.Where(m => m.UserType == (int)WorkEnum.UserType.UserFollows).Select(m => m.UserID).ToList();
                        List<string> arrNew = userFollows.Except<string>(userFollowsInDb).ToList();
                        List<string> arrDeletes = userFollowsInDb.Except<string>(userFollows).ToList();
                        foreach (var item in arrNew)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            //
                            workUserService.Create<string>(new WorkUser
                            {
                                WorkID = id,
                                UserID = item,
                                UserType = (int)WorkEnum.UserType.UserFollows,
                            }, transaction: _transaction);
                        }
                        workUserService.Execute("DELETE App_WorkUser WHERE WorkID = @WorkID AND UserID IN ('" + String.Join("','", arrDeletes) + "') AND UserType = @Type", new
                        {
                            WorkID = id,
                            Type = (int)WorkEnum.UserType.UserFollows
                        }, transaction: _transaction);
                    }
                    else
                    {
                        // add 
                        foreach (var item in userFollows)
                        {
                            if (string.IsNullOrWhiteSpace(item))
                                continue;
                            //
                            WorkUser workUser = workUserService.GetAlls(m => m.WorkID == id && m.UserID == item && m.UserType == (int)WorkEnum.UserType.UserFollows, transaction: _transaction).FirstOrDefault();
                            if (workUser == null)
                            {
                                string guid = workUserService.Create<string>(new WorkUser()
                                {
                                    WorkID = id,
                                    UserID = item,
                                    UserType = (int)WorkEnum.UserType.UserFollows,
                                }, transaction: _transaction);
                            }
                        }
                    }
                    _transaction.Commit();
                    return Notifization.Success(MessageText.UpdateSuccess);
                }
                catch (Exception ex)
                {
                    _transaction.Rollback();
                    return Notifization.Invalid(MessageText.NotService + ex);

                }
            }
        }
        //##############################################################################################################################################################################################################################################################
        public SiteAssiagnOption2 GetWorkAssignOption()
        {
            using (var service = new SiteService())
            {
                Site site = service.GetAlls(m => m.ID == Helper.Current.UserLogin.SiteID).FirstOrDefault();
                if (site == null)
                    return new SiteAssiagnOption2();
                //
                int type = site.Type;
                string sqlQuery = @"SELECT ID, Title, Type, 1 as Cate FROM App_Site as s WHERE Enabled = 1 AND Type > @type UNION  
                SELECT ID, Title, 1 as 'Type', 2 as Cate FROM App_Department as dpmt WHERE Enabled = 1 AND SiteID = @SiteID ORDER BY cate, Type, Title ASC";
                List<SiteAssiagnOption> dtList = service.Query<SiteAssiagnOption>(sqlQuery, new
                {
                    SiteID = Helper.Current.UserLogin.SiteID,
                    Type = type
                }).ToList();
                if (dtList.Count > 0)
                {
                    List<SiteAssiagnOption> dtSite = dtList.Where(m => m.Cate == 1).OrderBy(m => m.Title).ToList();
                    List<SiteAssiagnOption> dtDepartment = dtList.Where(m => m.Cate == 2).OrderBy(m => m.Title).ToList();
                    return new SiteAssiagnOption2()
                    {
                        Internal = dtDepartment,
                        OutSite = dtSite
                    };
                }
                return new SiteAssiagnOption2();
            }
        }
        public List<WorkAssignOptionModel> GetWorkAssignInternal()
        {
            using (var service = new SiteService())
            {
                string sqlQuery = @"SELECT ID, Title, 1 as 'Type', 2 as Cate   FROM App_Department as dpmt WHERE Enabled = 1 AND SiteID = @SiteID ORDER BY Title ASC";
                List<WorkAssignOptionModel> dtList = service.Query<WorkAssignOptionModel>(sqlQuery, new
                {
                    SiteID = Helper.Current.UserLogin.SiteID,
                }).ToList();
                if (dtList.Count == 0)
                    return new List<WorkAssignOptionModel>();
                // 
                return dtList;
            }
        }
        public List<WorkUserOption> WorkUserData(string department)
        {
            if (string.IsNullOrWhiteSpace(department))
                return new List<WorkUserOption>();
            //
            string result = string.Empty;
            using (var service = new UserService())
            {
                string whereCondition = " AND SiteID = @SiteID";
                string sqlQuery = $"SELECT ID, FullName as 'Title' FROM View_User WHERE DepartmentID = @DepartmentID AND Enabled = @Enabled AND IsBlock = 0 {whereCondition} ORDER BY FullName ASC";
                List<WorkUserOption> dtList = service.Query<WorkUserOption>(sqlQuery, new
                {
                    DepartmentID = department,
                    Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED,
                    SiteID = Helper.Current.UserLogin.SiteID,
                }).ToList();
                //
                if (dtList.Count == 0)
                    return new List<WorkUserOption>();
                //  
                return dtList;
            }
        }
        public List<WorkStateOptionModel> WorkStateOption()
        {
            List<WorkStateOptionModel> workStateOptionModels = new List<WorkStateOptionModel>{
                new  WorkStateOptionModel {
                    ID = 1, Title = "Giao việc"
                },
                new  WorkStateOptionModel {
                    ID = 2, Title = "Đã tiếp nhận"
                },
                new  WorkStateOptionModel {
                    ID = 3, Title = "Đang thực hiện"
                },
                 new  WorkStateOptionModel {
                    ID = 4, Title = "Tạm dừng"
                },
                new  WorkStateOptionModel {
                    ID = 5, Title = "Hoàn thành"
                }
            };
            return workStateOptionModels;
        }

        public List<WorkUserOption> WorkUserFollows(WorkUserInDepartmentModel model)
        {
            string department = model.DepartmentID;
            string workId = model.WorkID;
            if (string.IsNullOrWhiteSpace(department) || string.IsNullOrWhiteSpace(workId))
                return new List<WorkUserOption>();
            //
            WorkService workService = new WorkService(_connection);
            List<WorkUserOption> workUserOptions = workService.WorkUserData(department);
            if (workUserOptions.Count == 0)
                return new List<WorkUserOption>();
            //
            WorkUserService workUserService = new WorkUserService(_connection);
            List<string> workUsers = workUserService.GetAlls(m => m.WorkID == workId && m.UserType == (int)WorkEnum.UserType.UserFollows).Select(m => m.UserID).ToList();
            if (workUsers.Count == 0)
                return workUserOptions;
            // 
            foreach (var item in workUserOptions)
            {
                if (workUsers.Contains(item.ID))
                    item.IsSelected = true;
                //
            }
            return workUserOptions;
        }
        public List<WorkUserOption> WorkUserExecutes(WorkUserInDepartmentModel model)
        {
            string department = model.DepartmentID;
            string workId = model.WorkID;
            if (string.IsNullOrWhiteSpace(department) || string.IsNullOrWhiteSpace(workId))
                return new List<WorkUserOption>();
            //
            WorkService workService = new WorkService();
            List<WorkUserOption> workUserOptions = workService.WorkUserData(department);
            if (workUserOptions.Count == 0)
                return new List<WorkUserOption>();
            //
            WorkUserService workUserService = new WorkUserService();
            List<string> workUsers = workUserService.GetAlls(m => m.WorkID == workId && m.UserType == (int)WorkEnum.UserType.UserExecute).Select(m => m.UserID).ToList();
            if (workUsers.Count == 0)
                return workUserOptions;
            // 
            foreach (var item in workUserOptions)
            {
                if (workUsers.Contains(item.ID))
                    item.IsSelected = true;
                //
            }
            return workUserOptions;
        }
        public static string DDLWorkChat(string id)
        {
            string departmentId = "";
            UserSettingService userSettingService = new UserSettingService();
            UserSetting userSetting = userSettingService.GetAlls(m => m.UserID == Helper.Current.UserLogin.IdentifierID).FirstOrDefault();
            if (userSetting != null)
                departmentId = userSetting.DepartmentID;
            //  
            string sqlQuery = $@"SELECT * FROM App_Work WHERE AssignTo = @SiteID OR AssignTo = @DepartmentID OR CreatedBy = @UserID ORDER BY OrderID ASC ";
            //
            WorkService workService = new WorkService();
            List<WorkResult> dataAll = workService.Query<WorkResult>(sqlQuery, new
            {
                DepartmentID = departmentId,
                SiteID = Helper.Current.UserLogin.SiteID,
                UserID = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            //
            if (dataAll.Count == 0)
                return string.Empty;
            //
            string result = "";
            foreach (var item in dataAll)
            {
                string select = string.Empty;
                if (!string.IsNullOrWhiteSpace(id) && item.ID == id.ToLower())
                    select = "selected";
                result += $"<option value='{item.ID }' {select}>&nbsp; &nbsp; &nbsp;{item.Title}</option>";
            }
            return result;
        }
        public static string DDLWorkLevelLinitOne(string id, string notId = null)
        {
            string result = string.Empty;
            using (var service = new WorkService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(notId))
                    whereCondition += " AND ID != @NotID ";
                //
                whereCondition += @" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
                string query = $@"SELECT * FROM App_Work WHERE ParentID IS NULL AND Enabled = 1 {whereCondition} ORDER BY OrderID, Title ASC";
                List<WorkDDLOption> dtList = service.Query<WorkDDLOption>(query, new
                {
                    NotID = notId,
                    SiteID = Helper.Current.UserLogin.SiteID,
                    CreatedBy = Helper.Current.UserLogin.IdentifierID
                }).ToList();
                if (dtList.Count == 0)
                    return result;
                //
                foreach (var item in dtList)
                {
                    string select = string.Empty;
                    if (!string.IsNullOrWhiteSpace(id) && item.ID == id.ToLower())
                        select = "selected";
                    result += "<option value='" + item.ID + "'" + select + ">" + item.Title + "</option>";
                }
                return result;
            }
        }
        public static string DDLWorkOfMember(string id)
        {

            return string.Empty;
        }
        public static string DDLWorkAssignCategory(string id)
        {
            string result = string.Empty;
            using (var service = new WorkService())
            {
                string departmentId = "";
                UserSettingService userSettingService = new UserSettingService();
                UserSetting userSetting = userSettingService.GetAlls(m => m.UserID == Helper.Current.UserLogin.IdentifierID).FirstOrDefault();
                if (userSetting != null)
                    departmentId = userSetting.DepartmentID;
                // 
                string query = $@"SELECT w.*,CASE WHEN (select COUNT(ID) from App_Work where ParentID = w.ID)  > 0 THEN  1 ELSE 0 END IsAssigned
                FROM App_Work as w WHERE w.SiteID = @SiteID OR w.AssignTo = @SiteID OR w.AssignTo = @DepartmentID OR w.CreatedBy = @UserID ORDER BY w.ParentID, w.Title ASC";
                List<WorkDDLOption> dtList = service.Query<WorkDDLOption>(query, new
                {
                    SiteID = Helper.Current.UserLogin.SiteID,
                    DepartmentID = departmentId,
                    UserID = Helper.Current.UserLogin.IdentifierID
                }).ToList();
                //
                if (dtList.Count == 0)
                    return result;
                //
                foreach (var item in dtList)
                {
                    string select = string.Empty;
                    if (!string.IsNullOrWhiteSpace(id) && item.ID == id.ToLower())
                        select = "selected";
                    result += $"<option value='{item.ID }' data-IsAssigned ='{item.IsAssigned}' {select}>&nbsp; &nbsp; &nbsp;{item.Title}</option>";
                }
                return result;
            }
        }
        public static string DDLWorkState(int id)
        {
            string result = string.Empty;
            using (var service = new WorkService())
            {
                List<WorkStateOptionModel> dtList = service.WorkStateOption();
                if (dtList.Count == 0)
                    return result;
                //
                foreach (var item in dtList)
                {
                    string select = string.Empty;
                    if (item.ID == id)
                        select = "selected";
                    result += "<option value='" + item.ID + "'" + select + ">" + item.Title + "</option>";
                }
                return result;
            }
        }
        public static string DDLWorkAssign(string id)
        {
            string result = string.Empty;
            using (var service = new SiteService())
            {
                Site site = service.GetAlls(m => m.ID == Helper.Current.UserLogin.SiteID).FirstOrDefault();
                if (site == null)
                    return string.Empty;
                int type = site.Type;
                // 
                string sqlQuery = @"SELECT ID, Title, Type, 1 as Cate FROM App_Site as s WHERE Enabled = 1 AND Type > @type UNION  
                SELECT ID, Title, 1 as 'Type', 2 as Cate FROM App_Department as dpmt WHERE Enabled = 1 AND SiteID = @SiteID ORDER BY cate, Type, Title ASC";
                List<SiteAssiagnOption> dtList = service.Query<SiteAssiagnOption>(sqlQuery, new
                {
                    SiteID = Helper.Current.UserLogin.SiteID,
                    Type = type
                }).ToList();
                if (dtList.Count > 0)
                {
                    List<SiteAssiagnOption> dtSite = dtList.Where(m => m.Cate == 1).ToList();
                    List<SiteAssiagnOption> dtDepartment = dtList.Where(m => m.Cate == 2).ToList();
                    if (dtSite.Count > 0)
                    {
                        foreach (var item in dtSite)
                        {
                            string select = string.Empty;
                            if (!string.IsNullOrWhiteSpace(id) && item.ID == id.ToLower())
                                select = "selected";
                            //
                            result += $"<option value='{item.ID }' {select} data-type='{item.Cate}'>{item.Title}</option>";
                        }
                        result += "<option data-divider='true'></option>";
                    }
                    //
                    if (dtDepartment.Count > 0)
                    {
                        foreach (var item in dtDepartment)
                        {
                            string select = string.Empty;
                            if (!string.IsNullOrWhiteSpace(id) && item.ID == id.ToLower())
                                select = "selected";
                            //
                            result += $"<option value='{item.ID }' {select} data-type='{item.Cate}'>{item.Title}</option>";
                        }
                    }

                }
                return result;
            }
        }
        public static string GetWorkName(string id)
        {
            using (var service = new WorkService())
            {
                Work Work = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (Work == null)
                    return string.Empty;
                //
                return Work.Title;
            }
        }
        public static string AssignedName(string assignId, int type)
        {
            if (type == (int)WorkEnum.ReceptionType.OutSite)
            {
                using (var service = new SiteService())
                {
                    Site site = service.GetAlls(m => m.ID == assignId).FirstOrDefault();
                    if (site == null)
                        return string.Empty;
                    //
                    return site.Title;
                }
            }
            if (type == (int)WorkEnum.ReceptionType.Department || type == (int)WorkEnum.ReceptionType.UserOfDepartment)
            {
                using (var service = new DepartmentService())
                {
                    Department department = service.GetAlls(m => m.ID == assignId).FirstOrDefault();
                    if (department == null)
                        return string.Empty;
                    //
                    return department.Title;
                }
            }
            return string.Empty;
        }

        //#######################################################################################################################################################################################

        public static string GetWork(string selectedId)
        {
            WorkService WorkService = new WorkService();
            string result = string.Empty;
            string whereCondition = $@" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
            string sqlQuery = $"SELECT * FROM App_Work WHERE Enabled = @Enabled {whereCondition} ORDER DeadLine ASC";
            List<Work> dtList = WorkService.Query<Work>(sqlQuery, new
            {
                Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED,
                SiteID = Helper.Current.UserLogin.SiteID,
                CreatedBy = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            if (dtList.Count() == 0)
                return result;
            // 
            foreach (var item in dtList)
            {
                string _state = string.Empty;
                if (item.ID == selectedId)
                    _state = "checked";
                //
                result += $@"<li><input id='cbx{item.ID}' type='checkbox' class='filled-in' data-id='{item.ID}' {_state} /><label for='cbx{item.ID}'>{item.Title}</label></li>";
            }
            return result;
        }
        public static double GetProgress(string _startTime, string _endTime)
        {
            if (string.IsNullOrWhiteSpace(_startTime) || string.IsNullOrWhiteSpace(_endTime))
                return -1;
            // 
            if (!DateTime.TryParse(_startTime, out _) || !DateTime.TryParse(_endTime, out _))
                return -1;
            //
            DateTime today = Helper.TimeData.TimeHelper.UtcDateTime;
            DateTime startTime = Convert.ToDateTime(_startTime);
            DateTime endTime = Convert.ToDateTime(_endTime);
            double timeTotal = endTime.Subtract(startTime).TotalHours;
            // tinh thoi gian hien tai so với ngày bắt đầu thực hiện
            double timeTodayOnStart = today.Subtract(startTime).TotalHours;
            int totalDay = (endTime - startTime).Days;
            decimal temp01 = 0;
            double result = 0;
            if (totalDay > 1)
            {
                if ((today - startTime).Days <= 0)
                    return 0;
                // 
                temp01 = (today - startTime).Days;
                result = Math.Round(Convert.ToDouble(temp01 / totalDay) * 100, 2);
                if (result > 100)
                    return 100;
                //
                return result;
            }
            // get by hour
            if ((today - startTime).Days <= 0)
                return 0;
            //
            totalDay = (endTime - startTime).Hours;
            temp01 = (today - startTime).Days;
            result = Math.Round(Convert.ToDouble(temp01 / totalDay) * 100, 2);
            if (result > 100)
                return 100;
            //
            return result;
        }
        //#######################################################################################################################################################################################
        private static List<int> WorkAssignType()
        {
            return new List<int>() { 1, 2 };
        }

        private static List<int> WorkState()
        {
            return new List<int>() { 1, 2 };
        }
    }
}