using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using Helper.Language;
using Helper.TimeData;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_WorkPlan")]
    public partial class WorkPlan : WEBModel
    {
        public WorkPlan()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string WorkID { get; set; }
        public string Title { get; set; }
        public string HtmlText { get; set; }
        public string Alias { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime ExecuteDate { get; set; }
        //public string AssignorUserID { get; set; }
        //public string ExecuteUserID { get; set; }
        //public string FollowerUserID { get; set; }
        public bool State { get; set; }
        public int OrderID { get; set; }
    }
    public class WorkPlanCreateModel
    {
        public string WorkID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
        public string Deadline { get; set; }
        public string ExecuteDate { get; set; }
        public bool State { get; set; }
        public int OrderID { get; set; }
        public int Enabled { get; set; }
    }

    public class WorkPlanUpdateModel : WorkPlanCreateModel
    {
        public string ID { get; set; }
    }

    public class WorkPlanIDModel
    {
        public string ID { get; set; }
    }
    public class WorkPlanDeleteAndAtactmentModel
    {
        public string ID { get; set; }
        public string ImageFile { get; set; }
    }
    public class WorkPlanEditModel
    {
        public string ID { get; set; }
        public string Val { get; set; }
        public string Field { get; set; }
    }


    public partial class WorkPlanPageTypeOptionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }

        public WorkPlanPageTypeOptionModel(string Id, string title, string alias)
        {
            ID = Id;
            Title = title;
            Alias = alias;
        }
    }

    // menu list
    public class WorkPlanResult : WEBModelResult
    {
        public string ID { get; set; }
        public string WorkID { get; set; }
        public string WorkName { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; } 
        private string _executeDate;
        public string ExecuteDate
        {
            get
            {
                return TimeFormat.FormatToViewDate(_executeDate, LanguagePage.GetLanguageCode);
            }
            set
            {
                _executeDate = value;
            }
        }
        private string _deadline;
        public string Deadline
        {
            get
            {
                return TimeFormat.FormatToViewDate(_deadline, LanguagePage.GetLanguageCode);
            }
            set
            {
                _deadline = value;
            }
        }

        public bool State { get; set; }
        [NotMapped]
        public int Progress => WorkPlanService.GetProgress(_executeDate, _deadline);

    }

    public class ViewWorkPlanResult : WorkPlanResult
    {
        public string HtmlText { get; set; }

    }
    //

    public class WorkPlanDDLOption
    {
        public string ID { get; set; }
        public int OrderID { get; set; }
        public string Title { get; set; }
        [NotMapped]
        public List<WorkPlanDDLOption> SubOption { get; set; }
    }

    //
    public class WorkPlanOptionModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
    }

}