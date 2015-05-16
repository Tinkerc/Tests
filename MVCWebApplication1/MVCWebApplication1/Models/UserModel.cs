using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCWebApplication1.Models
{
    public class UserModel
    {
        [Required]
        public long Id { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        [Display(Name = "姓名")]
        [Remote("CheckUserName", "User",ErrorMessage = "账户{0}重复")]
        public string UserName { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        [Display(Name = "密码")]
        public string PassWord1 { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 6)]
        [System.ComponentModel.DataAnnotations.Compare("PassWord1")]
        [Display(Name = "确认密码")]
        public string PassWord2 { get; set; }

        [Required]
        [Range(18, 35)]
        [Display(Name = "年龄")]
        public int UserAge { get; set; }
    }
}