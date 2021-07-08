using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using AppLibrary.Core.Model.ReportModel;
using Dapper;
using Helper;
using Helper.File;
using Helper.Page;
using Helper.Pagination;
using Helper.TimeData;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using WebCore.ENM;
using WebCore.Entities;
using WebCore.Model.Entities;

namespace WebCore.Services
{
    public interface ITaskReportService : IEntityService<DbConnection> { }
    public class TaskReportService : EntityService<DbConnection>, ITaskReportService
    {

        public TaskReportService() : base() { }
        public TaskReportService(System.Data.IDbConnection db) : base(db) { }


        //###############################################################################################################################
        public ActionResult DataList(SearchTaskReportModel model)
        {
            int page = model.Page;

            string sqlQuery = GetSqlQuery(model);

            using (var service = new TaskReportService())
            {
                var dtList = _connection.Query<TaskReportModel>(sqlQuery, new
                {
                    Query = string.Empty,
                    SiteID = Helper.Current.UserLogin.SiteID
                }).ToList();
                if (dtList.Count == 0)
                    return Notifization.NotFound(MessageText.NotFound);

                var result = CalculateResult(dtList);

                PagingModel pagingModel = new PagingModel
                {
                    PageSize = Paging.PAGESIZE,
                    Total = result.Count,
                    Page = page
                };

                return Notifization.Data(MessageText.Success, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
            }
        }

        private List<SearchTaskReportResult> CalculateResult(List<TaskReportModel> reportModels)
        {
            var lstResult = new List<SearchTaskReportResult>();

            var assigneeIds = reportModels.Select(x => x.AssigneeId).Distinct().ToList();

            for (int i = 0; i < assigneeIds.Count; i++)
            {
                var workByAssignee = reportModels.Where(x => x.AssigneeId == assigneeIds[i]);
                var result = new SearchTaskReportResult
                {
                    Assignee = workByAssignee.Select(x => x.AssigneeName).FirstOrDefault(),
                    Total = workByAssignee.Count(),
                    Total_Assigned = workByAssignee.Count(x => x.State == (int)WorkEnum.State.Assigned),
                    Total_Inprogress = workByAssignee.Count(x => x.State == (int)WorkEnum.State.Processing),
                    Total_Completed = workByAssignee.Count(x => x.State == (int)WorkEnum.State.Completed),
                    Total_Pause = workByAssignee.Count(x => x.State == (int)WorkEnum.State.Pause),
                    Total_OutDate = workByAssignee.Where(x => x.CreatedDate > DateTime.Now).Count()
                };
                lstResult.Add(result);
            }
            return lstResult;
        }

        private string GetSqlQuery(SearchTaskReportModel model)
        {
            string whereCondition = string.Empty;
            string sqlQuery = string.Empty;
            //Report by phong ban
            if (model.ReportType == 1)
            {
                if (!string.IsNullOrWhiteSpace(model.DepartmentId))
                {
                    whereCondition = $@"WHERE A.AssignTo = '{model.DepartmentId}'";
                }
                sqlQuery = $@"
                       SELECT A.AssignTo AS 'AssigneeId',
                              B.Title AS 'AssigneeName',
			                  A.[State] AS 'State',
			                  A.CreatedDate
		                FROM [App_Work] A JOIN [dbo].[App_Department] B ON A.AssignTo = B.ID AND A.ReceptionType=2
		                 {whereCondition} ORDER BY A.CreatedDate DESC";
            }

            //Report by user
            if (model.ReportType == 2)
            {
                if (!string.IsNullOrWhiteSpace(model.EmpId))
                {
                    whereCondition = $@"WHERE C.ID = '{model.EmpId}'";
                }
                sqlQuery = $@"SELECT C.ID AS 'AssigneeId',
	                           C.FullName AS 'AssigneeName',
	                           A.[State] AS 'State',
	                           A.CreatedDate
                        FROM [dbo].[App_Work] A JOIN [dbo].[App_WorkUser] B ON A.ID = B.WorkID AND B.UserType = 1
						                        JOIN [dbo].[View_User] C ON B.UserID = C.ID
                        {whereCondition} ORDER BY A.CreatedDate DESC";
            }
            return sqlQuery;
        }

        //##############################################################################################################################################################################################################################################################

    }
}
