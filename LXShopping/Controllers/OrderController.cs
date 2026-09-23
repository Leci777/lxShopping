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
    public class OrderController : BaseController
    {
        // 结算页。购物车要是空的就不让进了，直接送回购物车页面
        public ActionResult Complete()
        {
            var vm = BuildCheckout();
            if (!vm.Items.Any())
            {
                SetToast("购物车还是空的，先去挑几件喜欢的商品吧", "info");
                return RedirectToAction("Index", "Cart");
            }
            return View(vm);
        }

        // 提交订单
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Complete(int addressId = 0, string memo = null, string payMethod = "在线支付")
        {
            var memberId = CurrentMemberId.Value;
            var member = db.Members.FirstOrDefault(m => m.Id == memberId);
            if (member == null) return RedirectToAction("Login", "Member");

            var items = db.Carts
                .Include(c => c.Product)
                .Where(c => c.Member.Id == memberId)
                .ToList();

            if (!items.Any())
            {
                SetToast("购物车是空的，无法提交订单", "error");
                return RedirectToAction("Index", "Cart");
            }

            // 用的是以前存过的地址，直接拿这个下单
            if (addressId > 0)
            {
                var saved = db.MemberAddresses
                    .FirstOrDefault(a => a.Id == addressId && a.Member.Id == memberId);
                if (saved == null)
                {
                    SetToast("请选择有效的收货地址", "error");
                    return RedirectToAction("Complete");
                }
                return PlaceOrder(member, saved, items, memo, payMethod);
            }

            // 用的是这次在结算页现填的新地址
            if (!ModelState.IsValid)
                return View(BuildCheckout());

            var name = (Request.Form["ContactName"] ?? string.Empty).Trim();
            var phone = (Request.Form["ContactPhoneNo"] ?? string.Empty).Trim();
            var addr = (Request.Form["ContactAddress"] ?? string.Empty).Trim();

            if (name.Length == 0 || phone.Length == 0 || addr.Length == 0)
            {
                SetToast("请完整填写收件人、联系电话与收货地址", "error");
                return View(BuildCheckout());
            }
            if (name.Length > 40) name = name.Substring(0, 40);
            if (phone.Length > 25) phone = phone.Substring(0, 25);
            if (addr.Length > 200) addr = addr.Substring(0, 200);

            var address = db.MemberAddresses.FirstOrDefault(a => a.Member.Id == memberId
                                                              && a.ContactName == name
                                                              && a.ContactPhoneNo == phone
                                                              && a.ContactAddress == addr);
            if (address == null)
            {
                address = new MemberAddress
                {
                    Member = member,
                    ContactName = name,
                    ContactPhoneNo = phone,
                    ContactAddress = addr
                };
                db.MemberAddresses.Add(address);
                db.SaveChanges();
            }

            return PlaceOrder(member, address, items, memo, payMethod);
        }

        // 下单成功页，顺便查一下这单是不是当前会员自己的
        public ActionResult Done(string no)
        {
            var memberId = CurrentMemberId.Value;
            var order = db.Orders
                .Include(o => o.MemberAddress)
                .Include(o => o.OrderDetailItems.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderNo == no && o.MemberAddress.Member.Id == memberId);

            if (order == null)
            {
                SetToast("没有找到该订单", "error");
                return RedirectToAction("Index", "Home");
            }
            return View(order);
        }

        // 下面这几个是内部用的小方法，页面上不会直接调
        private CheckoutViewModel BuildCheckout()
        {
            var memberId = CurrentMemberId.Value;
            var items = db.Carts
                .Include(c => c.Product)
                .Where(c => c.Member.Id == memberId)
                .OrderByDescending(c => c.Id)
                .ToList();

            return new CheckoutViewModel
            {
                Items = items,
                Addresses = db.MemberAddresses
                    .Where(a => a.Member.Id == memberId)
                    .OrderByDescending(a => a.Id)
                    .ToList(),
                Address = new MemberAddress(),
                Total = items.Sum(i => i.SubTotal),
                TotalQuantity = items.Sum(i => i.Amount)
            };
        }

        private ActionResult PlaceOrder(Member member, MemberAddress address, IList<Cart> items, string memo, string payMethod)
        {
            var order = new OrderHeader
            {
                OrderNo = LXHelper.NewOrderNo(),
                MemberAddress = address,
                BuyOn = DateTime.Now,
                Memo = string.IsNullOrWhiteSpace(memo) ? null : LXHelper.Ellipsis(memo.Trim(), 200),
                PayMethod = string.IsNullOrWhiteSpace(payMethod) ? "在线支付" : payMethod.Trim(),
                OrderDetailItems = new List<OrderDetail>()
            };

            decimal total = 0m;
            foreach (var line in items)
            {
                var product = db.Products.FirstOrDefault(p => p.Id == line.Product.Id);
                if (product == null) continue;

                var detail = new OrderDetail
                {
                    Product = product,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Amount = line.Amount,
                    Price = product.Price * line.Amount
                };
                total += detail.Price;
                product.Sales += line.Amount;

                order.OrderDetailItems.Add(detail);
                // 库存加购时已扣过，这里只把购物车这行删掉
                db.Carts.Remove(line);
            }

            if (!order.OrderDetailItems.Any())
            {
                SetToast("购物车中的商品都已下架，无法下单", "error");
                return RedirectToAction("Index", "Cart");
            }

            order.TotalPrice = total;
            db.Orders.Add(order);
            db.SaveChanges();

            SetToast("下单成功，感谢您的信任！", "success");
            return RedirectToAction("Done", new { no = order.OrderNo });
        }
    }
}
