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
    [Table("App_Menu")]
    public partial class Menu : WEBModel
    {
        public Menu()
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
        public string PathAction { get; set; }
        public string IconFont { get; set; }
        public string ImageFile { get; set; }
        public int OrderID { get; set; }
        public int LocationID { get; set; }
        public string PageTemlate { get; set; }
        public string BackLink { get; set; }
    }
    public partial class MenuBar
    { 
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string IconFont { get; set; }
        public string ImageFile { get; set; }
        public int OrderID { get; set; }
        public string PageTemlate { get; set; }
        public string BackLink { get; set; }
        [NotMapped]
        public string UrlRoot => MenuService.GetPath(PageTemlate);
    }

    public class ViewMenu : WEBModelResult
    {
        public string ID { get; set; }
        public string IntID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Alias { get; set; }
        public string Path { get; set; }
        public string PathAction { get; set; }
        public string IconFont { get; set; }
        public string ImageFile { get; set; }
        public string ImagePath { get; set; }
        public int OrderID { get; set; }
        public string PageTemlate { get; set; }
        public string BackLink { get; set; }
    }
    public class MenuCreateModel
    {
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        public string PathAction { get; set; }
        public string IconFont { get; set; }
        public string ImageFile { get; set; }
        public int OrderID { get; set; }
        public int LocationID { get; set; }
        public string PageTemlate { get; set; }
        public string BackLink { get; set; }
        public string MvcController { get; set; }
        public string MvcAction { get; set; }
        public int Enabled { get; set; }
    }

    public class MenuUpdateModel : MenuCreateModel
    {
        public string ID { get; set; }
    }

    public class MenuIDModel
    {
        public string ID { get; set; }
    }
    public class MenuDeleteAndAtactmentModel
    {
        public string ID { get; set; }
        public string ImageFile { get; set; }
    }
    public class MenuEditModel
    {
        public string ID { get; set; }
        public string Val { get; set; }
        public string Field { get; set; }
    }


    public partial class MenuPageTypeOptionModel
    {
        public string ID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }

        public MenuPageTypeOptionModel(string Id, string title,string alias)
        {
            ID = Id;
            Title = title;
            Alias = alias;
        }
    }

    // menu list
    public class MenuResult : WEBModelResult
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Summary { get; set; }
        public string IconFont { get; set; }
        public string IconHover { get; set; }
        public int TotalItem { get; set; }
        public string PathAction { get; set; }
        public int OrderID { get; set; }
        public bool IsSub { get; set; } = true;
        public string PageTemlate { get; set; } = string.Empty;
        [NotMapped]
        public List<MenuResult> SubMenu { get; set; }

    }
    //
    public class MenuOptionModel
    {
        public string ID { get; set; }
        public string ParentID { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string IconFont { get; set; }
        public string IconHover { get; set; }
        public int TotalItem { get; set; }
        public string PathAction { get; set; }
    }
    //
    public partial class MenuSortModel
    {
        public string ID { get; set; }
        public int OrderID { get; set; }
    }//

    public class SubMenuBar
    {
        public string InnerText { get; set; }
        public bool IsToggled { get; set; }
        public bool IsHasItem { get; set; }
    }

    public class SubMenuBarForCategory
    {
        public string InnerText { get; set; }
        public bool IsSubNull { get; set; }

    }
}