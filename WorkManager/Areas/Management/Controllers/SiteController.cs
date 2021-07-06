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
    [RoutePrefix("Site")]
    public class SiteController : CMSController
    {
        // GET: BackEnd/SiteInfomation
        public ActionResult DataList()
        {
            return View();
        }

        public ActionResult Update(string id)
        {
            SiteService service = new SiteService();
            Site model = service.GetSiteByID(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            SiteService service = new SiteService();
            SiteResult model = service.ViewSiteByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new SiteService())
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
        [Route("Action/Update")]
        public ActionResult Update(SiteUpdateModel model)
        {
            try
            {
                using (var service = new SiteService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(SiteIDModel model)
        {
            try
            {
                using (var service = new SiteService())
                    return service.Delete(model);
                //
            }
            catch (Exception)
            {
                return Notifization.NotService;
            }
        }
    }
}