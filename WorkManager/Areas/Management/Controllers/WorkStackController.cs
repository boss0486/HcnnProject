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
    [RoutePrefix("WorkPlan")]
    public class WorkPlanController : CMSController
    {
        // GET: BackEnd/WorkPlan
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
            WorkPlanService service = new WorkPlanService();
            WorkPlan model = service.GetWorkPlanByID(id);
            return View(model);
        }
        public ActionResult Details(string id)
        {
            WorkPlanService service = new WorkPlanService();
            ViewWorkPlanResult model = service.ViewWorkPlanByID(id);
            return View(model);
        }

        [HttpPost]
        [Route("Action/Option")]
        public ActionResult WorkPlanOption()
        {
            try
            {
                using (var service = new WorkPlanService())
                {
                    return service.GetWorkPlanOption();
                }
            }
            catch (Exception ex)
            {
                return Notifization.TEST(ex + "");

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
                using (var service = new WorkPlanService())
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
        public ActionResult Create(WorkPlanCreateModel model)
        {
            try
            {
                using (var service = new WorkPlanService())
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
        public ActionResult Update(WorkPlanUpdateModel model)
        {
            try
            {
                using (var service = new WorkPlanService())
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
        public ActionResult Delete(WorkPlanIDModel model)
        {
            try
            {
                using (var service = new WorkPlanService())
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
        public ActionResult SortUp(WorkPlanIDModel model)
        {
            try
            {
                return Notifization.NotService;
                //using (var service = new WorkPlanService())
                //    return service.SortUp(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/SortDown")]
        public ActionResult SortDown(WorkPlanIDModel model)
        {
            try
            {
                return Notifization.NotService;
                //using (var service = new WorkPlanService())
                //    return service.SortDown(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
    }
}