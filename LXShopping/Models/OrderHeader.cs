using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace LXShopping.Models
{
    // 订单主档，谁下的、多少钱、什么时候下的
    [DisplayName("订单")]
    [DisplayColumn("OrderNo")]
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("订单编号")]
        [MaxLength(30)]
        public string OrderNo { get; set; }

        [DisplayName("收货地址")]
        [Required]
        public virtual MemberAddress MemberAddress { get; set; }

        [DisplayName("订单金额")]
        [Required]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        [DisplayName("订单备注")]
        [DataType(DataType.MultilineText)]
        [MaxLength(200, ErrorMessage = "订单备注不可超过 200 个字")]
        public string Memo { get; set; }

        [DisplayName("下单时间")]
        public DateTime BuyOn { get; set; }

        [DisplayName("支付方式")]
        [MaxLength(20)]
        public string PayMethod { get; set; }

        public virtual ICollection<OrderDetail> OrderDetailItems { get; set; }

        // 这张订单里的商品是不是全都发货了
        public bool IsAllShipped
        {
            get
            {
                var items = OrderDetailItems;
                if (items == null || items.Count == 0) return false;
                return items.All(i => !string.IsNullOrEmpty(i.Shipmentnumber));
            }
        }

        // 这张订单里还有没有没发货的商品
        public bool HasUnshipped
        {
            get
            {
                var items = OrderDetailItems;
                if (items == null || items.Count == 0) return false;
                return items.Any(i => string.IsNullOrEmpty(i.Shipmentnumber));
            }
        }

        // 页面上显示的订单状态：已发货 / 部分发货 / 待发货
        public string StatusText
        {
            get
            {
                if (HasUnshipped && IsAllShipped == false && OrderDetailItems != null &&
                    OrderDetailItems.Any(i => !string.IsNullOrEmpty(i.Shipmentnumber)))
                    return "部分发货";
                if (IsAllShipped) return "已发货";
                return "待发货";
            }
        }

        // 这张订单一共买了几件商品，把每一行的数量加起来
        public int TotalQuantity
        {
            get { return OrderDetailItems == null ? 0 : OrderDetailItems.Sum(i => i.Amount); }
        }
    }
}
