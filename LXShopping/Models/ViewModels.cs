using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Routing;
using PagedList;

namespace LXShopping.Models
{
    // 首页要用到的数据
    public class HomeViewModel
    {
        public IPagedList<Product> Products { get; set; }
        public IList<ProductCategory> Categories { get; set; }
        public IList<Product> HotProducts { get; set; }
        public IList<Product> NewProducts { get; set; }
        public string Search { get; set; }
        public int TotalProductCount { get; set; }
        public int TotalCategoryCount { get; set; }
    }

    // 商品列表页要用到的数据
    public class ProductListViewModel
    {
        public IPagedList<Product> Products { get; set; }
        public IList<ProductCategory> Categories { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Search { get; set; }
        public string Sort { get; set; }        // 排序方式：default / price-asc / price-desc / sales / new
    }

    // 商品详情页要用到的数据
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public IList<Product> Related { get; set; }
        public bool IsFavorite { get; set; }
    }

    // 购物车页面要用到的数据
    public class CartViewModel
    {
        public IList<Cart> Items { get; set; }
        public decimal SubTotal { get; set; }
        public int TotalQuantity { get; set; }

        public CartViewModel()
        {
            Items = new List<Cart>();
        }

        // 运费。买满 99 就免运费，购物车是空的也不用算运费
        public decimal ShipFee
        {
            get { return SubTotal >= 99m || SubTotal == 0m ? 0m : 10m; }
        }

        // 最后要付的钱 = 商品小计 + 运费
        public decimal Payable
        {
            get { return SubTotal + ShipFee; }
        }
    }

    // 结算页要用到的数据
    public class CheckoutViewModel
    {
        public IList<Cart> Items { get; set; }
        public IList<MemberAddress> Addresses { get; set; }
        public MemberAddress Address { get; set; }
        public decimal Total { get; set; }
        public int TotalQuantity { get; set; }

        public CheckoutViewModel()
        {
            Items = new List<Cart>();
            Addresses = new List<MemberAddress>();
            Address = new MemberAddress();
        }
    }

    // 会员中心（个人中心）要用到的数据
    public class MemberCenterViewModel
    {
        public Member Member { get; set; }
        public IList<OrderDetail> AllOrders { get; set; }
        public IList<OrderHeader> OrderHeaders { get; set; }
        public IList<MemberAddress> Addresses { get; set; }
        public decimal TotalSpent { get; set; }
        public int UnshippedCount { get; set; }

        public MemberCenterViewModel()
        {
            AllOrders = new List<OrderDetail>();
            OrderHeaders = new List<OrderHeader>();
            Addresses = new List<MemberAddress>();
        }
    }

    // 后台首页的仪表盘，把各种统计数字都塞在一起了
    public class DashboardViewModel
    {
        public int ProductCount { get; set; }
        public int OnShelfCount { get; set; }
        public int MemberCount { get; set; }
        public int OrderCount { get; set; }
        public int OrderItemCount { get; set; }
        public int UnshippedCount { get; set; }
        public int TodayOrderCount { get; set; }
        public int NewMemberCount { get; set; }
        public decimal Turnover { get; set; }
        public decimal AvgOrderPrice { get; set; }
        public decimal StockValue { get; set; }
        public IList<Product> LowStockProducts { get; set; }
        public IList<OrderHeader> RecentOrders { get; set; }
        public IList<CategoryStat> CategoryStats { get; set; }
        public IList<DayStat> DayStats { get; set; }

        public DashboardViewModel()
        {
            LowStockProducts = new List<Product>();
            RecentOrders = new List<OrderHeader>();
            CategoryStats = new List<CategoryStat>();
            DayStats = new List<DayStat>();
        }
    }

    // 分类统计，画饼图用
    public class CategoryStat
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public int Stock { get; set; }
    }

    // 最近 7 天的下单情况，画折线图用
    public class DayStat
    {
        public string Label { get; set; }
        public int Orders { get; set; }
        public decimal Amount { get; set; }
    }

    // 修改密码页用的
    public class ChangePasswordViewModel
    {
        [DisplayName("当前密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "请输入当前密码")]
        public string OldPassword { get; set; }

        [DisplayName("新密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "请输入新密码")]
        [MinLength(6, ErrorMessage = "新密码至少 6 位")]
        [MaxLength(20, ErrorMessage = "新密码不可超过 20 位")]
        public string NewPassword { get; set; }

        [DisplayName("确认新密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "请再次输入新密码")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "两次输入的密码不一致")]
        public string ConfirmPassword { get; set; }
    }

    // 后台登录页用的
    public class AdminLoginViewModel
    {
        [DisplayName("管理员账号")]
        [Required(ErrorMessage = "请输入管理员账号")]
        public string AdminName { get; set; }

        [DisplayName("密码")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "请输入密码")]
        public string Pwd { get; set; }
    }

    // 分页条用的数据，配合 _Pager.cshtml
    public class PagerModel
    {
        // 控制器传进来的分页结果
        public IPagedList Data { get; set; }

        // 翻页时要保留的参数（不含页码）
        public RouteValueDictionary RouteValues { get; set; }

        // 页码用的参数名，默认是 p
        public string PageParam { get; set; }

        public PagerModel()
        {
            RouteValues = new RouteValueDictionary();
            PageParam = "p";
        }

        // 复制一份参数并把页码换掉，用来拼翻页链接
        public RouteValueDictionary ValuesFor(int page)
        {
            var dict = new RouteValueDictionary();
            foreach (var pair in RouteValues)
            {
                if (pair.Value == null) continue;
                var text = pair.Value as string;
                if (text != null && text.Length == 0) continue;
                dict[pair.Key] = pair.Value;
            }
            dict[PageParam] = page;
            return dict;
        }
    }
}
