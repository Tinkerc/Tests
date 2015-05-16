using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace JSONWebApplication1.Models
{
    public class EmployeeBean
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal Salary { get; set; }
        public string Phone { get; set; }
        
        [JsonIgnore]
        public DateTime HireDate { get; set; }
    }
}