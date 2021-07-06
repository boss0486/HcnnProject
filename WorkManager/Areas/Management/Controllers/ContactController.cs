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
    [RoutePrefix("Contact")]
    public class ContactController : CMSController
    {
        // GET: BackEnd/Contact
        public ActionResult DataList()
        {
            return View();
        }
          
        public ActionResult Send(string id)
        {
            ContactService service = new ContactService();
            Contact model = service.GetContactByID(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            ContactService service = new ContactService();
            ContactResult model = service.ViewContactByID(id);
            if (model != null)
                return View(model);
            //
            return View();
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new ContactService())
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
        [Route("Action/Delete")]
        public ActionResult Delete(ContactIDModel model)
        {
            try
            {
                using (var service = new ContactService())
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