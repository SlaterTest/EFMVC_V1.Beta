﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EFMVC.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Limitcontrol TEST";
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
    }
}
