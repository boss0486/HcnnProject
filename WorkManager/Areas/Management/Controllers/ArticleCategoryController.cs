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
    [RoutePrefix("ArticleCategory")]
    public class ArticleCategoryController : CMSController
    {
        // GET: BackEnd/ArticleCategory
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
            ArticleCategoryService service = new ArticleCategoryService();
            ArticleCategory model = service.GetArticleCategoryByID(id);
            return View(model);
        }
        public ActionResult Details(string id)
        {
            ArticleCategoryService service = new ArticleCategoryService();
            ViewArticleCategory model = service.ViewArticleCategoryByID(id);
            return View(model);
        }

        [HttpPost]
        [Route("Action/Option")]
        public ActionResult ArticleCategoryOption()
        {
            try
            {
                using (var service = new ArticleCategoryService())
                {
                    return service.GetArticleCategoryOption();
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
                using (var service = new ArticleCategoryService())
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
        public ActionResult Create(ArticleCategoryCreateModel model)
        {
            try
            { 
                using (var service = new ArticleCategoryService())
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
        public ActionResult Update(ArticleCategoryUpdateModel model)
        {
            try
            {     
                using (var service = new ArticleCategoryService())
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
        public ActionResult Delete(ArticleCategoryIDModel model)
        {
            try
            {     
                using (var service = new ArticleCategoryService())
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
        public ActionResult SortUp(ArticleCategoryIDModel model)
        {
            try
            {
                using (var service = new ArticleCategoryService())
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
        public ActionResult SortDown(ArticleCategoryIDModel model)
        {
            try
            {
                using (var service = new ArticleCategoryService())
                    return service.SortDown(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("::" + ex);
            }
        }
    }
}