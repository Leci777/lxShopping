using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace LXShopping.Models
{
    // 会员，在前台注册买东西的用户
    [DisplayName("会员资料")]
    [DisplayColumn("Name")]
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("账号")]
        [Required(ErrorMessage = "请输入 Email 地址")]
        [Description("直接以 Email 作为会员的登录账号")]
        [MaxLength(250, ErrorMessage = "Email 地址长度无法超过 250 个字元")]
        [DataType(DataType.EmailAddress)]
        [Remote("CheckDup", "Member", HttpMethod = "POST", ErrorMessage = "您输入的 Email 已经有人注册过了！")]
        public string Email { get; set; }

        [DisplayName("密码")]
        [Required(ErrorMessage = "请输入密码")]
        [MaxLength(40, ErrorMessage = "密码长度不正确")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DisplayName("姓名")]
        [Required(ErrorMessage = "请输入中文姓名")]
        [MaxLength(5, ErrorMessage = "中文姓名不可超过 5 个字")]
        public string Name { get; set; }

        [DisplayName("用户名")]
        [Required(ErrorMessage = "请输入用户名")]
        [MaxLength(10, ErrorMessage = "用户名请勿输入超过 10 个字")]
        public string Nickname { get; set; }

        [DisplayName("注册时间")]
        public DateTime RegisterOn { get; set; }

        [DisplayName("会员头像")]
        public byte[] ImageData { get; set; }

        [DisplayName("图片类型")]
        public string ImageMimeType { get; set; }

        [DisplayName("启用认证码")]
        [MaxLength(36)]
        [Description("AuthCode 等于 null 代表此会员已经通过 Email 有效性验证")]
        public string AuthCode { get; set; }

        public virtual ICollection<MemberAddress> MemberAddresses { get; set; }

        public virtual ICollection<Cart> Carts { get; set; }

        // 取昵称的第一个字，没头像时当文字头像用
        public string Initial
        {
            get
            {
                var source = string.IsNullOrWhiteSpace(Nickname) ? Name : Nickname;
                return string.IsNullOrEmpty(source) ? "L" : source.Substring(0, 1).ToUpper();
            }
        }
    }
}
