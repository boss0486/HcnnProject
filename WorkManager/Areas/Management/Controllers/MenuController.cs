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
    [RoutePrefix("Menu")]
    public class MenuController : CMSController
    {
        // GET: BackEnd/Menu
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
            MenuService service = new MenuService();
            Menu model = service.GetMenuByID(id);
            return View(model);
        }
        public ActionResult Details(string id)
        {
            MenuService service = new MenuService();
            ViewMenu model = service.ViewMenuByID(id);
            return View(model);
        }

        [HttpPost]
        [Route("Action/MenuOption")]
        public ActionResult MenuOption()
        {
            try
            {
                using (var service = new MenuService())
                {
                    return service.GetMenuOption();
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
                using (var service = new MenuService())
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
        public ActionResult Create(MenuCreateModel model)
        {
            try
            { 
                using (var service = new MenuService())
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
        public ActionResult Update(MenuUpdateModel model)
        {
            try
            {     
                using (var service = new MenuService())
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
        public ActionResult Delete(MenuIDModel model)
        {
            try
            {     
                using (var service = new MenuService())
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
        public ActionResult SortUp(MenuItemIDModel model)
        {
            try
            {
                using (var service = new MenuService())
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
        public ActionResult SortDown(MenuItemIDModel model)
        {
            try
            {
                using (var service = new MenuService())
                    return service.SortDown(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
    }
}