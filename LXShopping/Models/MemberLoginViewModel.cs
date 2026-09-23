using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LXShopping.Models
{
    // 登录页面用的，其实就只有账号和密码两个字段
    public class MemberLoginViewModel
    {
        [DisplayName("会员账号")]
        [DataType(DataType.EmailAddress, ErrorMessage = "请输入正确的 Email 地址")]
        [Required(ErrorMessage = "请输入{0}")]
        public string email { get; set; }

        [DisplayName("会员密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "请输入{0}")]
        public string password { get; set; }

        [DisplayName("记住我")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}
