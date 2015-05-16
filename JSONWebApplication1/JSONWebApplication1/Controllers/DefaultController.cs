using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JSONWebApplication1.Models;
using Newtonsoft.Json;


namespace JSONWebApplication1.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
        public ActionResult Index()
        {
            var employeeBean = new EmployeeBean()
            {
                Id = Guid.NewGuid(),
                Name = "gyzhao",
                Email = "gyzhao@gyzhao.com",
                Salary = 10000,
                Phone = "13912390987",
                HireDate = new DateTime(2012, 2, 1)
            };


            return Content(JsonConvert.SerializeObject(employeeBean));
        }
    }
}