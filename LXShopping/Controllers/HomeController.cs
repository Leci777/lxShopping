using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LXShopping.Infrastructure;
using LXShopping.Models;
using PagedList;

namespace LXShopping.Controllers
{
    public class HomeController : BaseController
    {
        private const int PageSize = 12;

        // 首页，没搜索词就显示热销和新品，有就显示搜索结果
        public ActionResult Index(string search = null, int p = 1)
        {
            var query = db.Products
                .Include(x => x.ProductCategory)
                .Where(x => x.IsPublished);

            var vm = new HomeViewModel
            {
                Categories = db.ProductCategories.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList(),
                Search = search,
                TotalProductCount = query.Count(),
                TotalCategoryCount = db.ProductCategories.Count()
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(x => x.Name.Contains(keyword)
                                      || x.Made.Contains(keyword)
                                      || x.Description.Contains(keyword)
                                      || x.ProductCategory.Name.Contains(keyword));

                vm.Products = query
                    .OrderByDescending(x => x.Sales)
                    .ThenByDescending(x => x.Id)
                    .ToPagedList(SafePage(p), PageSize);
                return View(vm);
            }

            vm.Products = query
                .OrderByDescending(x => x.Id)
                .ToPagedList(SafePage(p), PageSize);

            vm.HotProducts = db.Products
                .Include(x => x.ProductCategory)
                .Where(x => x.IsPublished)
                .OrderByDescending(x => x.Sales)
                .Take(8)
                .ToList();

            vm.NewProducts = db.Products
                .Include(x => x.ProductCategory)
                .Where(x => x.IsPublished)
                .OrderByDescending(x => x.Id)
                .Take(4)
                .ToList();

            return View(vm);
        }

        // 商品列表页，从分类点进来带 id，搜索和排序也在这
        public ActionResult ProductList(int id = 0, string search = null, string sort = null, int p = 1)
        {
            var query = db.Products.Include(x => x.ProductCategory).Where(x => x.IsPublished);

            string categoryName = null;
            if (id != 0)
            {
                var category = db.ProductCategories.Find(id);
                if (category == null) return HttpNotFound();
                categoryName = category.Name;
                query = query.Where(x => x.ProductCategory.Id == id);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(x => x.Name.Contains(keyword)
                                      || x.Made.Contains(keyword)
                                      || x.Description.Contains(keyword));
            }

            switch (sort)
            {
                case "price-asc":
                    query = query.OrderBy(x => x.Price);
                    break;
                case "price-desc":
                    query = query.OrderByDescending(x => x.Price);
                    break;
                case "sales":
                    query = query.OrderByDescending(x => x.Sales);
                    break;
                case "new":
                    query = query.OrderByDescending(x => x.Id);
                    break;
                default:
                    query = query.OrderByDescending(x => x.IsPublished).ThenByDescending(x => x.Sales);
                    break;
            }

            var vm = new ProductListViewModel
            {
                Products = query.ToPagedList(SafePage(p), PageSize),
                Categories = db.ProductCategories.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList(),
                CategoryId = id == 0 ? (int?)null : id,
                CategoryName = categoryName,
                Search = search,
                Sort = sort
            };

            return View(vm);
        }

        // 商品详情页。商品要是下架了就直接跳回列表页
        public ActionResult ProductDetail(int id)
        {
            var product = db.Products
                .Include(x => x.ProductCategory)
                .FirstOrDefault(x => x.Id == id);

            if (product == null || !product.IsPublished)
                return RedirectToAction("ProductList");

            var vm = new ProductDetailViewModel
            {
                Product = product,
                Related = db.Products
                    .Include(x => x.ProductCategory)
                    .Where(x => x.IsPublished
                                && x.Id != id
                                && x.ProductCategory.Id == product.ProductCategory.Id)
                    .OrderByDescending(x => x.Sales)
                    .Take(4)
                    .ToList()
            };

            if (!vm.Related.Any())
            {
                vm.Related = db.Products
                    .Include(x => x.ProductCategory)
                    .Where(x => x.IsPublished && x.Id != id)
                    .OrderByDescending(x => x.Sales)
                    .Take(4)
                    .ToList();
            }

            return View(vm);
        }

        // 返回商品图片的字节，没图就现画一张顶上
        public ActionResult GetImage(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == id);
            if (product != null && product.ImageData != null && product.ImageData.Length > 0)
            {
                return File(product.ImageData, string.IsNullOrEmpty(product.ImageMimeType) ? "image/png" : product.ImageMimeType);
            }

            var fallback = LXHelper.BuildPlaceholderSvg(
                product == null ? "LX" : product.Name,
                product == null ? "LX 优选" : product.Made, id);
            return File(fallback, "image/svg+xml");
        }

        // 关于我们，一张静态介绍页，没什么逻辑
        public ActionResult About()
        {
            return View();
        }

        private static int SafePage(int p)
        {
            return p < 1 ? 1 : p;
        }
    }
}
