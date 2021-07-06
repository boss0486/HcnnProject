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
    [Table("App_Quotation")]
    public partial class Quotation : WEBModel
    {
        public Quotation()
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
        public string Summary { get; set; }
        public string ImageFile { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
    }
    // model
    public class QuotationCreateModel
    {
        public string MenuID { get; set; }
        [AllowHtml]
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
        public string ImageFile { get; set; }
        public int Enabled { get; set; }
        public List<string> Files { get; set; }
    }
    public class QuotationUpdateModel : QuotationCreateModel
    {
        public string ID { get; set; }
    }
    public class QuotationIDModel
    {
        public string ID { get; set; }
    }
    public class QuotationResult : WEBModelResult
    {

        public string ID { get; set; }
        public string MenuID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);

        public string Summary { get; set; }
        public string HtmlText { get; set; }
    }
    public class QuotationHome
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);

        public string Summary { get; set; }

    }
    public class QuotationOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
    }

    public class QuotationStatusModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public QuotationStatusModel(int Id, string title)
        {
            ID = Id;
            Title = title;
        }
    }
    public class QuotationSearchModel
    {
        public string Query { get; set; }
        public int State { get; set; }
        public int Status { get; set; }
        public int Page { get; set; }
    }
    public class QuotationOtherPatial
    {
        public string ID { get; set; }
    }
}