using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LXShopping.Infrastructure;
using LXShopping.Models;

namespace LXShopping.Controllers
{
    [MemberAuthorize]
    public class CartController : BaseController
    {
        // 同一件商品一次最多买 99 件
        private const int MaxPerLine = 99;

        // 购物车页面
        public ActionResult Index()
        {
            return View(BuildCartViewModel());
        }

        // 购物车里一共有几件，顶部导航栏那个角标显示的就是这个数
        [AllowAnonymous]
        public ActionResult Count()
        {
            var id = CurrentMemberId;
            if (!id.HasValue) return Json(new { ok = true, count = 0 }, JsonRequestBehavior.AllowGet);

            var count = db.Carts.Where(c => c.Member.Id == id.Value).Sum(c => (int?)c.Amount) ?? 0;
            return Json(new { ok = true, count = count }, JsonRequestBehavior.AllowGet);
        }

        // 加入购物车，库存这里就先扣掉
        [HttpPost]
        public ActionResult AddToCart(int productId, int amount = 1)
        {
            if (amount < 1) amount = 1;
            if (amount > MaxPerLine) amount = MaxPerLine;

            var id = CurrentMemberId.Value;
            var product = db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return FailJson("商品不存在或已下架");
            if (!product.IsPublished) return FailJson("该商品已下架");
            if (product.Amount <= 0) return FailJson("该商品已售罄");
            if (product.Amount < amount) return FailJson("库存不足，当前仅剩 " + product.Amount + " 件");

            var member = db.Members.FirstOrDefault(m => m.Id == id);
            if (member == null) return FailJson("登录状态已失效，请重新登录");

            var line = db.Carts.Include(c => c.Product)
                               .FirstOrDefault(c => c.Member.Id == id && c.Product.Id == productId);

            if (line != null)
            {
                if (line.Amount + amount > MaxPerLine)
                    return FailJson("同一商品最多一次购买 " + MaxPerLine + " 件");

                line.Amount += amount;
                product.Amount -= amount;
            }
            else
            {
                db.Carts.Add(new Cart
                {
                    Member = member,
                    Product = product,
                    Amount = amount
                });
                product.Amount -= amount;
            }

            db.SaveChanges();

            var count = db.Carts.Where(c => c.Member.Id == id).Sum(c => (int?)c.Amount) ?? 0;
            return OkJson(new { count = count }, "已加入购物车");
        }

        // 从购物车删掉一件商品，把之前扣的库存加回去
        [HttpPost]
        public ActionResult Remove(int productId)
        {
            var id = CurrentMemberId.Value;
            var line = db.Carts.Include(c => c.Product)
                               .FirstOrDefault(c => c.Member.Id == id && c.Product.Id == productId);
            if (line == null) return FailJson("购物车中没有该商品");

            line.Product.Amount += line.Amount;
            db.Carts.Remove(line);
            db.SaveChanges();

            var count = db.Carts.Where(c => c.Member.Id == id).Sum(c => (int?)c.Amount) ?? 0;
            return OkJson(new { count = count }, "已从购物车移除");
        }

        // 清空购物车，顺手把每件商品的库存都还回去
        [HttpPost]
        public ActionResult Clear()
        {
            var id = CurrentMemberId.Value;
            var lines = db.Carts.Include(c => c.Product).Where(c => c.Member.Id == id).ToList();
            foreach (var line in lines)
            {
                line.Product.Amount += line.Amount;
                db.Carts.Remove(line);
            }
            db.SaveChanges();
            return OkJson(new { count = 0 }, "购物车已清空");
        }

        // 改购买数量，库存已经扣过了，这里只处理差出来的部分
        [HttpPost]
        public ActionResult UpdateAmount(int productId, int amount)
        {
            if (amount < 1) amount = 1;
            if (amount > MaxPerLine) amount = MaxPerLine;

            var id = CurrentMemberId.Value;
            var line = db.Carts.Include(c => c.Product)
                               .FirstOrDefault(c => c.Member.Id == id && c.Product.Id == productId);
            if (line == null) return FailJson("购物车中没有该商品");

            var delta = amount - line.Amount;
            if (delta > 0 && line.Product.Amount < delta)
                return FailJson("库存不足，当前仅剩 " + line.Product.Amount + " 件，无法再加购");

            line.Product.Amount -= delta;
            line.Amount = amount;
            db.SaveChanges();

            var vm = BuildCartViewModel();
            return OkJson(new
            {
                amount = line.Amount,
                lineTotal = LXHelper.Money(line.SubTotal),
                subTotal = LXHelper.Money(vm.SubTotal),
                shipFee = LXHelper.Money(vm.ShipFee),
                payable = LXHelper.Money(vm.Payable),
                count = vm.TotalQuantity
            }, "数量已更新");
        }

        private CartViewModel BuildCartViewModel()
        {
            var id = CurrentMemberId.Value;
            var items = db.Carts
                .Include(c => c.Product)
                .Include(c => c.Product.ProductCategory)
                .Where(c => c.Member.Id == id)
                .OrderByDescending(c => c.Id)
                .ToList();

            return new CartViewModel
            {
                Items = items,
                SubTotal = items.Sum(i => i.SubTotal),
                TotalQuantity = items.Sum(i => i.Amount)
            };
        }
    }
}
