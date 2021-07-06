using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using Helper.File;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using WebCore.Model.Entities;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_Site")]
    public partial class Site : WEBModel
    {
        public Site()
        {
            ID = Guid.NewGuid().ToString().ToLower();
            LanguageID = Helper.Language.LanguagePage.GetLanguageCode;
            Enabled = 0;
            CreatedBy = Helper.Current.UserLogin.IdentifierID;
            CreatedDate = DateTime.Now;
        }

        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string CodeID { get; set; }
        public string Summary { get; set; }
        public string IconFile { get; set; }
        public string ImageFile { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string Tel { get; set; }
        public string WorkTime { get; set; }
        public string Address { get; set; }
        public string Gmaps { get; set; }
        public string Path { get; set; }
        public int Type { get; set; }
    }

    // model
    public class SiteCreateModel
    {
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string IconFile { get; set; }
        public string ImageFile { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public string Phone { get; set; }
        public string Tel { get; set; }
        public string WorkTime { get; set; }
        public string Address { get; set; }
        [AllowHtml]
        public string Gmaps { get; set; }
        public int Type { get; set; }
        public int Enabled { get; set; }

    }
    public class SiteUpdateModel : SiteCreateModel
    {
        public string ID { get; set; }
    }
    public class SiteIDModel
    {
        public string ID { get; set; }
    }
    public class SiteResult : WEBModelResult
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string CodeID { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string IconFile { get; set; }
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);
        public string Email { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Tel { get; set; } = string.Empty;
        public string WorkTime { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Gmaps { get; set; } = string.Empty;
        public int Type { get; set; }
    }
    public class SiteOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string CodeID { get; set; }
        public string Alias { get; set; }
        public string ParentID { get; set; }
    }
    public class SiteTypeOption
    {
        public int ID { get; set; }
        public string Title { get; set; }
    }

    public class SiteAssiagnOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int Cate { get; set; }

    }
    public class SiteAssiagnOption2
    {
        public List<SiteAssiagnOption> Internal { get; set; }
        public List<SiteAssiagnOption> OutSite { get; set; }
    }
}