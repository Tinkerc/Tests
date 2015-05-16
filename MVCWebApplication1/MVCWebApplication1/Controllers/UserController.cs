using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;
using MVCWebApplication1.Models;

namespace MVCWebApplication1.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(UserModel model)
        {
            if (ModelState.IsValid)
            {
                return Json(model, JsonRequestBehavior.AllowGet);
            }

            return Content("失败");
        }

        public ActionResult CheckUserName(string UserName)
        {
            bool result = true;

            if (UserName == "administrator")
            {
                result = false;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}