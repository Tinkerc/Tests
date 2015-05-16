using System.Web;
using System.Web.Mvc;
using MvcValidateDemo.Models;

namespace MvcValidateDemo
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new MyExceptionFilterAttribute());

            //全局过滤器，优先级最低，但是可以作用到所有的控制器和action
            //filters.Add(new MyActionFilterAttribute(){Name = "Gloable"});
        }
    }
}