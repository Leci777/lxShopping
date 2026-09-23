using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LXShopping.Models
{
    // 订单明细，一张订单里买了哪几样商品就记在这里
    [DisplayName("订单明细")]
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("订单主档")]
        [Required]
        public virtual OrderHeader OrderHeader { get; set; }

        [DisplayName("订购商品")]
        [Required]
        public virtual Product Product { get; set; }

        [DisplayName("商品单价")]
        [Required]
        [DataType(DataType.Currency)]
        [Description("下单当下的商品单价，商品改价不影响历史订单")]
        public decimal UnitPrice { get; set; }

        [DisplayName("小计金额")]
        [Required]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [DisplayName("购买数量")]
        [Required]
        [Range(1, 999)]
        public int Amount { get; set; }

        [DisplayName("发货单号")]
        [MaxLength(40)]
        public string Shipmentnumber { get; set; }

        // 下单时把商品名复制存一份，商家改名后旧订单还显示原名
        [DisplayName("商品名称")]
        [MaxLength(60)]
        public string ProductName { get; set; }

        public bool IsShipped
        {
            get { return !string.IsNullOrEmpty(Shipmentnumber); }
        }
    }
}
