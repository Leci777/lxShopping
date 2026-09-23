using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LXShopping.Models
{
    // 购物车，一个会员买了同一件商品就占一行
    [DisplayName("购物车")]
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("订购会员")]
        [Required]
        public virtual Member Member { get; set; }

        [DisplayName("选购商品")]
        [Required]
        public virtual Product Product { get; set; }

        [DisplayName("选购数量")]
        [Required]
        [Range(1, 999, ErrorMessage = "选购数量必须介于 1 ~ 999 之间")]
        public int Amount { get; set; }

        // 这一行的小计：单价 × 数量，页面上不用再自己算
        public decimal SubTotal
        {
            get { return Product == null ? 0m : Product.Price * Amount; }
        }
    }
}
