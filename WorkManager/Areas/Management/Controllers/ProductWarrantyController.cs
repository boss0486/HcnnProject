﻿using Helper;
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
    [RoutePrefix("ProductWarranty")]
    public class ProductWarrantyController : CMSController
    {
        // GET: BackEnd/ProductWarranty
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
            ProductWarrantyService service = new ProductWarrantyService();
            var model = service.GetProductWarrantyByID(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            ProductWarrantyService service = new ProductWarrantyService();
            var model = service.ViewProductWarrantyByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new ProductWarrantyService())
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
        public ActionResult Create(ProductWarrantyCreateModel model)
        {
            try
            {
                using (var service = new ProductWarrantyService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(ProductWarrantyUpdateModel model)
        {
            try
            {
                using (var service = new ProductWarrantyService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(ProductWarrantyIDModel model)
        {
            try
            {
                using (var service = new ProductWarrantyService())
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