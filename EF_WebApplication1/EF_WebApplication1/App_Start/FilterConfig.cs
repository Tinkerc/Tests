using System.Web;
using System.Web.Mvc;
using EF_WebApplication1.Filter;

namespace EF_WebApplication1
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
           // filters.Add(new MyActionFilterAttribute());
        }
    }
}
