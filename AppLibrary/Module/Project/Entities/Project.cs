using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using Helper.File;
using Helper.Language;
using Helper.TimeData;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_Project")]
    public partial class Project : WEBModel
    {
        public Project()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string MenuID { get; set; }
        public string CategoryID { get; set; }
        public string GroupID { get; set; }
        [AllowHtml]
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TextID { get; set; }  
        [AllowHtml]
        public string Summary { get; set; } 
        [AllowHtml]
        public string HtmlNote { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
        public string Tag { get; set; }
        public string ImageFile { get; set; }
        public int ViewTotal { get; set; }
        public DateTime? ViewDate { get; set; }
        [NotMapped]
        public int DocumentFile { get; set; }
    }
    // model
    public class ProjectCreateModel
    {
        public string MenuID { get; set; }
        public string CategoryID { get; set; }
        public string GroupID { get; set; }
        [AllowHtml]
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TextID { get; set; }
        [AllowHtml]
        public string Summary { get; set; } 
        [AllowHtml]
        public string HtmlNote { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
        public string Tag { get; set; }
        public string ImageFile { get; set; }
        public int ViewTotal { get; set; }
        public string ViewDate { get; set; }
        public int Enabled { get; set; }
        [NotMapped]
        public HttpPostedFileBase DocumentFile { get; set; }
    }
    public class ProjectUpdateModel : ProjectCreateModel
    {
        public string ID { get; set; }
    }
    public class ProjectIDModel
    {
        public string ID { get; set; }
    }
    public class ProjectResult : WEBModelResult
    {

        public string ID { get; set; }
        public string MenuID { get; set; }
        public string CategoryID { get; set; }
        public string GroupID { get; set; }
        [NotMapped]
        public string CategoryText => ProjectGroupService.GetProjectGroupName(GroupID);
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TextID { get; set; }
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);

        public string Summary { get; set; } = string.Empty;
        public string HtmlText { get; set; } = string.Empty;
        public string Tag { get; set; }
        public string ViewTotal { get; set; }

        private string _viewDate;
        public string ViewDate {
            get
            {
                if (_viewDate == null)
                    return "../" + "../" + "..";
                return TimeFormat.FormatToViewDate(_viewDate, LanguagePage.GetLanguageCode);
            }
            set
            {
                _viewDate = value;
            }
        }


    }

    public class ProjectHome  
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TextID { get; set; } = string.Empty;
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);

        public string Summary { get; set; } = string.Empty;
        public string ViewTotal { get; set; }

        private string _viewDate;
        public string ViewDate
        {
            get
            {
                if (_viewDate == null)
                    return "../" + "../" + "..";
                return TimeFormat.FormatToViewDate(_viewDate, LanguagePage.GetLanguageCode);
            }
            set
            {
                _viewDate = value;
            }
        }


    }
    public class ProjectOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }

    public class ProjectStatusModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public ProjectStatusModel(int Id, string title)
        {
            ID = Id;
            Title = title;
        }
    }
    public class ProjectSearchModel
    {
        public string Query { get; set; }
        public string GroupID { get; set; }
        public int State { get; set; }
        public int Status { get; set; }
        public int Page { get; set; }
    }

    public class ProjectOtherPatial
    {
        public string GroupID { get; set; }
        public string ID { get; set; }
    }
}