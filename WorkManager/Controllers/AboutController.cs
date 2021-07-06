using Helper;
using Helper.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebApplication.HomePage.Controllers
{
    public class AboutController : Controller
    {
        // GET: Introduction 
        public ActionResult DataList(string alias)
        {
            ViewData["CategoryText"] = "Giới thiệu";
            MenuBar menuBar = MenuService.GetMenuBarByAlias(alias);
            if (menuBar != null) 
                ViewData["CategoryText"] = menuBar.Title;
            //    
            AboutResult models = AboutService.GetAboutDefault();
            return View(models);
        }


        public ActionResult Details(string alias)
        {
            ViewData["CategoryText"] = "Giới thiệu";
            AboutResult model = AboutService.GetAboutByAlias(alias);
            return View(model);
        }
    }
}