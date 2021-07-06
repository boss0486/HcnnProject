﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebCore.Core;

namespace WebApplication.Management.Controllers
{
    [RouteArea("Management")]
    [RoutePrefix("Home-Page")]
    public class HomeController : CMSController
    {
        // GET: Management/Home
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Test()
        {
            return View();
        }
    }
}