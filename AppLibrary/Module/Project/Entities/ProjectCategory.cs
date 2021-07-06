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
    [Table("App_ProjectCategory")]
    public partial class ProjectCategory : WEBModel
    {
        public ProjectCategory()
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
        public string IconFont { get; set; }
        public string ImageFile { get; set; }
        public int OrderID { get; set; }
        public string BackLink { get; set; }
    }
    public partial class ProjectCategoryBar
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string IconFont { get; set; }
        public string ImageFile { get; set; }
        public int OrderID { get; set; }
        public string BackLink { get; set; }
    }

    public class ViewProjectCategory : WEBModelResult
    {
        public string ID { get; set; }
        public string IntID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
        public string Path { get; set; }
        public string IconFont { get; set; }
        public string ImageFile { get; set; }
        public string ImagePath { get; set; }
        public int OrderID { get; set; }
        public string BackLink { get; set; }
    }
    public class ProjectCategoryCreateModel
    {
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        public string IconFont { get; set; }
        public string ImageFile { get; set; }
        public int OrderID { get; set; }
        public string BackLink { get; set; }
        public int Enabled { get; set; }
    }

    public class ProjectCategoryUpdateModel : ProjectCategoryCreateModel
    {
        public string ID { get; set; }
    }

    public class ProjectCategoryIDModel
    {
        public string ID { get; set; }
    }
    public class ProjectCategoryDeleteAndAtactmentModel
    {
        public string ID { get; set; }
        public string ImageFile { get; set; }
    }
    public class ProjectCategoryEditModel
    {
        public string ID { get; set; }
        public string Val { get; set; }
        public string Field { get; set; }
    }


    public partial class ProjectCategoryPageTypeOptionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }

        public ProjectCategoryPageTypeOptionModel(string Id, string title, string alias)
        {
            ID = Id;
            Title = title;
            Alias = alias;
        }
    }

    // ProjectCategory list
    public class ProjectCategoryResult : WEBModelResult
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string IconFont { get; set; }
        public string IconHover { get; set; }
        public int TotalItem { get; set; }
        public int OrderID { get; set; }
        public bool IsSub { get; set; } = true;
        public List<ProjectCategoryResult> SubData { get; set; }

    }
    //
    public class ProjectCategoryOptionModel
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string IconFont { get; set; }
        public string IconHover { get; set; }
        public int TotalItem { get; set; }
    }
    //
    public partial class ProjectCategorySortModel
    {
        public string ID { get; set; }
        public int OrderID { get; set; }
    }//

    public class SubCategoryBar
    {
        public string InnerText { get; set; }
        public bool IsToggled { get; set; }
        public bool IsHasItem { get; set; }
    }

    public class SubBarForCategory
    {
        public string InnerText { get; set; }
        public bool IsSubNull { get; set; }

    }
}