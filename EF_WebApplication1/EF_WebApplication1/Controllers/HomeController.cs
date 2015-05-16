using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Applications.Biz;
using Applications.Entity;
using EF_WebApplication1.Filter;
using EF_WebApplication1.Models;

namespace EF_WebApplication1.Controllers
{
    public class HomeController : BaseController
    {
        private readonly UserBiz biz;

        public HomeController()
        {
            biz = new UserBiz();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SetAdmin(bool flag)
        {
            System.Web.HttpContext.Current.Session["IsAdmin"] = flag;

            return new EmptyResult();
        }

        [MyActionFilter(ActionName = "UserList")]
        public ActionResult UserList()
        {
            System.Web.HttpContext.Current.Session["IsAdmin"] = true;
            var list = biz.GetUserList();

            return View(list);
        }

        [MyActionFilter(ActionName = "AddUser")]
        public ActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddUser(UserModel model)
        {
            var result = false;
            if (ModelState.IsValid)
            {
                result = biz.Insert(new UserEntity()
                {
                    Name = model.Name,
                    Pwd = model.Pwd1,
                    Age = model.Age
                });
            }

            /* return Json(result, JsonRequestBehavior.AllowGet);*/

            //让网站睡眠1秒钟
            System.Threading.Thread.Sleep(5000);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 检测用户名是否存在
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public ActionResult CheckUserName(string Name)
        {
            bool flag = true;

            var result = biz.GetUserByCondition(b => b.Name == Name);
            if (result.Any())
            {
                flag = false;
            }

            return Json(flag, JsonRequestBehavior.AllowGet);
        }
    }
}
