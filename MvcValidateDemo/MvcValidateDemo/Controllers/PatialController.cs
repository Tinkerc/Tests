using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcValidateDemo.Controllers
{
    public class PatialController : Controller
    {
        //
        // GET: /Patial/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewData["key"] = "Action 执行了";

            ViewBag.Demo = "Action bag 执行了";

            return View();
        }

    }
}
