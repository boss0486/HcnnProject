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
    public class ProjectController : Controller
    {
        public ActionResult DataList(string alias, int page = 1)
        {
            string menuId = string.Empty;
            ViewData["CategoryText"] = "Dự án"; 
            IEnumerable<ProjectHome> models = ProjectService.GetProjectHomeList(alias, page);
            return View(models);
        } 
         
        public ActionResult Details(string alias)
        {
            ProjectResult model = ProjectService.GetProjectByAlias(alias);
            return View(model);
        }

    }
}