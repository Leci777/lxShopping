using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LXShopping.Models
{
    // 后台管理员，登录后台用的账号
    [DisplayName("管理员")]
    [DisplayColumn("ChinaName")]
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("管理员账号")]
        [Required(ErrorMessage = "请输入管理员账号")]
        [MaxLength(40)]
        public string AdminName { get; set; }

        [DisplayName("密码")]
        [Required(ErrorMessage = "请输入密码")]
        [MaxLength(40)]
        [DataType(DataType.Password)]
        public string Pwd { get; set; }

        [DisplayName("姓名")]
        [Required(ErrorMessage = "请输入姓名")]
        [MaxLength(20)]
        public string ChinaName { get; set; }

        [DisplayName("角色")]
        [MaxLength(20)]
        public string Role { get; set; }
    }
}
