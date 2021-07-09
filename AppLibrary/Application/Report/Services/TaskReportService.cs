using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using AppLibrary.Core.Model.ReportModel;
using Dapper;
using Helper;
using Helper.File;
using Helper.Page;
using Helper.Pagination;
using Helper.TimeData;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Dynamic;
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

                if (model.IsExportExcel)
                {
                    return ExportResult(result, model); 
                }
                return SearchResult(result, model);
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

        private ActionResult SearchResult(List<SearchTaskReportResult> result, SearchTaskReportModel model)
        {
            PagingModel pagingModel = new PagingModel
            {
                PageSize = Paging.PAGESIZE,
                Total = result.Count,
                Page = model.Page
            };

            return Notifization.Data(MessageText.Success, data: result, role: RoleActionSettingService.RoleListForUser(), paging: pagingModel);
        }

        private ActionResult ExportResult(List<SearchTaskReportResult> result, SearchTaskReportModel model)
        {
            List<SearchTaskReportResult> dtList = result;
            if (dtList.Count == 0)
                return Notifization.NotFound();
            //     
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                string fileName = model.ReportType == 1 ? "Báo cáo công việc theo phòng ban" : "Báo cáo công việc theo cán bộ";
                string alias = Library.FormatToUni2NONE(fileName);

                ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("Bao cao");
                workSheet.TabColor = Color.Black;
                workSheet.DefaultRowHeight = 12;
                //Header of table   
                workSheet.Cells["A1:H1"].Merge = true;
                workSheet.Cells["A1:H1"].Value = fileName;
                workSheet.Cells["A1:H1"].Style.Font.Name = "Arial Unicode MS";
                workSheet.Cells["A1:H1"].Style.Font.Size = 16;
                workSheet.Row(1).Height = 35;
                workSheet.Cells["A1:H1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                workSheet.Cells["A1:H1"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                workSheet.Cells["A1:H1"].Style.Font.Bold = true;
                workSheet.Cells["A1:H1"].Style.Font.Color.SetColor(Color.White);
                workSheet.Cells["A1:H1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(Color.Teal);
                workSheet.Cells["A1:H1"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                workSheet.Cells["A1:H1"].Style.Border.Bottom.Color.SetColor(Color.Gray);
                //title of table  
                workSheet.Row(2).Height = 20;
                workSheet.Cells[2, 1].Value =  "STT";
                workSheet.Cells[2, 2].Value = model.ReportType == 1 ? "Tên phòng ban" : "Tên cán bộ";
                workSheet.Cells[2, 3].Value = "Tổng số công việc";
                workSheet.Cells[2, 4].Value = "Đã giao việc";
                workSheet.Cells[2, 5].Value = "Đang thực hiện";
                workSheet.Cells[2, 6].Value = "Hoàn thành";
                workSheet.Cells[2, 7].Value = "Tạm dừng";
                workSheet.Cells[2, 8].Value = "Quá hạn";


                //Style of table  
                workSheet.Cells["A2:H2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells["A2:H2"].Style.Fill.BackgroundColor.SetColor(Color.Teal);
                workSheet.Cells["A2:H2"].Style.Font.Color.SetColor(Color.White);
                workSheet.Cells["A2:H2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                //

                dynamic sumTask = new ExpandoObject();
                sumTask.Total = 0;
                sumTask.Assigned = 0;
                sumTask.Inprogress = 0;
                sumTask.Completed = 0;
                sumTask.Pause = 0;
                sumTask.OutDate = 0;

                int recordIndex = 3;
                int cnt = 1;
                // data to sheet
                foreach (var item in dtList)
                {
                    sumTask.Total += item.Total;
                    sumTask.Assigned += item.Total_Assigned;
                    sumTask.Inprogress += item.Total_Inprogress;
                    sumTask.Completed += item.Total_Completed;
                    sumTask.Pause += item.Total_Pause;
                    sumTask.OutDate += item.Total_OutDate;

                    workSheet.Cells[recordIndex, 1].Value = cnt;
                    workSheet.Cells[recordIndex, 2].Value = item.Assignee;
                    workSheet.Cells[recordIndex, 3].Value = item.Total;
                    workSheet.Cells[recordIndex, 4].Value = item.Total_Assigned;
                    workSheet.Cells[recordIndex, 5].Value = item.Total_Inprogress;
                    workSheet.Cells[recordIndex, 6].Value = item.Total_Completed;
                    workSheet.Cells[recordIndex, 7].Value = item.Total_Pause;
                    workSheet.Cells[recordIndex, 8].Value = item.Total_OutDate;
                    ////// attribute 
                    workSheet.Cells[recordIndex, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[recordIndex, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[recordIndex, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[recordIndex, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[recordIndex, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[recordIndex, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[recordIndex, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    workSheet.Cells[recordIndex, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    
                    recordIndex++;
                    cnt++;
                }

                var rowTotalStyle = "A" + recordIndex + ":B" + recordIndex;
                workSheet.Cells[rowTotalStyle].Merge = true;
                workSheet.Cells[rowTotalStyle].Value = "Tổng";
                workSheet.Cells[rowTotalStyle].Style.Fill.PatternType = ExcelFillStyle.Solid;
                workSheet.Cells[rowTotalStyle].Style.Fill.BackgroundColor.SetColor(Color.Teal);
                workSheet.Cells[rowTotalStyle].Style.Font.Color.SetColor(Color.White);
                workSheet.Cells[rowTotalStyle].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                workSheet.Cells[recordIndex, 3].Value = sumTask.Total;
                workSheet.Cells[recordIndex, 4].Value = sumTask.Assigned;
                workSheet.Cells[recordIndex, 5].Value = sumTask.Inprogress;
                workSheet.Cells[recordIndex, 6].Value = sumTask.Completed;
                workSheet.Cells[recordIndex, 7].Value = sumTask.Pause;
                workSheet.Cells[recordIndex, 8].Value = sumTask.OutDate;

                for (int i = 1; i < 9; i++)
                {
                    workSheet.Column(i).AutoFit();
                }

                // create file
                string outFolder = "/Files/Export/Report/";
                string pathFile = AttachmentFile.AttachmentExls(alias, excelPackage, outFolder);
                if (string.IsNullOrWhiteSpace(pathFile))
                    return null;
                //
                return Notifization.DownLoadFile(MessageText.DownLoad, pathFile);
            }
        }


        //##############################################################################################################################################################################################################################################################

    }
}
