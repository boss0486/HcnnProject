using Helper;
using Helper.Page;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Model.Entities;
using WebCore.Services;
using PagedList.Mvc;


namespace WebApplication.HomePage.Controllers
{

    [RouteArea("HomePage")]
    [RoutePrefix("Contact")]
    public class ContactController : Controller
    {
        public ActionResult Index()
        { 
            return View();
        }

        [HttpPost]
        [Route("Action/Send")]
        public ActionResult Send(ContactCreateModel model)
        {
            try
            {
                using (var service = new ContactService())
                    return service.Send(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

    }
}