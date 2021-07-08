using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCore.ENM;
using WebCore.Model.Entities;

namespace AppLibrary.Core.Model.ReportModel
{
    public class TaskReportModel
    {
        public string AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public int State { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SearchTaskReportModel : SearchModel
    {
        public string EmpId { get; set; }
        public string DepartmentId { get; set; }
        public int ReportType { get; set; }
    }

    public class SearchTaskReportResult
    {
        public string Assignee { get; set; }
        public int Total { get; set; }
        public int Total_Assigned { get; set; }
        public int Total_Inprogress { get; set; }
        public int Total_Completed { get; set; }
        public int Total_Pause { get; set; }
        public int Total_OutDate { get; set; }
    }
}
