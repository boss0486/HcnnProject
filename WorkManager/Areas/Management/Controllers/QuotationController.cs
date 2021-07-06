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
    [RoutePrefix("Quotation")]
    public class QuotationController : CMSController
    {
        // GET: BackEnd/Quotation
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
            QuotationService service = new QuotationService();
            Quotation model = service.GetQuotationByID(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            QuotationService service = new QuotationService();
            QuotationResult model = service.ViewQuotationByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new QuotationService())
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
        public ActionResult Create(QuotationCreateModel model)
        {
            try
            {
                using (var service = new QuotationService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(QuotationUpdateModel model)
        {
            try
            {
                using (var service = new QuotationService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(QuotationIDModel model)
        {
            try
            {
                using (var service = new QuotationService())
                    return service.Delete(model.ID);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
    }
}