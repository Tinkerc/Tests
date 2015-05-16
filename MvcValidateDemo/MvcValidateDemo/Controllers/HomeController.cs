
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcValidateDemo.Models;

namespace MvcValidateDemo.Controllers
{
    [MyActionFilter(Name = "HomeController")]
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        [MyActionFilter(Name = "Index Action")]
        public ActionResult Index()
        {
            Response.Write("<p>Action执行了</p>");

            return Content("<br />ok:视图被渲染<br />");
        }


         
        public ActionResult About()
        {

            //throw  new Exception("demo");

            return Content("<P>About 渲染</P>");
        }

    }
}
