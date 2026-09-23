using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LXShopping.Models
{
    // 会员的收货地址，一个会员可以存好几个地址
    [DisplayName("收货地址")]
    public class MemberAddress
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("所属会员")]
        [Required]
        public virtual Member Member { get; set; }

        [DisplayName("收件人姓名")]
        [Required(ErrorMessage = "请输入收件人姓名")]
        [MaxLength(40, ErrorMessage = "收件人姓名长度不可超过 40 个字元")]
        public string ContactName { get; set; }

        [DisplayName("联络电话")]
        [Required(ErrorMessage = "请输入联络电话")]
        [MaxLength(25, ErrorMessage = "电话号码长度不可超过 25 个字元")]
        [DataType(DataType.PhoneNumber)]
        public string ContactPhoneNo { get; set; }

        [DisplayName("收货地址")]
        [Required(ErrorMessage = "请输入商品收货地址")]
        [MaxLength(200, ErrorMessage = "地址长度不可超过 200 个字")]
        public string ContactAddress { get; set; }

        // 地址太长页面放不下，截到 22 个字加省略号
        public string ShortAddress
        {
            get
            {
                if (string.IsNullOrEmpty(ContactAddress)) return "";
                return ContactAddress.Length > 22 ? ContactAddress.Substring(0, 22) + "…" : ContactAddress;
            }
        }
    }
}
