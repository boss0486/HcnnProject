using Helper;
using System;
using System.Web.Mvc;
using WebCore.Core;
using WebCore.Entities;
using WebCore.Services;

namespace WebApplication.Management.Controllers
{
    [IsManage]
    [RouteArea("Management")]
    [RoutePrefix("Permission")]
    public class PermissionController : CMSController
    {
        public ActionResult Setting()
        {
            return View();
        }


        //##############################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/PermissionData")]
        public ActionResult PermissionDataByRoleID()
        {
            try
            {
                return Notifization.Data(MessageText.Success, null); 
            }
            catch (Exception ex)
            {
                return Notifization.TEST(">>" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Setting")]
        public ActionResult Setting(RoleSettingRequest model)
        {
            try
            {
                using (var service = new PermissionService())
                {
                    model.RouteArea = AreaApplicationService.GetRouteAreaID((int)WebCore.ENM.AreaApplicationEnum.AreaType.MANAGEMENT);
                    return service.SettingPermission(model);
                }
            }
            catch (Exception ex)
            {
                return Notifization.TEST(">>" + ex);
            }

        }

        //##############################################################################################################################################################################################################################################################

         
    }
}