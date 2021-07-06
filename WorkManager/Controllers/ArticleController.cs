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
    public class ArticleController : Controller
    {
        public ActionResult DataList(string alias, int page = 1)
        {
            string menuId = string.Empty;
            ViewData["CategoryText"] = "Tin tức";
            MenuBar menuBar = MenuService.GetMenuBarByAlias(alias);
            if (menuBar != null)
            {
                menuId = menuBar.ID;
                ViewData["CategoryText"] = menuBar.Title;
            }
            //    
            IEnumerable<ArticleHome> models = ArticleService.GetArticleByMenu(menuId, page);
            return View(models);
        }
        public ActionResult CateList(string alias, int page = 1)
        {
            string groupId = string.Empty;
            ViewData["CategoryText"] = "Tin tức";
            ArticleGroup  articleGroup = ArticleGroupService.GetArticleGroupByAlias(alias);
            if (articleGroup != null)
            {
                groupId = articleGroup.ID;
                ViewData["CategoryText"] = articleGroup.Title;
            }
            //    
            IEnumerable<ArticleHome> models = ArticleService.GetArticleByCategory(groupId, page);
            return View(models);
        }

        public ActionResult Details(string alias)
        {
            ArticleResult model = ArticleService.GetArticleByAlias(alias);
            return View(model);
        }
    }
}