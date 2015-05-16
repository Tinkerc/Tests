using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using Applications.Entity;

namespace EF_WebApplication1.Models
{
    public class UserModel
    {
        [Required]
        [Display(Name = "姓名")]
        [Remote("CheckUserName", "Home", ErrorMessage = "姓名重复")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "密码")]
        public string Pwd1 { get; set; }

        [Required]
        [Display(Name = "确认密码")]
        [System.ComponentModel.DataAnnotations.Compare("Pwd1",ErrorMessage = "确认密码不一致")]
        public string Pwd2 { get; set; }

        [Required]
        [Range(18, 35)]
        [Display(Name = "年龄")]
        public int Age { get; set; }
    }
}
