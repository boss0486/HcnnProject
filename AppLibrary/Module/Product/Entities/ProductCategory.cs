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
    [Table("App_ProductCategory")]
    public partial class ProductCategory : WEBModel
    {
        public ProductCategory()
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
    public partial class ProductCategoryBar
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

    public class ViewProductCategory : WEBModelResult
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
    public class ProductCategoryCreateModel
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

    public class ProductCategoryUpdateModel : ProductCategoryCreateModel
    {
        public string ID { get; set; }
    }

    public class ProductCategoryIDModel
    {
        public string ID { get; set; }
    }
    public class ProductCategoryDeleteAndAtactmentModel
    {
        public string ID { get; set; }
        public string ImageFile { get; set; }
    }
    public class ProductCategoryEditModel
    {
        public string ID { get; set; }
        public string Val { get; set; }
        public string Field { get; set; }
    }


    public partial class ProductCategoryPageTypeOptionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }

        public ProductCategoryPageTypeOptionModel(string Id, string title,string alias)
        {
            ID = Id;
            Title = title;
            Alias = alias;
        }
    }

    // ProductCategory list
    public class ProductCategoryResult : WEBModelResult
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
        public List<ProductCategoryResult> SubData { get; set; }

    }
    //
    public class ProductCategoryOptionModel
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
    public partial class ProductCategorySortModel
    {
        public string ID { get; set; }
        public int OrderID { get; set; }
    }//

    public class SubProductCategoryBar
    {
        public string InnerText { get; set; }
        public bool IsToggled { get; set; }
        public bool IsHasItem { get; set; }
    }

    public class SubProductCategoryBarForCategory
    {
        public string InnerText { get; set; }
        public bool IsSubNull { get; set; }

    }
}