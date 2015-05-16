using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using EF_WebApplication1.Controllers;

namespace EF_WebApplication1.Filter
{
    /// <summary>
    /// 验证是否有权限操作
    /// </summary>
    public class MyActionFilterAttribute : ActionFilterAttribute
    {
        public string ActionName { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (BaseController.IsAdmin)
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {
                File.AppendAllText(HttpContext.Current.Server.MapPath("/App_Data/") + "log.txt", "无权限访问" + ActionName);

                HttpContext.Current.Response.Redirect("/Home/Index");
            }
        }
    }
}