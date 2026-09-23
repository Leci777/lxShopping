using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using LXShopping.Models;
using PagedList;

namespace LXShopping.Infrastructure
{
    // 页面里会重复用到的小方法
    public static class LxHtmlHelpers
    {
        // 生成底下的分页条，用法：
        //   @Html.LxPager(Model.Products)
        //   @Html.LxPager(Model.Products, new { id = Model.CategoryId, sort = Model.Sort })
        //   @Html.LxPager(orders, new { tab = "orders" }, "p2")
        // 最后一个参数是页码的参数名，后台几个列表分页要分开
        public static IHtmlString LxPager(this HtmlHelper html, IPagedList data, object routeValues = null, string pageParam = "p")
        {
            if (html == null || data == null || data.PageCount <= 1)
            {
                return MvcHtmlString.Empty;
            }

            var values = routeValues == null
                ? new RouteValueDictionary()
                : new RouteValueDictionary(routeValues);

            var model = new PagerModel
            {
                Data = data,
                PageParam = string.IsNullOrEmpty(pageParam) ? "p" : pageParam,
                RouteValues = values
            };

            return html.Partial("_Pager", model);
        }
    }
}
