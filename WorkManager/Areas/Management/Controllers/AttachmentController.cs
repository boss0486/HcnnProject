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
    [RoutePrefix("Attachment")]
    public class AttachmentController : CMSController
    {
        // GET: BackEnd/AttachmentFile
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
            AttachmentService service = new AttachmentService();
            Attachment model = service.GetAttachmentByID(id);
            return View(model);
        }

        //##########################################################################################################################################################################################################################################################

        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(AttachmentSearchModel model)
        {
            try
            {
                using (var attachmentService = new AttachmentService())
                    return attachmentService.DataList(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
        [HttpPost]
        [Route("Action/Create")]
        public ActionResult Create(AttachmentUploadModel model)
        {
            try
            {
                AttachmentService attachmentService = new AttachmentService();
                return attachmentService.Create(model);
            }
            catch (Exception)
            {
                return Notifization.NotService;
            }
        }
        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(AttachmentUpdateModel model)
        {
            try
            {
                AttachmentService attachmentService = new AttachmentService();
                return attachmentService.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(AttachmentIDModel model)
        {
            try
            {
                using (var service = new AttachmentService())
                    return service.Delete(model.ID);
            }
            catch (Exception)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/CategoryList")]
        public ActionResult CategoryList(AttachmentIDModel model)
        {
            try
            {

                return Notifization.NotService;
                //using (var service = new AttachmentCategoryService())
                //    return Notifization.Data("", service.DataOption(""));
            }
            catch (Exception)
            {
                return Notifization.NotService;
            }
        }
    }
}