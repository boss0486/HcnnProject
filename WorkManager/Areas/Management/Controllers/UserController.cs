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
    [RoutePrefix("User")]
    public class UserController : CMSController
    {
        // GET: BackEnd/User
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
            UserClientService service = new UserClientService();
            UserClient model = service.GetUserModel(id);
            return View(model);
        }

        public ActionResult UserRole(string id)
        {
            UserClientService service = new UserClientService();
            UserClient model = service.GetUserModel(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            UserClientService service = new UserClientService();
            UserClientResult model = service.ViewUserModel(id);
            return View(model);
        }

        // #######################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                UserClientService service = new UserClientService();
                return service.DataList(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Create")]
        public ActionResult Create(UserClientCreateModel model)
        {
            try
            {
                var service = new UserClientService();
                return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(UserClientUpdateModel model)
        {
            try
            {
                var service = new UserClientService();
                return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(UserIDModel model)
        {
            try
            {
                using (var service = new UserClientService())
                    return service.Delete(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST(">>" + ex);
            }
        }
        [HttpPost]
        [Route("Action/UserRole")]
        public ActionResult UserRoleSetting(UserRoleSettingModel model)
        {
            try
            {
                using (var service = new UserClientService())
                    return service.UserRoleSetting(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST(">>" + ex);
            }
        }

        // #######################################################################################################################################
    }
}