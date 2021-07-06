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
    [Table("App_About")]
    public partial class About : WEBModel
    {
        public About()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string MenuID { get; set; }
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
        public bool IsShow { get; set; }
    }
    // model
    public class AboutCreateModel
    {
        public string MenuID { get; set; }
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
        public int IsShow { get; set; }
    }
    public class AboutUpdateModel : AboutCreateModel
    {
        public string ID { get; set; }
    }
    public class AboutIDModel
    {
        public string ID { get; set; }
    }
    public class AboutResult : WEBModelResult
    {

        public string ID { get; set; }
        public string MenuID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TextID { get; set; }
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);

        public string Summary { get; set; }
        public string HtmlText { get; set; }
        public string Tag { get; set; }
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
    public class AboutHome
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TextID { get; set; }
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);

        public string Summary { get; set; }
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
        public bool IsShow { get; set; }

    }
    public class AboutOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }

    public class AboutStatusModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public AboutStatusModel(int Id, string title)
        {
            ID = Id;
            Title = title;
        }
    }
    public class AboutSearchModel
    {
        public string Query { get; set; }
        public int State { get; set; }
        public int Status { get; set; }
        public int Page { get; set; }
    }
    public class AboutOtherPatial
    {
        public string ID { get; set; }
    }
}