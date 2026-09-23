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
    public class AboutMemberController : BaseController
    {
        // 个人中心，tab 决定显示哪一块
        public ActionResult Index(string tab = null)
        {
            var memberId = CurrentMemberId.Value;
            var member = db.Members.FirstOrDefault(m => m.Id == memberId);
            if (member == null)
            {
                SetToast("登录状态已失效，请重新登录", "error");
                return RedirectToAction("Login", "Member");
            }

            var headers = db.Orders
                .Include(o => o.MemberAddress)
                .Include(o => o.OrderDetailItems.Select(d => d.Product))
                .Where(o => o.MemberAddress.Member.Id == memberId)
                .OrderByDescending(o => o.BuyOn)
                .ThenByDescending(o => o.Id)
                .ToList();

            var details = headers.SelectMany(o => o.OrderDetailItems).ToList();

            var vm = new MemberCenterViewModel
            {
                Member = member,
                OrderHeaders = headers,
                AllOrders = details,
                Addresses = db.MemberAddresses
                    .Where(a => a.Member.Id == memberId)
                    .OrderByDescending(a => a.Id)
                    .ToList(),
                TotalSpent = headers.Sum(o => o.TotalPrice),
                UnshippedCount = details.Count(d => !d.IsShipped)
            };

            ViewBag.ActiveTab = string.IsNullOrEmpty(tab) ? "profile" : tab;
            return View(vm);
        }

        // 改个人资料，昵称、姓名和头像都能改
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MemberEdit(HttpPostedFileBase image = null)
        {
            var memberId = CurrentMemberId.Value;
            var member = db.Members.FirstOrDefault(m => m.Id == memberId);
            if (member == null) return RedirectToAction("Login", "Member");

            var name = (Request.Form["Name"] ?? string.Empty).Trim();
            var nickname = (Request.Form["Nickname"] ?? string.Empty).Trim();

            if (name.Length == 0 || name.Length > 5)
            {
                SetToast("姓名不能为空且不可超过 5 个字", "error");
                return RedirectToAction("Index", new { tab = "settings" });
            }
            if (nickname.Length == 0 || nickname.Length > 10)
            {
                SetToast("用户名不能为空且不可超过 10 个字", "error");
                return RedirectToAction("Index", new { tab = "settings" });
            }

            string error;
            var uploaded = LXHelper.ReadImage(image, out error);
            if (!string.IsNullOrEmpty(error))
            {
                SetToast(error, "error");
                return RedirectToAction("Index", new { tab = "settings" });
            }

            member.Name = name;
            member.Nickname = nickname;
            if (uploaded != null)
            {
                member.ImageData = uploaded.Data;
                member.ImageMimeType = uploaded.MimeType;
            }

            db.SaveChanges();
            Session["name"] = member.Nickname;

            SetToast("个人资料已更新", "success");
            return RedirectToAction("Index", new { tab = "settings" });
        }

        // 保存收货地址，id 为 0 是新增，否则是修改
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddressSave(int id = 0)
        {
            var memberId = CurrentMemberId.Value;
            var name = (Request.Form["ContactName"] ?? string.Empty).Trim();
            var phone = (Request.Form["ContactPhoneNo"] ?? string.Empty).Trim();
            var addr = (Request.Form["ContactAddress"] ?? string.Empty).Trim();

            if (name.Length == 0 || phone.Length == 0 || addr.Length == 0)
            {
                SetToast("请完整填写收件人、联系电话与收货地址", "error");
                return RedirectToAction("Index", new { tab = "address" });
            }
            if (name.Length > 40 || phone.Length > 25 || addr.Length > 200)
            {
                SetToast("填写内容过长，请检查后重试", "error");
                return RedirectToAction("Index", new { tab = "address" });
            }

            MemberAddress address;
            if (id > 0)
            {
                address = db.MemberAddresses.FirstOrDefault(a => a.Id == id && a.Member.Id == memberId);
                if (address == null)
                {
                    SetToast("地址不存在", "error");
                    return RedirectToAction("Index", new { tab = "address" });
                }
                address.ContactName = name;
                address.ContactPhoneNo = phone;
                address.ContactAddress = addr;
                SetToast("收货地址已更新", "success");
            }
            else
            {
                var member = db.Members.FirstOrDefault(m => m.Id == memberId);
                db.MemberAddresses.Add(new MemberAddress
                {
                    Member = member,
                    ContactName = name,
                    ContactPhoneNo = phone,
                    ContactAddress = addr
                });
                SetToast("收货地址已添加", "success");
            }

            db.SaveChanges();
            return RedirectToAction("Index", new { tab = "address" });
        }

        // 删除收货地址，被订单用过的不能删
        [HttpPost]
        public ActionResult AddressDelect(int id)
        {
            var memberId = CurrentMemberId.Value;
            var address = db.MemberAddresses.FirstOrDefault(a => a.Id == id && a.Member.Id == memberId);
            if (address == null) return FailJson("地址不存在或无权操作");

            if (db.Orders.Any(o => o.MemberAddress.Id == id))
                return FailJson("该地址已被历史订单使用，无法删除");

            db.MemberAddresses.Remove(address);
            db.SaveChanges();
            return OkJson(null, "地址已删除");
        }
    }
}
