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
    [RoutePrefix("Article")]
    public class ArticleController : CMSController
    {
        // GET: BackEnd/Article
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
            ArticleService service = new ArticleService();
            Article model = service.GetArticleByID(id);
            return View(model);
        }

        public ActionResult Details(string id)
        {
            ArticleService service = new ArticleService();
            ArticleResult model = service.ViewArticleByID(id);
            return View(model);
        }
        //##########################################################################################################################################################################################################################################################
        [HttpPost]
        [Route("Action/DataList")]
        public ActionResult DataList(SearchModel model)
        {
            try
            {
                using (var service = new ArticleService())
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
        public ActionResult Create(ArticleCreateModel model)
        {
            try
            {
                using (var service = new ArticleService())
                    return service.Create(model);
            }
            catch (Exception ex)
            {
                return Notifization.NotService;
            }
        }

        [HttpPost]
        [Route("Action/Update")]
        public ActionResult Update(ArticleUpdateModel model)
        {
            try
            {
                using (var service = new ArticleService())
                    return service.Update(model);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }

        [HttpPost]
        [Route("Action/Delete")]
        public ActionResult Delete(ArticleIDModel model)
        {
            try
            {
                using (var service = new ArticleService())
                    return service.Delete(model.ID);
            }
            catch (Exception ex)
            {
                return Notifization.TEST("" + ex);
            }
        }
    }
}