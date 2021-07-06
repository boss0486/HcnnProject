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
    [RoutePrefix("SocialNetwork")]
    public class SocialNetworkController : CMSController
    {
        // GET: BackEnd/bank
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
            SocialNetworkService service = new SocialNetworkService();
            SocialNetwork model = service.GetSocialNetworkByID(id);
            return View(model);
        }
         
        public ActionResult Details(string id)
        {
            SocialNetworkService service = new SocialNetworkService();
            SocialNetworkResult model = service.ViewSocialNetworkByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new SocialNetworkService())
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
        public ActionResult Create(SocialNetworkCreateModel model)
        {
            try
            {
                using (var service = new SocialNetworkService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(SocialNetworkUpdateModel model)
        {
            try
            {
                using (var service = new SocialNetworkService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(SocialNetworkIDModel model)
        {
            try
            {
                using (var service = new SocialNetworkService())
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