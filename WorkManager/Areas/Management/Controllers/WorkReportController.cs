using AppLibrary.Core.Model.ReportModel;
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
    [RoutePrefix("Report")]
    public class WorkReportController : CMSController
    {
        public ActionResult DataList()
        {
            return View();
        }

        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchTaskReportModel model)
        {
            try
            {
                TaskReportService service = new TaskReportService();
                return service.DataList(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
    }
}