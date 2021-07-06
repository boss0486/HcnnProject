using AL.NetFrame.Attributes;
using AL.NetFrame.Interfaces;
using AL.NetFrame.Services;
using Dapper;
using Helper.File;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebCore.Entities
{
    [ConnectionString(DbConnect.ConnectionString.CMS)]
    [Table("App_Product")]
    public partial class Product : WEBModel
    {
        public Product()
        {
            ID = Guid.NewGuid().ToString().ToLower();
        }
        [Key]
        [IgnoreUpdate]
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TextID { get; set; }
        public string Summary { get; set; }
        public string HtmlNote { get; set; }
        public string HtmlText { get; set; }
        public string Tag { get; set; }
        public string ImageFile { get; set; }
        public double Price { get; set; }
        public double PriceListed { get; set; }
        public string PriceText { get; set; }
        public string MenuID { get; set; }
        public string GroupID { get; set; }
        public string CategoryID { get; set; }
        public string MadeInID { get; set; }
        public string WarrantyID { get; set; }
        public int State { get; set; }
        public int ViewTotal { get; set; }
        public DateTime? ViewDate { get; set; }
    }
    // model
    public class ProductCreateModel
    {
        public string Title { get; set; }
        public string Alias { get; set; }
        public string TextID { get; set; }
        public string Summary { get; set; }
        [AllowHtml]
        public string HtmlNote { get; set; }
        [AllowHtml]
        public string HtmlText { get; set; }
        public string Tag { get; set; }
        public string ImageFile { get; set; }
        public double Price { get; set; }
        public double PriceListed { get; set; }
        public string PriceText { get; set; }
        public string MenuID { get; set; }
        public string CategoryID { get; set; }
        public string GroupID { get; set; }
        public string MadeInID { get; set; }
        public string WarrantyID { get; set; }
        public int State { get; set; }
        public int ViewTotal { get; set; }
        public string ViewDate { get; set; }
        public int Enabled { get; set; }
        [NotMapped]
        public List<string> Photos { get; set; }
    }
    public class ProductUpdateModel : ProductCreateModel
    {
        public string ID { get; set; }
    }
    public class ProductIDModel
    {
        public string ID { get; set; }
    }

    public class ProductResult : WEBModelResult
    {
        public string ID { get; set; }
        public string TextID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Alias { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string HtmlNote { get; set; } = string.Empty;
        public string HtmlText { get; set; } = string.Empty;
        public string Tag { get; set; }
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);

        public double Price { get; set; }
        public double PriceListed { get; set; }
        public string PriceText { get; set; }
        public string MenuID { get; set; }
        public string CategoryID { get; set; }
        public string GroupID { get; set; }
        public string MadeInID { get; set; }
        public string WarrantyID { get; set; }
        [NotMapped]
        public string CategoryText => ProductGroupService.GetNameByID(GroupID);
        [NotMapped]
        public string MadeInText => ProductProviderService.GetNameByID(MadeInID);
        [NotMapped]
        public string WarrantyText => ProductWarrantyService.GetNameByID(WarrantyID);
        public int State { get; set; }
        [NotMapped]
        public string StateTx => ProductService.ViewState(State);
        public int ViewTotal { get; set; }
        public string ViewDate { get; set; }
    }

    public class ProductHome
    {
        public string ID { get; set; }
        public string TextID { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Alias { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string ImageFile { get; set; }
        [NotMapped]
        public string ImagePath => AttachmentFile.GetFile(ImageFile, true);

        public double Price { get; set; }
        public double PriceListed { get; set; }
        public string PriceText { get; set; }
        public string CategoryID { get; set; }
        public string GroupID { get; set; }
        public string MadeInID { get; set; }
        public string WarrantyID { get; set; }
        [NotMapped]
        public string CategoryText => ProductGroupService.GetNameByID(GroupID);
        [NotMapped]
        public string MadeInText => ProductProviderService.GetNameByID(MadeInID);
        [NotMapped]
        public string WarrantyText => ProductWarrantyService.GetNameByID(WarrantyID);
        public int State { get; set; }
        [NotMapped]
        public string StateTx => ProductService.ViewState(State);
        public int ViewTotal { get; set; }
        public string ViewDate { get; set; }
    }
    public class ProductOption
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public double Price { get; set; }
        public double PriceListed { get; set; }
        public string PriceText { get; set; }
        public string MadeInID { get; set; }
        public string WarrantyID { get; set; }
        public int State { get; set; }
    }

    public class ProductStateModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public ProductStateModel(int Id, string title)
        {
            ID = Id;
            Title = title;
        }
    }

    public class ProductSearchModel : SearchModel
    {
        public string GroupID { get; set; }
        public int State { get; set; }
    }

    public class ProductOtherPatial
    {
        public string GroupID { get; set; }
        public string ID { get; set; }
    }

    public class ProductHomeSearch
    {
        public string Query { get; set; } = "";
        public int PriceMin { get; set; }
        public int PriceMax { get; set; }
        public int page { get; set; }
    }

}