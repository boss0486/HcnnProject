﻿using Helper;
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
    public class FaqController : Controller
    {
        public ActionResult DataList(string alias, int page = 1)
        {
            string menuId = string.Empty;
            ViewData["CategoryText"] = "Hỏi đáp";
            MenuBar menuBar = MenuService.GetMenuBarByAlias(alias);
            if (menuBar != null)
            {
                menuId = menuBar.ID;
                ViewData["CategoryText"] = menuBar.Title;
            }
            //    
            IEnumerable<FaqResult> models = FaqService.GetFaqByMenu(menuId, page);
            return View(models);
        }  
    }
}