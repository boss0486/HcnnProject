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
    public interface IWorkPlanService : IEntityService<WorkPlan> { }
    public class WorkPlanService : EntityService<WorkPlan>, IWorkPlanService
    {
        public WorkPlanService() : base() { }
        public WorkPlanService(System.Data.IDbConnection db) : base(db) { }
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
            whereCondition += $@" AND t.SiteID = @SiteID AND t.CreatedBy = @UserID";
            string sqlQuery = $@"SELECT t.*,w.Title as 'WorkName' FROM App_WorkPlan as t RIGHT JOIN App_Work as w ON t.WorkID = w.ID
            WHERE t.Title LIKE N'%'+ dbo.Uni2NONE(@Query) +'%' {whereCondition} ORDER BY w.CreatedDate, t.CreatedDate DESC ";
            //
            List<WorkPlanResult> dtList = _connection.Query<WorkPlanResult>(sqlQuery, new
            {
                Query = Helper.Page.Library.FormatNameToUni2NONE(query),
                SiteID = Helper.Current.UserLogin.SiteID,
                UserID = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            //
            if (dtList.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            // 
            List<WorkPlanResult> result = dtList.ToPagedList(page, Helper.Pagination.Paging.PAGESIZE).ToList();
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

        public ActionResult GetWorkPlanOption(int showLevel = 0)
        {
            WorkPlanService WorkPlanService = new WorkPlanService(_connection);
            string langID = Helper.Current.UserLogin.LanguageID;
            string whereCondition = $@" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
            string sqlQuery = $"SELECT * FROM App_WorkPlan WHERE Enabled = @Enabled {whereCondition} ORDER BY ParentID, OrderID ASC";
            List<WorkPlanOptionModel> WorkPlanOptionModels = _connection.Query<WorkPlanOptionModel>(sqlQuery, new
            {
                Enabled = (int)WebCore.Model.Enum.ModelEnum.State.ENABLED,
                SiteID = Helper.Current.UserLogin.SiteID,
                CreatedBy = Helper.Current.UserLogin.IdentifierID
            }).ToList();
            if (WorkPlanOptionModels.Count == 0)
                return Notifization.NotFound(MessageText.NotFound);
            // 
            return Notifization.Data(MessageText.Success, WorkPlanOptionModels);
        }
        public ActionResult Create(WorkPlanCreateModel model)
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
                    string title = model.Title;
                    string htmlText = model.HtmlText;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    int sortType = model.OrderID;

                    if (string.IsNullOrWhiteSpace(workId))
                        return Notifization.Invalid("Chọn công việc");
                    // 
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Nhập tên tác vụ");
                    //
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tên tác vụ không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tên tác vụ giới hạn [2-80] ký tự");
                    // summary valid               
                    if (string.IsNullOrWhiteSpace(htmlText))
                        return Notifization.Invalid("Nhập nội dung");
                    //
                    if (!Validate.TestText(htmlText))
                        return Notifization.Invalid("Nội dung không hợp lệ");
                    if (htmlText.Length < 1 || htmlText.Length > 5000)
                        return Notifization.Invalid("Nội dung giới hạn [1-5000] ký tự");
                    if (string.IsNullOrWhiteSpace(executeDate))
                        return Notifization.Invalid("Nhập ngày thực hiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày thực hiện không hợp lệ");
                    //     
                    if (string.IsNullOrWhiteSpace(deadline))
                        return Notifization.Invalid("Nhập ngày hoàn thiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày hoàn thiện không hợp lệ");
                    //    
                    WorkService workService = new WorkService(_connection);
                    WorkPlanService WorkPlanService = new WorkPlanService(_connection);
                    WorkPlan WorkPlanTitle = WorkPlanService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.WorkID == workId, transaction: _transaction).FirstOrDefault();
                    if (WorkPlanTitle != null)
                        return Notifization.Invalid("Tên tác vụ đã được sử dụng");
                    // 
                    Work work = workService.GetAlls(m => m.ID == workId, transaction: _transaction).FirstOrDefault();
                    if (work == null)
                        return Notifization.Invalid("Công việc không hợp lệ");
                    //  
                    //int orderId = 1;
                    //if (sortType == (int)AttachmentEnum.Sort.FIRST)
                    //{
                    //    List<WorkPlan> attachmentCategories = WorkPlanService.GetAlls(transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                    //    if (attachmentCategories.Count > 0)
                    //    {
                    //        int cnt = 2;
                    //        foreach (var item in attachmentCategories)
                    //        {
                    //            item.OrderID = cnt;
                    //            WorkPlanService.Update(item, transaction: _transaction);
                    //            cnt++;
                    //        }
                    //    }
                    //    //
                    //}
                    //else
                    //{
                    //    orderId = WorkPlanService.GetAlls(transaction: _transaction).Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
                    //}
                    //
                    AttachmentIngredientService attachmentIngredientService = new AttachmentIngredientService(_connection);
                    string _controllerText = Helper.Page.MetaSEO.ControllerText.ToLower();
                    //
                    string id = WorkPlanService.Create<string>(new WorkPlan()
                    {
                        WorkID = workId,
                        Title = title,
                        Alias = Helper.Page.Library.FormatToUni2NONE(title),
                        HtmlText = htmlText,
                        ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate),
                        Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline),
                        State = false,
                        Enabled = model.Enabled,
                    }, transaction: _transaction);

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
        public ActionResult Update(WorkPlanUpdateModel model)
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
                    string id = model.ID;
                    string title = model.Title;
                    string htmlText = model.HtmlText;
                    string executeDate = model.ExecuteDate;
                    string deadline = model.Deadline;
                    bool state = model.State;
                    ///int sortType = model.OrderID;
                    if (string.IsNullOrWhiteSpace(title))
                        return Notifization.Invalid("Nhập tên tác vụ");
                    //
                    title = title.Trim();
                    if (!Validate.TestText(title))
                        return Notifization.Invalid("Tên tác vụ không hợp lệ");
                    if (title.Length < 2 || title.Length > 80)
                        return Notifization.Invalid("Tên tác vụ giới hạn [2-80] ký tự");
                    // summary valid               
                    if (string.IsNullOrWhiteSpace(htmlText))
                        return Notifization.Invalid("Nhập nội dung");
                    //
                    if (!Validate.TestText(htmlText))
                        return Notifization.Invalid("Nội dung không hợp lệ");
                    if (htmlText.Length < 1 || htmlText.Length > 5000)
                        return Notifization.Invalid("Nội dung giới hạn [1-5000] ký tự");
                    //
                    //if (string.IsNullOrWhiteSpace(assignorUserId))
                    //    return Notifization.Invalid("Vui lòng chọn người giao việc");
                    ////
                    //if (string.IsNullOrWhiteSpace(executeUserId))
                    //    return Notifization.Invalid("Vui lòng chọn người thực hiện");
                    ////
                    //if (string.IsNullOrWhiteSpace(followerUserId))
                    //    return Notifization.Invalid("Vui lòng chọn người theo dõi");
                    ////
                    if (string.IsNullOrWhiteSpace(executeDate))
                        return Notifization.Invalid("Nhập ngày thực hiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày thực hiện không hợp lệ");
                    //     
                    if (string.IsNullOrWhiteSpace(deadline))
                        return Notifization.Invalid("Nhập ngày hoàn thiện");
                    //
                    if (!Validate.TestDate(deadline))
                        return Notifization.Invalid("Ngày hoàn thiện không hợp lệ");
                    //   
                    WorkPlanService WorkPlanService = new WorkPlanService(_connection);
                    WorkPlan WorkPlan = WorkPlanService.GetAlls(m => m.ID == id, transaction: _transaction).FirstOrDefault();
                    if (WorkPlan == null)
                        return Notifization.Invalid(MessageText.Invalid);
                    //
                    WorkService workService = new WorkService(_connection);
                    WorkPlan WorkPlanTitle = WorkPlanService.GetAlls(m => m.Title.ToLower() == title.ToLower() && m.WorkID == workId && m.ID != id, transaction: _transaction).FirstOrDefault();
                    if (WorkPlanTitle != null)
                        return Notifization.Invalid("Tên tác vụ đã được sử dụng");
                    // 
                    Work work = workService.GetAlls(m => m.ID == workId, transaction: _transaction).FirstOrDefault();
                    if (work == null)
                        return Notifization.Invalid("Công việc không hợp lệ");
                    // 
                    //int orderId = WorkPlan.OrderID;
                    //if (sortType == (int)AttachmentEnum.Sort.FIRST)
                    //{
                    //    orderId = 1;
                    //    List<WorkPlan> WorkPlans1 = WorkPlanService.GetAlls(transaction: _transaction).OrderBy(m => m.OrderID).ToList();
                    //    if (WorkPlans1.Count > 0)
                    //    {
                    //        int cnt = 2;
                    //        foreach (var item in WorkPlans1)
                    //        {
                    //            item.OrderID = cnt;
                    //            WorkPlanService.Update(item);
                    //            cnt++;
                    //        }
                    //    }
                    //    //
                    //}
                    //if (sortType == (int)AttachmentEnum.Sort.LAST)
                    //{
                    //    orderId = WorkPlanService.GetAlls().Select(t => t.OrderID).DefaultIfEmpty().ToList().Max() + 1;
                    //}
                    // 
                    // update content  
                    WorkPlan.Title = title;
                    WorkPlan.Alias = Helper.Page.Library.FormatToUni2NONE(title);
                    WorkPlan.HtmlText = htmlText;
                    WorkPlan.ExecuteDate = Helper.TimeData.TimeFormat.FormatToServerDate(executeDate);
                    WorkPlan.Deadline = Helper.TimeData.TimeFormat.FormatToServerDate(deadline);
                    WorkPlan.State = state;
                    WorkPlan.Enabled = (int)ModelEnum.State.ENABLED;
                    WorkPlanService.Update(WorkPlan, transaction: _transaction);
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
        public WorkPlan GetWorkPlanByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_WorkPlan WHERE ID = @Query";
            WorkPlan WorkPlan = _connection.Query<WorkPlan>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (WorkPlan == null)
                return null;
            //
            return WorkPlan;
        }

        public ViewWorkPlanResult ViewWorkPlanByID(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return null;
            //
            string langID = Helper.Current.UserLogin.LanguageID;
            string sqlQuery = @"SELECT TOP (1) * FROM App_WorkPlan WHERE ID = @Query";
            ViewWorkPlanResult viewWorkPlan = _connection.Query<ViewWorkPlanResult>(sqlQuery, new { Query = id }).FirstOrDefault();
            if (viewWorkPlan == null)
                return null;
            //
            return viewWorkPlan;
        }
        //########################################################################tttt######################################################################################################################################################################################
        public ActionResult Delete(WorkPlanIDModel model)
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

                    // delete WorkPlan
                    id = id.ToLower();
                    WorkPlanService WorkPlanService = new WorkPlanService(_connection);
                    WorkPlan WorkPlan = WorkPlanService.GetAlls(m => m.ID == id, transaction:_transaction).FirstOrDefault();
                    if (WorkPlan == null)
                        return Notifization.NotFound();
                    //
                    WorkPlanService.Remove(id, transaction: _transaction);
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
        //##############################################################################################################################################################################################################################################################
        public static List<WorkPlanOptionModel> WorkPlanStateOption()
        {
            List<WorkPlanOptionModel> WorkPlanOptionModels = new List<WorkPlanOptionModel>{
                new  WorkPlanOptionModel {
                    ID = 1, Title = "Giao việc"
                },
                new  WorkPlanOptionModel {
                    ID = 2, Title = "Tiếp nhận"
                },
                new  WorkPlanOptionModel {
                    ID = 3, Title = "Đang thực hiện"
                },
                 new  WorkPlanOptionModel {
                    ID = 4, Title = "Tạm dừng"
                },
                new  WorkPlanOptionModel {
                    ID = 5, Title = "Hoàn thành"
                }
            };
            return WorkPlanOptionModels;
        }


        public static string DDLWorkPlanLevelLinitOne(string id, string notId = null)
        {
            string result = string.Empty;
            using (var service = new WorkPlanService())
            {
                string whereCondition = string.Empty;
                if (!string.IsNullOrWhiteSpace(notId))
                    whereCondition += " AND ID != @NotID ";
                //
                whereCondition += @" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
                string query = $@"SELECT * FROM App_WorkPlan WHERE ParentID IS NULL AND Enabled = 1 {whereCondition} ORDER BY OrderID, Title ASC";
                List<WorkPlanDDLOption> dtList = service.Query<WorkPlanDDLOption>(query, new
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

        public static string DDLWorkPlanState(int id)
        {
            string result = string.Empty;
            using (var service = new WorkPlanService())
            {
                List<WorkPlanOptionModel> dtList = WorkPlanService.WorkPlanStateOption();
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
        //##############################################################################################################################################################################################################################################################
        //public ActionResult SortUp(WorkPlanIDModel model)
        //{
        //    _connection.Open();
        //    using (var _transaction = _connection.BeginTransaction())
        //    {
        //        try
        //        {
        //            WorkPlanService WorkPlanService = new WorkPlanService(_connection);
        //            WorkPlan WorkPlan = WorkPlanService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
        //            if (WorkPlan == null)
        //                return Notifization.Invalid(MessageText.Invalid);
        //            int _orderId = WorkPlan.OrderID;
        //            // list first
        //            IList<WorkPlanSortModel> lstFirst = new List<WorkPlanSortModel>();
        //            // list last
        //            IList<WorkPlanSortModel> lstLast = new List<WorkPlanSortModel>();
        //            //
        //            List<WorkPlan> WorkPlans = WorkPlanService.GetAlls(m => m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
        //            if (WorkPlans.Count > 0)
        //            {
        //                foreach (var item in WorkPlans)
        //                {
        //                    // set list first
        //                    if (item.OrderID < WorkPlan.OrderID)
        //                    {
        //                        lstFirst.Add(new WorkPlanSortModel
        //                        {
        //                            ID = item.ID,
        //                            OrderID = item.OrderID
        //                        });
        //                    }
        //                    // set list last
        //                    if (item.OrderID > WorkPlan.OrderID)
        //                    {
        //                        lstLast.Add(new WorkPlanSortModel
        //                        {
        //                            ID = item.ID,
        //                            OrderID = item.OrderID
        //                        });
        //                    }
        //                }
        //                //  first
        //                int _cntFirst = 1;
        //                if (lstFirst.Count > 0)
        //                {
        //                    for (int i = 0; i < lstFirst.Count; i++)
        //                    {
        //                        if (i == lstFirst.Count - 1)
        //                        {
        //                            WorkPlan.OrderID = _cntFirst;
        //                            WorkPlanService.Update(WorkPlan, transaction: _transaction);
        //                            _cntFirst++;
        //                        }
        //                        WorkPlan itemFirst = WorkPlanService.GetAlls(m => m.ID == lstFirst[i].ID, transaction: _transaction).FirstOrDefault();
        //                        itemFirst.OrderID = _cntFirst;
        //                        WorkPlanService.Update(itemFirst, transaction: _transaction);
        //                        _cntFirst++;
        //                    }

        //                }
        //                else
        //                {
        //                    WorkPlan.OrderID = 1;
        //                    WorkPlanService.Update(WorkPlan, transaction: _transaction);
        //                    _cntFirst++;
        //                }
        //                //last
        //                int _cntLast = _cntFirst;
        //                if (lstLast.Count > 0)
        //                {
        //                    foreach (var item in lstLast)
        //                    {
        //                        WorkPlan itemLast = WorkPlanService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
        //                        itemLast.OrderID = _cntLast;
        //                        WorkPlanService.Update(itemLast, transaction: _transaction);
        //                        _cntLast++;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                WorkPlan.OrderID = 1;
        //                WorkPlanService.Update(WorkPlan, transaction: _transaction);
        //            }
        //            _transaction.Commit();
        //            return Notifization.Success(MessageText.UpdateSuccess);
        //        }
        //        catch (Exception ex)
        //        {
        //            _transaction.Rollback();
        //            return Notifization.TEST("::" + ex);
        //        }
        //    }// end transaction
        //}
        //#######################################################################################################################################################################################
        //public ActionResult SortDown(WorkPlanIDModel model)
        //{
        //    _connection.Open();
        //    using (var _transaction = _connection.BeginTransaction())
        //    {
        //        try
        //        {
        //            WorkPlanService WorkPlanService = new WorkPlanService(_connection);
        //            WorkPlan WorkPlan = WorkPlanService.GetAlls(m => m.ID == model.ID, transaction: _transaction).FirstOrDefault();
        //            if (WorkPlan == null)
        //                return Notifization.TEST("::");
        //            int _orderId = WorkPlan.OrderID;
        //            // list first
        //            IList<WorkPlanSortModel> lstFirst = new List<WorkPlanSortModel>();
        //            // list last
        //            IList<WorkPlanSortModel> lstLast = new List<WorkPlanSortModel>();
        //            //
        //            List<WorkPlan> WorkPlans = WorkPlanService.GetAlls(m => m.ID != model.ID, transaction: _transaction).OrderBy(m => m.OrderID).ToList();
        //            if (WorkPlans.Count > 0)
        //            {
        //                foreach (var item in WorkPlans)
        //                {
        //                    // set list first
        //                    if (item.OrderID < WorkPlan.OrderID)
        //                    {
        //                        lstFirst.Add(new WorkPlanSortModel
        //                        {
        //                            ID = item.ID,
        //                            OrderID = item.OrderID
        //                        });
        //                    }
        //                    // set list last
        //                    if (item.OrderID > WorkPlan.OrderID)
        //                    {
        //                        lstLast.Add(new WorkPlanSortModel
        //                        {
        //                            ID = item.ID,
        //                            OrderID = item.OrderID
        //                        });
        //                    }
        //                }
        //                // xu ly
        //                int _cntFirst = 1;
        //                if (lstFirst.Count > 0)
        //                {
        //                    foreach (var item in lstFirst)
        //                    {
        //                        WorkPlan itemFirst = WorkPlanService.GetAlls(m => m.ID == item.ID, transaction: _transaction).FirstOrDefault();
        //                        itemFirst.OrderID = _cntFirst;
        //                        WorkPlanService.Update(itemFirst, transaction: _transaction);
        //                        _cntFirst++;
        //                    }

        //                }
        //                //  last
        //                int _cntLast = _cntFirst;
        //                if (lstLast.Count == 1)
        //                {
        //                    WorkPlan itemLast = WorkPlanService.GetAlls(m => m.ID == lstLast[0].ID, transaction: _transaction).FirstOrDefault();
        //                    itemLast.OrderID = _cntLast;
        //                    WorkPlanService.Update(itemLast, transaction: _transaction);
        //                    //
        //                    WorkPlan.OrderID = _cntLast + 1;
        //                    WorkPlanService.Update(WorkPlan, transaction: _transaction);
        //                    _cntLast++;
        //                }
        //                else if (lstLast.Count > 1)
        //                {
        //                    for (int i = 0; i < lstLast.Count; i++)
        //                    {
        //                        if (i == 1)
        //                        {
        //                            WorkPlan.OrderID = _cntLast;
        //                            WorkPlanService.Update(WorkPlan, transaction: _transaction);
        //                            _cntLast++;
        //                        }
        //                        WorkPlan itemLast = WorkPlanService.GetAlls(m => m.ID == lstLast[i].ID, transaction: _transaction).FirstOrDefault();
        //                        itemLast.OrderID = _cntLast;
        //                        WorkPlanService.Update(itemLast, transaction: _transaction);
        //                        _cntLast++;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                WorkPlan.OrderID = 1;
        //                WorkPlanService.Update(WorkPlan, transaction: _transaction);
        //            }
        //            _transaction.Commit();
        //            return Notifization.Success(MessageText.UpdateSuccess);
        //        }
        //        catch (Exception ex)
        //        {
        //            _transaction.Rollback();
        //            return Notifization.TEST("::" + ex);
        //        }
        //    }// end transaction
        //}
        //#######################################################################################################################################################################################
        public static string GetWorkPlanName(string id)
        {
            using (var service = new WorkPlanService())
            {
                WorkPlan WorkPlan = service.GetAlls(m => m.ID == id).FirstOrDefault();
                if (WorkPlan == null)
                    return string.Empty;
                //
                return WorkPlan.Title;
            }
        }

        //#######################################################################################################################################################################################

        public static string GetWorkPlan(string selectedId)
        {
            WorkPlanService WorkPlanService = new WorkPlanService();
            string result = string.Empty;
            string whereCondition = $@" AND SiteID = @SiteID AND CreatedBy = @CreatedBy";
            string sqlQuery = $"SELECT * FROM App_WorkPlan WHERE Enabled = @Enabled {whereCondition} ORDER DeadLine ASC";
            List<WorkPlan> dtList = WorkPlanService.Query<WorkPlan>(sqlQuery, new
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
        public static int GetProgress(string _executeDate, string _deadline)
        {
            if (string.IsNullOrWhiteSpace(_executeDate) || string.IsNullOrWhiteSpace(_deadline))
                return -1;
            // 
            if (!DateTime.TryParse(_executeDate, out _) || !DateTime.TryParse(_deadline, out _))
                return -1;
            //
            DateTime today = Helper.TimeData.TimeHelper.UtcDateTime;
            DateTime dtExecuteDate = Convert.ToDateTime(_executeDate);
            DateTime dtDeadline = Convert.ToDateTime(_deadline);
            double timeTotal = dtDeadline.Subtract(dtExecuteDate).TotalHours;
            // tinh thoi gian hien tai so với ngày bắt đầu thực hiện
            double timeTodayOnStart = today.Subtract(dtExecuteDate).TotalHours;
            if (timeTotal <= 8)
            {

            }

            // 40 %
            // 60 %
            // 80 %
            // 100%


            // tong thời gian =  _deadline -  _executeDate


            return -1;
        }


        //#######################################################################################################################################################################################

    }
}