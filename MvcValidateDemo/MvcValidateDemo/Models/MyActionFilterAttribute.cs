using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcValidateDemo.Models
{
    //AllowMultiple = true:允许多个标签同时都起作用
    [AttributeUsage(AttributeTargets.All,AllowMultiple = true)]
    public class MyActionFilterAttribute : ActionFilterAttribute
    {
        public string Name { get; set; }

        //Action 执行之前先执行此方法
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);


            //HttpContext.Current.Session[""]

            HttpContext.Current.Response.Write("<br />OnActionExecuting ：" + Name);
            
        }

        //Action执行之后
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);
            HttpContext.Current.Response.Write("<br />OnActionExecuted ：" + Name);
        }

        //ActionResult执行之前先执行此方法
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            base.OnResultExecuting(filterContext);
            HttpContext.Current.Response.Write("<br />OnResultExecuting ：" + Name);

        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            base.OnResultExecuted(filterContext);
            HttpContext.Current.Response.Write("<br />OnResultExecuted ：" + Name);
        }
    }
}