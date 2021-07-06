using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Helper;
using Helper.Page;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Model.Entities;
using WebCore.Services;

namespace WebApplication.Management.Controllers
{
    [IsManage]
    [RouteArea("Management")]
    [RoutePrefix("ProductCategory")]
    public class ProductCategoryController : CMSController
    {
        // GET: BackEnd/ProductCategory
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
            ProductCategoryService service = new ProductCategoryService();
            ProductCategory model = service.GetProductCategoryByID(id);
            return View(model);
        }
        public ActionResult Details(string id)
        {
            ProductCategoryService service = new ProductCategoryService();
            ViewProductCategory model = service.ViewProductCategoryByID(id);
            return View(model);
        }

        [HttpPost]
        [Route("Action/Option")]
        public ActionResult ProductCategoryOption()
        {
            try
            {
                using (var service = new ProductCategoryService())
                {
                    return service.GetProductCategoryOption();
                }
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }
        //
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new ProductCategoryService())
                    return service.DataList(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        } 
        //
        [HttpPost]
        [Route("Action/Create")]
        public ActionResult Create(ProductCategoryCreateModel model)
        {
            try
            { 
                using (var service = new ProductCategoryService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(ProductCategoryUpdateModel model)
        {
            try
            {     
                using (var service = new ProductCategoryService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(ProductCategoryIDModel model)
        {
            try
            {     
                using (var service = new ProductCategoryService())
                    return service.Delete(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/SortUp")]
        public ActionResult SortUp(ProductCategoryIDModel model)
        {
            try
            {
                using (var service = new ProductCategoryService())
                    return service.SortUp(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
        //
        [HttpPost]
        [Route("Action/SortDown")]
        public ActionResult SortDown(ProductCategoryIDModel model)
        {
            try
            {
                using (var service = new ProductCategoryService())
                    return service.SortDown(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
    }
}