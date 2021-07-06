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
    [RoutePrefix("About")]
    public class AboutController : CMSController
    {
        // GET: BackEnd/About
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
            AboutService service = new AboutService();
            About model = service.GetAboutByID(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            AboutService service = new AboutService();
            AboutResult model = service.ViewAboutByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new AboutService())
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
        public ActionResult Create(AboutCreateModel model)
        {
            try
            {
                using (var service = new AboutService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(AboutUpdateModel model)
        {
            try
            {
                using (var service = new AboutService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(AboutIDModel model)
        {
            try
            {
                using (var service = new AboutService())
                    return service.Delete(model.ID);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
    }
}