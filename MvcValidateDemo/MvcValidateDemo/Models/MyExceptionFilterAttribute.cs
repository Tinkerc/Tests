using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcValidateDemo.Models
{
    public class MyExceptionFilterAttribute: HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            base.OnException(filterContext);

            //当出现了异常的时候，才执行此方法

            //记录日志
            //多个线程同时访问一个日志文件
            //性能非得低。
            //考虑使用内存队列提高性能，Redis
            //加入观察者模式屏蔽写入不同地方的变化点
            //log4net

            //页面跳转到错误页面或者是首页
            HttpContext.Current.Response.Redirect("/Home/Index");
        }
    }
}