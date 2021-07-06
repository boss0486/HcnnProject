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
    [RoutePrefix("AttachmentCategory")]
    public class AttachmentCategoryController : CMSController
    {
        // GET: BackEnd/AttachmentCategory
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
            AttachmentCategoryService service = new AttachmentCategoryService();
            AttachmentCategory model = service.GetAttachmentCategoryByID(id);
            return View(model);
        }
        public ActionResult Details(string id)
        {
            AttachmentCategoryService service = new AttachmentCategoryService();
            AttachmentCategoryResult model = service.ViewAttachmentCategoryByID(id);
            return View(model);
        }

        [HttpPost]
        [Route("Action/Option")]
        public ActionResult AttachmentCategoryOption(AttachmentCategoryIDModel model)
        {
            try
            {
                using (var service = new AttachmentCategoryService())
                    return service.GetAttachmentCategoryJs(model.ID);
                //
            }
            catch (Exception ex)
            {
                return Notifization.TEST(ex + "");

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
                using (var service = new AttachmentCategoryService())
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
        public ActionResult Create(AttachmentCategoryCreateModel model)
        {
            try
            {
                using (var service = new AttachmentCategoryService())
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
        public ActionResult Update(AttachmentCategoryUpdateModel model)
        {
            try
            {
                using (var service = new AttachmentCategoryService())
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
        public ActionResult Delete(AttachmentCategoryIDModel model)
        {
            try
            {
                using (var service = new AttachmentCategoryService())
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
        public ActionResult SortUp(AttachmentCategoryIDModel model)
        {
            try
            {
                using (var service = new AttachmentCategoryService())
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
        public ActionResult SortDown(AttachmentCategoryIDModel model)
        {
            try
            {
                using (var service = new AttachmentCategoryService())
                    return service.SortDown(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
    }
}