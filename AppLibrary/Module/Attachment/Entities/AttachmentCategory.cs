using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using System;
using System.Collections.Generic;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("AttachmentCategory")]
    public partial class AttachmentCategory : WEBModel
    {
        public AttachmentCategory()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
        public string Path { get; set; }
        public int OrderID { get; set; }
    }
    public partial class AttachmentCategoryBar
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public int OrderID { get; set; }
    }

    public class AttachmentCategoryCreateModel
    {
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        public int OrderID { get; set; }
        public int Enabled { get; set; }
    }

    public class AttachmentCategoryUpdateModel : AttachmentCategoryCreateModel
    {
        public string ID { get; set; }
    }

    public class AttachmentCategoryIDModel
    {
        public string ID { get; set; }
    }
    public class AttachmentCategoryDeleteAndAtactmentModel
    {
        public string ID { get; set; }
        public string ImageFile { get; set; }
    }
    public class AttachmentCategoryEditModel
    {
        public string ID { get; set; }
        public string Val { get; set; }
        public string Field { get; set; }
    }


    public partial class AttachmentCategoryPageTypeOptionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }

        public AttachmentCategoryPageTypeOptionModel(string Id, string title, string alias)
        {
            ID = Id;
            Title = title;
            Alias = alias;
        }
    }

    // menu list
    public class AttachmentCategoryResult : WEBModelResult
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        public int TotalItem { get; set; }
        public int OrderID { get; set; }
        public bool IsSub { get; set; } = true;
        public List<AttachmentCategoryResult> SubOptionData { get; set; }

    }
    //

    public class AttachmentCategoryOption
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public int OrderID { get; set; }
        public string Title { get; set; }
        [NotMapped]
        public bool IsActived { get; set; }
        [NotMapped]
        public List<AttachmentCategoryOption> SubOption { get; set; }
    }

    //
    public class AttachmentCategoryOptionModel
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public bool IsSub { get; set; } = true;
        public int OrderID { get; set; }
        public List<AttachmentCategoryOptionModel> SubOptionData { get; set; }
    }


    public class SubAttachmentCategoryOptionModel 
    {
        public int level { get; set; }
        
        public List<AttachmentCategoryOptionModel> SubData { get; set; }
    }
    //
    public partial class AttachmentCategorySortModel
    {
        public string ID { get; set; }
        public int OrderID { get; set; }
    }//
   
    public class SubAttachmentCategoryBarForCategory
    {
        public string InnerText { get; set; }
        public bool IsSubNull { get; set; }

    }
}