using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcValidateDemo.Controllers
{
    public class AjaxController : Controller
    {
        //
        // GET: /Ajax/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetDate()
        {
            //让网站睡眠1秒钟
            System.Threading.Thread.Sleep(1000);
            return Content(DateTime.Now.ToString());
        }

        public ActionResult MicrosoftAjax()
        {
            return View();
        }

    }
}
