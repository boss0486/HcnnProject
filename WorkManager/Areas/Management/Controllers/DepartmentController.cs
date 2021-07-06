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
    [RoutePrefix("Department")]
    public class DepartmentController : CMSController
    {
        // GET: BackEnd/Department
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
            DepartmentService service = new DepartmentService();
            Department model = service.GetDepartmentByID(id);
            return View(model);
        }
         
        public ActionResult Details(string id)
        {
            DepartmentService service = new DepartmentService();
            DepartmentResult model = service.ViewDepartmentByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new DepartmentService())
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
        public ActionResult Create(DepartmentCreateModel model)
        {
            try
            {
                using (var service = new DepartmentService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                throw ex;
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(DepartmentUpdateModel model)
        {
            try
            {
                using (var service = new DepartmentService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(DepartmentIDModel model)
        {
            try
            {
                using (var service = new DepartmentService())
                {
                    if (model == null)
                        return Notifization.Invalid();
                    //
                    return service.Delete(model.ID);
                }
            }
            catch (Exception)
            {
                return Notifization.NotService;
            }
        }
    }
}