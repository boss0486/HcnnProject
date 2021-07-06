using Helper;
using Helper.Page;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Model.Entities;
using WebCore.Services;
using PagedList.Mvc;


namespace WebApplication.HomePage.Controllers
{
    public class ProductController : Controller
    {
        public ActionResult DataList(string alias, int page = 1)
        {
            string menuId = string.Empty;
            ViewData["CategoryText"] = "Sản phẩm";
            MenuBar menuBar = MenuService.GetMenuBarByAlias(alias);
            if (menuBar != null)
            {
                menuId = menuBar.ID;
                ViewData["CategoryText"] = menuBar.Title;
            }
            //    
            IEnumerable<ProductHome> models = ProductService.GetProductByMenu(menuId, page);
            return View(models);
        } 
        
 
        public ActionResult CateList(string alias, int page = 1)
        {
            string groupId = string.Empty;
            ViewData["CategoryText"] = "Sản phẩm";
            ProductGroup productGroup = ProductGroupService.GetProductGroupByAlias(alias);
            if (productGroup != null)
            {
                groupId = productGroup.ID;
                ViewData["CategoryText"] = productGroup.Title;
            }
            //    
            IEnumerable<ProductHome> models = ProductService.GetProductByCategory(groupId, page);
            return View(models);
        }
        

        public ActionResult Details(string alias)
        {
            ProductResult model = ProductService.GetProductByAlias(alias);
            return View(model);
        }

    }
}