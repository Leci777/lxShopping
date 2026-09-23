using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LXShopping.Infrastructure;
using LXShopping.Models;

namespace LXShopping.Controllers
{
    // 所有控制器的父类，每个请求一个 DbContext
    public class BaseController : Controller
    {
        private LXShoppingContext _db;

        // 数据库上下文。用到的时候才 new，不用就不创建
        protected LXShoppingContext db
        {
            get { return _db ?? (_db = new LXShoppingContext()); }
        }

        // 当前登录会员的 id，没登录就返回 null
        protected int? CurrentMemberId
        {
            get { return Session == null ? null : Session["memberid"] as int?; }
        }

        // 当前登录的会员信息，没登录就返回 null
        protected Member CurrentMember
        {
            get
            {
                var id = CurrentMemberId;
                if (!id.HasValue) return null;
                return db.Members.FirstOrDefault(m => m.Id == id.Value);
            }
        }

        // 会员登录了没有
        protected bool IsMemberLoggedIn
        {
            get { return User != null && User.Identity.IsAuthenticated && CurrentMemberId.HasValue; }
        }

        // 当前登录的管理员名字
        protected string CurrentAdminName
        {
            get { return Session == null ? null : Session["adminname"] as string; }
        }

        // 设一条提示消息，跳到下一个页面之后会在右上角弹出来
        protected void SetToast(string message, string type = "success")
        {
            TempData["LX_Toast"] = message;
            TempData["LX_ToastType"] = type;
        }

        // Ajax 请求成功，统一返回成这个格式
        protected JsonResult OkJson(object data = null, string message = null)
        {
            return Json(new { ok = true, message = message, data = data },
                JsonRequestBehavior.AllowGet);
        }

        // Ajax 请求失败，统一返回成这个格式
        protected JsonResult FailJson(string message)
        {
            return Json(new { ok = false, message = message },
                JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _db != null)
            {
                _db.Dispose();
                _db = null;
            }
            base.Dispose(disposing);
        }
    }
}
