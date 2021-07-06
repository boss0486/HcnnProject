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
    [Table("App_Article")]
    public partial class Article : WEBModel
    {
        public Article()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string MenuID { get; set; }
        public string GroupID { get; set; }
        public string CategoryID { get; set; }
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
    public class ArticleCreateModel
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
    public class ArticleUpdateModel : ArticleCreateModel
    {
        public string ID { get; set; }
    }
    public class ArticleIDModel
    {
        public string ID { get; set; }
    }
    public class ArticleResult : WEBModelResult
    {

        public string ID { get; set; }
        public string MenuID { get; set; }
        public string GroupID { get; set; }
        public string CategoryID { get; set; }
        [NotMapped]
        public string CategoryText => ArticleGroupService.GetArticleGroupName(GroupID);
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

    public class ArticleHome  
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


    }
    public class ArticleOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }

    public class ArticleStatusModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public ArticleStatusModel(int Id, string title)
        {
            ID = Id;
            Title = title;
        }
    }
    public class ArticleSearchModel
    {
        public string Query { get; set; }
        public string GroupID { get; set; }
        public int State { get; set; }
        public int Status { get; set; }
        public int Page { get; set; }
    }

    public class ArticleOtherPatial
    {
        public string GroupID { get; set; }
        public string ID { get; set; }
    }
}