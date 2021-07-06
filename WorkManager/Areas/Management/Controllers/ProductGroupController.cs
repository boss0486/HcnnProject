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
    [RoutePrefix("ProductGroup")]
    public class ProductGroupController : CMSController
    {
        // GET: BackEnd/ProductGroup
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
            ProductGroupService service = new ProductGroupService();
            ProductGroup model = service.GetProductGroupByID(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            ProductGroupService service = new ProductGroupService();
            ProductGroupResult model = service.ViewProductGroupByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new ProductGroupService())
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
        public ActionResult Create(ProductGroupCreateModel model)
        {
            try
            {
                using (var service = new ProductGroupService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(ProductGroupUpdateModel model)
        {
            try
            {
                using (var service = new ProductGroupService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(ProductGroupIDModel model)
        {
            try
            {
                using (var service = new ProductGroupService())
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