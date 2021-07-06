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

namespace WebApplication.Management.Controllers
{
    [IsManage]
    [RouteArea("Management")]
    [RoutePrefix("Project")]
    public class ProjectController : CMSController
    {
        // GET: BackEnd/Project
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
            ProjectService service = new ProjectService();
            Project model = service.GetProjectByID(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            ProjectService service = new ProjectService();
            ProjectResult model = service.ViewProjectByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new ProjectService())
                {
                    return service.DataList(model);
                }
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Create")]
        public ActionResult Create(ProjectCreateModel model)
        {
            try
            {
                using (var service = new ProjectService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(ProjectUpdateModel model)
        {
            try
            {
                using (var service = new ProjectService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(ProjectIDModel model)
        {
            try
            {
                using (var service = new ProjectService())
                    return service.Delete(model.ID);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
    }
}