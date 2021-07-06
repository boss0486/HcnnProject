using Helper;
using System.Web.Mvc;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using WebCore.Core;
using WebCore.Services;
using WebCore.Entities;
using Helper.User;
using System;

namespace WebApplication.HomePage.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.UId = string.Empty;
            ViewBag.PId = string.Empty;
            ViewBag.Rmb = string.Empty;
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                //do something
                HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                var _login = JsonConvert.DeserializeObject<CookiModel>(authTicket.UserData);
                if (_login != null && _login.IsRemember)
                {
                    ViewBag.UId = _login.LoginID;
                    ViewBag.PId = _login.Password;
                    ViewBag.Rmb = "checked";
                }
            }
            return View("~/Areas/Authentication/Views/Authen/Login.cshtml");
        }
    }
}