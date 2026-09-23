using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LXShopping.Infrastructure
{
    // 会员登录检查，没登录跳登录页，Ajax 就返回 json
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MemberAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ctx = filterContext.HttpContext;
            var memberId = ctx.Session == null ? null : ctx.Session["memberid"] as int?;

            if (!ctx.User.Identity.IsAuthenticated || !memberId.HasValue)
            {
                if (ctx.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { ok = false, needLogin = true, message = "请先登录后再操作" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    var returnUrl = ctx.Request.Url == null ? "/" : ctx.Request.Url.PathAndQuery;
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new
                        {
                            controller = "Member",
                            action = "Login",
                            returnUrl = returnUrl
                        }));
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }

    // 检查管理员有没有登录
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 标了 [AllowAnonymous] 的跳过检查，不然登录页进不去
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            var ctx = filterContext.HttpContext;
            var adminId = ctx.Session == null ? null : ctx.Session["adminid"] as int?;

            if (!adminId.HasValue)
            {
                if (ctx.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new { ok = false, needLogin = true, message = "管理员登录已过期，请重新登录" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Admin", action = "Login" }));
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
