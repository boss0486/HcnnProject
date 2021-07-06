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
    [Table("App_ArticleCategory")]
    public partial class ArticleCategory : WEBModel
    {
        public ArticleCategory()
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
    public partial class ArticleCategoryBar
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

    public class ViewArticleCategory : WEBModelResult
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
    public class ArticleCategoryCreateModel
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

    public class ArticleCategoryUpdateModel : ArticleCategoryCreateModel
    {
        public string ID { get; set; }
    }

    public class ArticleCategoryIDModel
    {
        public string ID { get; set; }
    }
    public class ArticleCategoryDeleteAndAtactmentModel
    {
        public string ID { get; set; }
        public string ImageFile { get; set; }
    }
    public class ArticleCategoryEditModel
    {
        public string ID { get; set; }
        public string Val { get; set; }
        public string Field { get; set; }
    }


    public partial class ArticleCategoryPageTypeOptionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }

        public ArticleCategoryPageTypeOptionModel(string Id, string title, string alias)
        {
            ID = Id;
            Title = title;
            Alias = alias;
        }
    }

    // ArticleCategory list
    public class ArticleCategoryResult : WEBModelResult
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        public string IconFont { get; set; }
        public string IconHover { get; set; }
        public int TotalItem { get; set; }
        public int OrderID { get; set; }
        public bool IsSub { get; set; } = true;
        public List<ArticleCategoryResult> SubArticleCategory { get; set; }

    }
    //
    public class ArticleCategoryOptionModel
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
    public partial class ArticleCategorySortModel
    {
        public string ID { get; set; }
        public int OrderID { get; set; }
    }//

    public class SubArticleCategoryBar
    {
        public string InnerText { get; set; }
        public bool IsToggled { get; set; }
        public bool IsHasItem { get; set; }
    }

    public class SubArticleCategoryBarForCategory
    {
        public string InnerText { get; set; }
        public bool IsSubNull { get; set; }

    }
}