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
    [RoutePrefix("DepartmentPart")]
    public class DepartmentPartController : CMSController
    {
        // GET: BackEnd/DepartmentPart
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
            DepartmentPartService service = new DepartmentPartService();
            DepartmentPart model = service.GetDepartmentPartByID(id);
            return View(model);
        }
         
        public ActionResult Details(string id)
        {
            DepartmentPartService service = new DepartmentPartService();
            DepartmentPartResult model = service.ViewDepartmentPartByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new DepartmentPartService())
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
        public ActionResult Create(DepartmentPartCreateModel model)
        {
            try
            {
                using (var service = new DepartmentPartService())
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
        public ActionResult Update(DepartmentPartUpdateModel model)
        {
            try
            {
                using (var service = new DepartmentPartService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(DepartmentPartIDModel model)
        {
            try
            {
                using (var service = new DepartmentPartService())
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
        
        [HttpPost]
        [Route("Action/OptionByCate")]
        public ActionResult OptionByCate(DepartmentPartIDModel model)
        {
            try
            {
                using (var service = new DepartmentPartService())
                    return Notifization.Data("", service.GetOptionByCate(model.ID));
            }
            catch (Exception)
            {
                return Notifization.NotService;
            }
        }
    }
}