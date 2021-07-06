using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Helper;
using Helper.Page;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebApplication.Management.Controllers
{
    [IsManage]
    [RouteArea("Management")]
    [RoutePrefix("ProjectCategory")]
    public class ProjectCategoryController : CMSController
    {
        // GET: BackEnd/ProjectCategory
        public ActionResult DataList()
        {
            return View();
        }
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult Update(string id)
        {
            ProjectCategoryService service = new ProjectCategoryService();
            ProjectCategory model = service.GetProjectCategoryByID(id);
            return View(model);
        }
        public ActionResult Details(string id)
        {
            ProjectCategoryService service = new ProjectCategoryService();
            ViewProjectCategory model = service.ViewProjectCategoryByID(id);
            return View(model);
        }

        [HttpPost]
        [Route("Action/Option")]
        public ActionResult ProjectCategoryOption()
        {
            try
            {
                using (var service = new ProjectCategoryService())
                {
                    return service.GetProjectCategoryOption();
                }
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }
        //
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new ProjectCategoryService())
                    return service.DataList(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        } 
        //
        [HttpPost]
        [Route("Action/Create")]
        public ActionResult Create(ProjectCategoryCreateModel model)
        {
            try
            { 
                using (var service = new ProjectCategoryService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(ProjectCategoryUpdateModel model)
        {
            try
            {     
                using (var service = new ProjectCategoryService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(ProjectCategoryIDModel model)
        {
            try
            {     
                using (var service = new ProjectCategoryService())
                    return service.Delete(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/SortUp")]
        public ActionResult SortUp(ProjectCategoryIDModel model)
        {
            try
            {
                using (var service = new ProjectCategoryService())
                    return service.SortUp(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/SortDown")]
        public ActionResult SortDown(ProjectCategoryIDModel model)
        {
            try
            {
                using (var service = new ProjectCategoryService())
                    return service.SortDown(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
    }
}