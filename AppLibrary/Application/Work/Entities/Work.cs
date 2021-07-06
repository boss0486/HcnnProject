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
using WebCore.Model.Enum;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_Work")]
    public partial class Work : WEBModel
    {
        public Work()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string HtmlText { get; set; }
        public string Alias { get; set; }
        public DateTime ExecuteDate { get; set; }
        public DateTime Deadline { get; set; }
        public string AssignTo { get; set; }
        public int ReceptionType { get; set; } // loai don vi tiep nhan vp or site 
        public int AssignType { get; set; } // loai giao viec: giao luon or tao cong viec con
        public DateTime? AssignDate { get; set; }
        public string Path { get; set; }
        public string HtmlNote { get; set; }
        public int State { get; set; }
        public int OrderID { get; set; }
    }
    public class WorkCreateModel
    {
        public string CategoryID { get; set; }
        public string Title { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
        public string ExecuteDate { get; set; }
        public string Deadline { get; set; }
        public List<string> Files { get; set; }
        public int State { get; set; }
    }
    public class WorkUpdateModel : WorkCreateModel
    {
        public string ID { get; set; }
    }
    public class WorkAssignFastModel
    {
        public string WorkID { get; set; }
        public string AssignTo { get; set; }
        public int ReceptionType { get; set; }
    }

    public class WorkAssignCreateModel
    {
        public string CategoryID { get; set; }
        public string AssignTo { get; set; }
        public int ReceptionType { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
        public string ExecuteDate { get; set; }
        public string Deadline { get; set; }
        public List<string> Files { get; set; }
        public int State { get; set; }
    }
    public class WorkAssignUpdateModel : WorkAssignCreateModel
    {
        public string ID { get; set; }
    }

    public class WorkProcessAssignModel
    {
        public string ID { get; set; }
        public string ExecuteDate { get; set; }
        public string Deadline { get; set; }
        public string HtmlNote { get; set; }
        public List<string> UserFollows { get; set; }
        public List<string> UserExecutes { get; set; }
    }
    public class WorkProcessCreateModel
    {
        public string CategoryID { get; set; }
        public string Title { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
        public string ExecuteDate { get; set; }
        public string Deadline { get; set; }
        public List<string> Files { get; set; }
        public string HtmlNote { get; set; }
        public List<string> UserFollows { get; set; }
        public List<string> UserExecutes { get; set; }
    } 
    public class WorkProcessUpdateModel : WorkProcessCreateModel
    {
        public string ID { get; set; }
    }

    public class WorkIDModel
    {
        public string ID { get; set; }
    }
    public class WorkDeleteAndAtactmentModel
    {
        public string ID { get; set; }
        public string ImageFile { get; set; }
    }

    // menu list
    public class WorkResult : WEBModelResult2
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
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

        public int State { get; set; }
        [NotMapped]
        public double Progress => WorkService.GetProgress(_executeDate, _deadline);

        public string AssignTo { get; set; }
        public int AssignType { get; set; }

        public int ReceptionType { get; set; }
        [NotMapped]
        public string ReceptionName => WorkService.AssignedName(AssignTo, ReceptionType);

        [NotMapped]
        public bool IsSub { get; set; } = true;
        [NotMapped]
        public List<WorkResult> SubData { get; set; }
        public string HtmlNote { get; set; }
        public List<string> UserFollows { get; set; }
        public List<string> UserExecutes { get; set; }
    }

    public class ViewWorkResult : WorkResult
    {
        public string HtmlText { get; set; }
        public List<ViewAttachment> Attachments = null;
    }

    //
    public partial class WorkAssign
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public DateTime ExecuteDate { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsAssigned { get; set; }
        public int ReceptionType { get; set; }
        public string AssignTo { get; set; }
        [NotMapped]
        public string ReceptionName => WorkService.AssignedName(AssignTo, ReceptionType);
    }

    public class WorkDDLOption
    {
        public string ID { get; set; }
        public int OrderID { get; set; }
        public string Title { get; set; }
        public bool IsAssigned { get; set; }
    }

    //
    public class WorkOptionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public int AssignType { get; set; }
    }
    public class WorkAssignOptionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int Cate { get; set; }
    }
    public class WorkStateOptionModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
    }

    public class WorkAssignSearchModel : SearchModel
    {
        public string WorkID { get; set; }
    }
    public class WorkUserOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        [NotMapped]
        public bool IsSelected { get; set; } = false;
    }

    public class WorkUserInDepartmentModel
    {
        public string WorkID { get; set; }
        public string DepartmentID { get; set; }
    }

}