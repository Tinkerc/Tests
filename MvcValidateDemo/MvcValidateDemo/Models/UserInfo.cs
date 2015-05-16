using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcValidateDemo.Models
{
    public class UserInfo
    {
       
        public int Id { get; set; }

        [StringLength(5,ErrorMessage = "*长度必须<5")]
        [Required(ErrorMessage = "*必填姓名")]
        public string UserName { get; set; }

        [RegularExpression(@"^\d+$")]
        [Range(18,120)]
        [Required(ErrorMessage = "*")]
        public int Age { get; set; }
    }
}