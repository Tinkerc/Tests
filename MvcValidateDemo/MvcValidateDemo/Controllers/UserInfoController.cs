using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcValidateDemo.Models;

namespace MvcValidateDemo.Controllers
{
    public class UserInfoController : Controller
    {
      
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(UserInfo userInfo)
        {
            //ModelState.IsValid=true那么校验就是成功的。
            if (ModelState.IsValid)
            {
                
            }

            return RedirectToAction("Index");
        }
    }
}
