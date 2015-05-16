using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EF_WebApplication1.Controllers
{
    public class BaseController : Controller
    {
        /// <summary>
        /// 是否有权限
        /// </summary>
        public static bool IsAdmin
        {
            get
            {
                var flag = false;
                if (System.Web.HttpContext.Current.Session["IsAdmin"] != null)
                {
                    flag = bool.Parse(System.Web.HttpContext.Current.Session["IsAdmin"].ToString());
                }
                return flag;
            }
        }
    }
}