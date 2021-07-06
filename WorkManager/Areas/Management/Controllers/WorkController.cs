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
    [RoutePrefix("Work")]
    public class WorkController : CMSController
    {
        // GET: BackEnd/Work
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
            WorkService service = new WorkService();
            Work model = service.GetWorkByID(id);
            return View(model);
        }
        public ActionResult Details(string id)
        {
            WorkService service = new WorkService();
            ViewWorkResult model = service.ViewWorkByID(id);
            return View(model);
        }
        public ActionResult Assign(string id)
        {
            WorkService service = new WorkService();
            WorkAssign model = service.GetWorkForAssign(id);
            return View(model);
        } 
        [HttpPost]
        [Route("Action/Option")]
        public ActionResult WorkOption()
        {
            try
            {
                using (var service = new WorkService())
                {
                    return service.GetWorkOption();
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
                using (var service = new WorkService())
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
        public ActionResult Create(WorkCreateModel model)
        {
            try
            {
                using (var service = new WorkService())
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
        public ActionResult Update(WorkUpdateModel model)
        {
            try
            {
                using (var service = new WorkService())
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
        public ActionResult Delete(WorkIDModel model)
        {
            try
            {
                using (var service = new WorkService())
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
        public ActionResult SortUp(WorkIDModel model)
        {
            try
            {
                return Notifization.NotService;
                //using (var service = new WorkService())
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
        public ActionResult SortDown(WorkIDModel model)
        {
            try
            {
                return Notifization.NotService;
                //using (var service = new WorkService())
                //    return service.SortDown(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/AssignList")]
        public ActionResult AssignList(WorkAssignSearchModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return service.AssignList(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/WorkAdd")]
        public ActionResult AssignSub(WorkAssignCreateModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return service.AssignAdd(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        // 
        [HttpPost]
        [Route("Action/WorkUpdate")]
        public ActionResult WorkUpdate(WorkAssignUpdateModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return service.AssignUpdate(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/GetWorkByID")]
        public ActionResult GetWorkByID(WorkIDModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return Notifization.Data("", service.ViewWorkByID(model.ID));
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        } 
        //
        [HttpPost]
        [Route("Action/Assign")]
        public ActionResult Assign(WorkAssignFastModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return service.AssignFast(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/AssignOption")]
        public ActionResult AssignOption()
        {
            try
            {
                using (var service = new WorkService())
                    return Notifization.Data("OK", service.GetWorkAssignOption());
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
        [HttpPost]
        [Route("Action/AssignInOption")]
        public ActionResult AssignInOption()
        {
            try
            {
                using (var service = new WorkService())
                    return Notifization.Data("OK", service.GetWorkAssignInternal());
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/StateOption")]
        public ActionResult StateOption(WorkIDModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return Notifization.Data("OK", service.WorkStateOption());
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
        // 

        [HttpPost]
        [Route("Action/ProcessAdd")]
        public ActionResult ProcessAdd(WorkProcessCreateModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return service.ProcessAdd(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/ProcessUpdate")]
        public ActionResult ProcessUpdate(WorkProcessUpdateModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return service.ProcessUpdate(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        
        [HttpPost]
        [Route("Action/ProcessAssign")]
        public ActionResult ProcessAssign(WorkProcessAssignModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return service.ProcessFastAssign(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        
        [HttpPost]
        [Route("Action/WorkUserExecutes")]
        public ActionResult WorkUserExecutes(WorkUserInDepartmentModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return Notifization.Data("OK", service.WorkUserExecutes(model));
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        [HttpPost]
        [Route("Action/WorkUserFollows")]
        public ActionResult WorkUserFollows(WorkUserInDepartmentModel model)
        {
            try
            {
                using (var service = new WorkService())
                    return Notifization.Data("OK", service.WorkUserFollows(model));
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }


    }
}