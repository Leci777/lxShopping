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
    [AdminAuthorize]
    public class AdminController : BaseController
    {
        private const int PageSize = 10;

        // 后台的登录和登出
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Session["adminid"] is int) return RedirectToAction("Index");
            ViewBag.ReturnUrl = returnUrl;
            return View(new AdminLoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string AdminName, string Pwd, string returnUrl)
        {
            AdminName = (AdminName ?? string.Empty).Trim();
            Pwd = Pwd ?? string.Empty;

            var hash = LXHelper.HashPassword(Pwd);
            var admin = db.Admins.FirstOrDefault(a => a.AdminName == AdminName && a.Pwd == hash);

            if (admin == null)
            {
                ModelState.AddModelError("", "您输入的账号或密码错误");
                return View(new AdminLoginViewModel { AdminName = AdminName });
            }

            Session["adminid"] = admin.Id;
            Session["adminname"] = admin.ChinaName;
            Session["adminrole"] = admin.Role;

            SetToast("欢迎回来，" + admin.ChinaName, "success");

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session.Remove("adminid");
            Session.Remove("adminname");
            Session.Remove("adminrole");
            Session.Clear();
            SetToast("已退出后台管理", "info");
            return RedirectToAction("Login");
        }

        // 后台首页的仪表盘
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.ProductCategory).ToList();
            var orders = db.Orders
                .Include(o => o.MemberAddress)
                .Include(o => o.OrderDetailItems)
                .ToList();

            // 日期先在外面算好，直接写进条件里 EF 翻译不了
            var memberCount = db.Members.Count();
            var newMemberSince = DateTime.Today.AddDays(-7);
            var newMemberCount = db.Members.Count(m => m.RegisterOn >= newMemberSince);

            var vm = new DashboardViewModel
            {
                ProductCount = products.Count,
                OnShelfCount = products.Count(p => p.IsPublished),
                MemberCount = memberCount,
                OrderCount = orders.Count,
                OrderItemCount = orders.Sum(o => o.OrderDetailItems.Count),
                UnshippedCount = orders.Sum(o => o.OrderDetailItems.Count(d => !d.IsShipped)),
                TodayOrderCount = orders.Count(o => o.BuyOn.Date == DateTime.Today),
                NewMemberCount = newMemberCount,
                Turnover = orders.Sum(o => o.TotalPrice),
                AvgOrderPrice = orders.Any() ? orders.Average(o => o.TotalPrice) : 0m,
                StockValue = products.Sum(p => p.Price * p.Amount),
                LowStockProducts = products
                    .Where(p => p.IsPublished && p.Amount <= 20)
                    .OrderBy(p => p.Amount)
                    .Take(6)
                    .ToList(),
                RecentOrders = orders
                    .OrderByDescending(o => o.BuyOn)
                    .Take(6)
                    .ToList(),
                CategoryStats = products
                    .GroupBy(p => p.ProductCategory.Name)
                    .Select(g => new CategoryStat
                    {
                        Name = g.Key,
                        Count = g.Count(),
                        Stock = g.Sum(p => p.Amount)
                    })
                    .OrderByDescending(s => s.Count)
                    .ToList(),
                DayStats = Enumerable.Range(0, 7)
                    .Select(i =>
                    {
                        var day = DateTime.Today.AddDays(-6 + i);
                        var dayOrders = orders.Where(o => o.BuyOn.Date == day).ToList();
                        return new DayStat
                        {
                            Label = day.ToString("MM-dd"),
                            Orders = dayOrders.Count,
                            Amount = dayOrders.Sum(o => o.TotalPrice)
                        };
                    })
                    .ToList()
            };

            return View(vm);
        }

        // 商品管理
        public ActionResult Products(int p = 1, string product = null)
        {
            var query = db.Products.Include(x => x.ProductCategory).AsQueryable();

            if (!string.IsNullOrWhiteSpace(product))
            {
                var keyword = product.Trim();
                query = query.Where(x => x.Name.Contains(keyword)
                                      || x.Made.Contains(keyword)
                                      || x.ProductCategory.Name.Contains(keyword));
            }

            ViewBag.ProductCategories = db.ProductCategories.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList();
            ViewBag.Keyword = product;

            var data = query.OrderByDescending(x => x.Id).ToPagedList(SafePage(p), PageSize);
            return View(data);
        }

        public ActionResult ProductCreate()
        {
            ViewBag.ProductCategories = db.ProductCategories.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList();
            return View(new Product());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductCreate(HttpPostedFileBase image = null)
        {
            ViewBag.ProductCategories = db.ProductCategories.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList();

            string error;
            var product = BuildProductFromForm(out error);
            if (product == null)
            {
                SetToast(error, "error");
                return View(new Product());
            }

            AttachImage(product, image, out error);
            if (!string.IsNullOrEmpty(error))
            {
                SetToast(error, "error");
                return View(new Product());
            }

            if (product.ImageData == null)
            {
                product.ImageData = LXHelper.BuildPlaceholderSvg(product.Name, product.Made, product.Name.Length);
                product.ImageMimeType = "image/svg+xml";
            }

            db.Products.Add(product);
            db.SaveChanges();

            SetToast("商品「" + product.Name + "」已上架", "success");
            return RedirectToAction("Products");
        }

        public ActionResult ProductEdit(int id)
        {
            var product = db.Products
                .Include(p => p.ProductCategory)
                .FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                SetToast("商品不存在", "error");
                return RedirectToAction("Products");
            }

            ViewBag.ProductCategories = db.ProductCategories.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductEdit(int id, HttpPostedFileBase image = null)
        {
            var product = db.Products.Include(p => p.ProductCategory).FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                SetToast("商品不存在", "error");
                return RedirectToAction("Products");
            }

            ViewBag.ProductCategories = db.ProductCategories.OrderBy(c => c.Sort).ThenBy(c => c.Id).ToList();

            string error;
            var parsed = BuildProductFromForm(out error);
            if (parsed == null)
            {
                SetToast(error, "error");
                return View(product);
            }

            product.Name = parsed.Name;
            product.Made = parsed.Made;
            product.Description = parsed.Description;
            product.Price = parsed.Price;
            product.OriginalPrice = parsed.OriginalPrice;
            product.Amount = parsed.Amount;
            product.PublishOn = parsed.PublishOn;
            product.ProductCategory = parsed.ProductCategory;

            AttachImage(product, image, out error);
            if (!string.IsNullOrEmpty(error))
            {
                SetToast(error, "error");
                return View(product);
            }

            db.SaveChanges();
            SetToast("商品「" + product.Name + "」已保存", "success");
            return RedirectToAction("Products");
        }

        // 上架和下架之间来回切
        [HttpPost]
        public ActionResult ProductToggle(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return FailJson("商品不存在");

            product.IsPublished = !product.IsPublished;
            if (product.IsPublished && product.PublishOn == null)
                product.PublishOn = DateTime.Now;

            db.SaveChanges();
            return OkJson(new { isPublished = product.IsPublished },
                product.IsPublished ? "商品已上架" : "商品已下架");
        }

        // 删除商品，被订单引用过的不让删，只能下架
        [HttpPost]
        public ActionResult ProductDelete(int id)
        {
            var product = db.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return FailJson("商品不存在");

            if (db.OrderDetailItems.Any(d => d.Product.Id == id))
                return FailJson("该商品存在历史订单，无法删除，建议改为「下架」");

            var inCarts = db.Carts.Where(c => c.Product.Id == id).ToList();
            foreach (var cart in inCarts) db.Carts.Remove(cart);

            db.Products.Remove(product);
            db.SaveChanges();
            return OkJson(null, "商品已删除");
        }

        // 商品类别管理
        [HttpPost]
        public ActionResult CategoryCreate(string name)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length == 0) return FailJson("请输入类别名称");
            if (name.Length > 20) return FailJson("类别名称不可超过 20 个字");
            if (db.ProductCategories.Any(c => c.Name == name)) return FailJson("该类别已经存在");

            var maxSort = db.ProductCategories.Any() ? db.ProductCategories.Max(c => c.Sort) : 0;
            db.ProductCategories.Add(new ProductCategory { Name = name, Icon = "life", Sort = maxSort + 1 });
            db.SaveChanges();
            return OkJson(null, "类别「" + name + "」已创建");
        }

        [HttpPost]
        public ActionResult CategoryRename(int id, string name)
        {
            name = (name ?? string.Empty).Trim();
            if (name.Length == 0) return FailJson("请输入类别名称");
            if (name.Length > 20) return FailJson("类别名称不可超过 20 个字");

            var category = db.ProductCategories.FirstOrDefault(c => c.Id == id);
            if (category == null) return FailJson("类别不存在");
            if (category.Name == name) return OkJson(null, "名称未变化");
            if (db.ProductCategories.Any(c => c.Name == name && c.Id != id)) return FailJson("该类别名称已被使用");

            category.Name = name;
            db.SaveChanges();
            return OkJson(null, "类别已重命名为「" + name + "」");
        }

        [HttpPost]
        public ActionResult CategoryDelete(int id)
        {
            var category = db.ProductCategories.FirstOrDefault(c => c.Id == id);
            if (category == null) return FailJson("类别不存在");
            if (db.Products.Any(p => p.ProductCategory.Id == id))
                return FailJson("该类别下还有商品，请先移除或转移商品");

            db.ProductCategories.Remove(category);
            db.SaveChanges();
            return OkJson(null, "类别已删除");
        }

        // 用户管理
        public ActionResult Users(int p = 1, string user = null)
        {
            var query = db.Members.AsQueryable();
            if (!string.IsNullOrWhiteSpace(user))
            {
                var keyword = user.Trim();
                query = query.Where(m => m.Email.Contains(keyword)
                                      || m.Name.Contains(keyword)
                                      || m.Nickname.Contains(keyword));
            }

            ViewBag.Keyword = user;
            var data = query.OrderByDescending(m => m.Id).ToPagedList(SafePage(p), PageSize);
            return View(data);
        }

        public ActionResult UserCreate()
        {
            return View(new Member());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserCreate(HttpPostedFileBase image = null)
        {
            var email = (Request.Form["Email"] ?? string.Empty).Trim();
            var password = Request.Form["Password"] ?? string.Empty;
            var name = (Request.Form["Name"] ?? string.Empty).Trim();
            var nickname = (Request.Form["Nickname"] ?? string.Empty).Trim();
            var verified = string.Equals(Request.Form["Verified"], "true", StringComparison.OrdinalIgnoreCase);

            if (email.Length == 0 || !email.Contains("@")) { SetToast("请输入正确的 Email 地址", "error"); return View(new Member()); }
            if (password.Length < 6) { SetToast("密码至少 6 位", "error"); return View(new Member()); }
            if (name.Length == 0 || name.Length > 5) { SetToast("姓名不能为空且不可超过 5 个字", "error"); return View(new Member()); }
            if (nickname.Length == 0 || nickname.Length > 10) { SetToast("用户名不能为空且不可超过 10 个字", "error"); return View(new Member()); }
            if (db.Members.Any(m => m.Email == email)) { SetToast("该 Email 已经有人注册过了", "error"); return View(new Member()); }

            string error;
            var uploaded = LXHelper.ReadImage(image, out error);
            if (!string.IsNullOrEmpty(error)) { SetToast(error, "error"); return View(new Member()); }

            var member = new Member
            {
                Email = email,
                Password = LXHelper.HashPassword(password),
                Name = name,
                Nickname = nickname,
                RegisterOn = DateTime.Now,
                AuthCode = verified ? null : Guid.NewGuid().ToString(),
                ImageData = uploaded == null ? null : uploaded.Data,
                ImageMimeType = uploaded == null ? null : uploaded.MimeType
            };

            db.Members.Add(member);
            db.SaveChanges();

            SetToast("用户「" + nickname + "」创建成功", "success");
            return RedirectToAction("Users");
        }

        public ActionResult UserEdit(int id)
        {
            var member = db.Members.FirstOrDefault(m => m.Id == id);
            if (member == null)
            {
                SetToast("用户不存在", "error");
                return RedirectToAction("Users");
            }
            return View(member);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserEdit(int id, HttpPostedFileBase image = null)
        {
            var member = db.Members.FirstOrDefault(m => m.Id == id);
            if (member == null)
            {
                SetToast("用户不存在", "error");
                return RedirectToAction("Users");
            }

            var name = (Request.Form["Name"] ?? string.Empty).Trim();
            var nickname = (Request.Form["Nickname"] ?? string.Empty).Trim();

            if (name.Length == 0 || name.Length > 5) { SetToast("姓名不能为空且不可超过 5 个字", "error"); return View(member); }
            if (nickname.Length == 0 || nickname.Length > 10) { SetToast("用户名不能为空且不可超过 10 个字", "error"); return View(member); }

            string error;
            var uploaded = LXHelper.ReadImage(image, out error);
            if (!string.IsNullOrEmpty(error)) { SetToast(error, "error"); return View(member); }

            member.Name = name;
            member.Nickname = nickname;
            if (uploaded != null)
            {
                member.ImageData = uploaded.Data;
                member.ImageMimeType = uploaded.MimeType;
            }

            db.SaveChanges();
            SetToast("用户资料已保存", "success");
            return RedirectToAction("Users");
        }

        // 管理员手动帮用户通过邮箱验证
        [HttpPost]
        public ActionResult UserAuth(int id)
        {
            var member = db.Members.FirstOrDefault(m => m.Id == id);
            if (member == null) return FailJson("用户不存在");
            if (member.AuthCode == null) return FailJson("该用户已经通过认证");

            member.AuthCode = null;
            db.SaveChanges();
            return OkJson(null, "用户「" + member.Nickname + "」已通过认证");
        }

        // 把密码重置成 123456
        [HttpPost]
        public ActionResult PwdInitialize(int id)
        {
            var member = db.Members.FirstOrDefault(m => m.Id == id);
            if (member == null) return FailJson("用户不存在");

            member.Password = LXHelper.HashPassword("123456");
            db.SaveChanges();
            return OkJson(null, "用户「" + member.Nickname + "」的密码已重置为 123456");
        }

        // 删除用户。有订单的用户不能删，不然订单数据就对不上了
        [HttpPost]
        public ActionResult UserDelete(int id)
        {
            var member = db.Members.FirstOrDefault(m => m.Id == id);
            if (member == null) return FailJson("用户不存在");

            if (db.Orders.Any(o => o.MemberAddress.Member.Id == id))
                return FailJson("该用户存在历史订单，删除会破坏订单数据，建议保留账号");

            var carts = db.Carts.Where(c => c.Member.Id == id).ToList();
            foreach (var cart in carts) db.Carts.Remove(cart);

            var addresses = db.MemberAddresses.Where(a => a.Member.Id == id).ToList();
            foreach (var address in addresses) db.MemberAddresses.Remove(address);

            db.Members.Remove(member);
            db.SaveChanges();
            return OkJson(null, "用户「" + member.Nickname + "」已删除");
        }

        // 订单管理
        public ActionResult Ordermanagement(int p = 1, int p1 = 1, int p2 = 1, int p3 = 1, string keyword = null)
        {
            var orderQuery = db.Orders
                .Include(o => o.MemberAddress)
                .Include(o => o.OrderDetailItems)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.Trim();
                orderQuery = orderQuery.Where(o => o.OrderNo.Contains(key)
                                                || o.MemberAddress.Member.Nickname.Contains(key)
                                                || o.MemberAddress.ContactName.Contains(key));
            }

            ViewBag.Keyword = keyword;
            ViewData["Orders"] = orderQuery.OrderByDescending(o => o.BuyOn)
                                           .ThenByDescending(o => o.Id)
                                           .ToPagedList(SafePage(p), 8);

            var detailQuery = db.OrderDetailItems
                .Include(d => d.Product)
                .Include(d => d.Product.ProductCategory)
                .Include(d => d.OrderHeader)
                .Include(d => d.OrderHeader.MemberAddress);

            ViewData["Shippedorders"] = detailQuery
                .Where(d => d.Shipmentnumber != null)
                .OrderByDescending(d => d.Id)
                .ToPagedList(SafePage(p1), 8);

            ViewData["Unshippedorders"] = detailQuery
                .Where(d => d.Shipmentnumber == null)
                .OrderByDescending(d => d.Id)
                .ToPagedList(SafePage(p2), 8);

            ViewData["AddShoppingCart"] = db.Carts
                .Include(c => c.Product)
                .Include(c => c.Product.ProductCategory)
                .Include(c => c.Member)
                .OrderByDescending(c => c.Id)
                .ToPagedList(SafePage(p3), 8);

            return View();
        }

        // 看单个订单里的明细
        public ActionResult Orderdetails(int id, int p = 1)
        {
            var order = db.Orders
                .Include(o => o.MemberAddress)
                .FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                SetToast("订单不存在", "error");
                return RedirectToAction("Ordermanagement");
            }

            ViewBag.Order = order;
            var data = db.OrderDetailItems
                .Include(d => d.Product)
                .Include(d => d.Product.ProductCategory)
                .Where(d => d.OrderHeader.Id == id)
                .OrderBy(d => d.Id)
                .ToPagedList(SafePage(p), 8);

            return View(data);
        }

        // 发货，给这条明细生成快递单号
        [HttpPost]
        public ActionResult GoodsShipment(int id)
        {
            var detail = db.OrderDetailItems.Include(d => d.Product).FirstOrDefault(d => d.Id == id);
            if (detail == null) return FailJson("订单明细不存在");
            if (detail.IsShipped) return FailJson("该商品已经发过货了");

            detail.Shipmentnumber = LXHelper.NewShipmentNo();
            db.SaveChanges();
            return OkJson(new { shipmentNo = detail.Shipmentnumber },
                "「" + detail.Product.Name + "」发货成功，单号 " + detail.Shipmentnumber);
        }

        // 删一条订单明细，库存加回来、销量减回去，总额重算
        [HttpPost]
        public ActionResult Deleteorder(int id)
        {
            var detail = db.OrderDetailItems
                .Include(d => d.Product)
                .Include(d => d.OrderHeader)
                .FirstOrDefault(d => d.Id == id);
            if (detail == null) return FailJson("订单明细不存在");
            if (detail.IsShipped) return FailJson("已发货的订单不能取消");

            var productName = detail.Product == null ? "商品" : detail.Product.Name;

            if (detail.Product != null)
            {
                detail.Product.Amount += detail.Amount;
                detail.Product.Sales = Math.Max(0, detail.Product.Sales - detail.Amount);
            }

            var order = detail.OrderHeader;
            order.TotalPrice -= detail.Price;
            db.OrderDetailItems.Remove(detail);

            if (order.TotalPrice <= 0 || db.OrderDetailItems.Count(d => d.OrderHeader.Id == order.Id) <= 1)
            {
                db.Orders.Remove(order);
            }

            db.SaveChanges();
            return OkJson(null, "「" + productName + "」的订单已删除，库存已回滚");
        }

        // 关于这个项目的介绍页
        public ActionResult AboutMe()
        {
            ViewBag.AdminCount = db.Admins.Count();
            ViewBag.MemberCount = db.Members.Count();
            ViewBag.ProductCount = db.Products.Count();
            ViewBag.OrderCount = db.Orders.Count();
            return View();
        }

        // 下面几个是内部用的小方法
        // 从表单里取一个值出来，顺手把两边空格去掉
        private string Form(string key)
        {
            return (Request.Form[key] ?? string.Empty).Trim();
        }

        private Product BuildProductFromForm(out string error)
        {
            error = null;

            var name = Form("Name");
            var made = Form("Made");
            var description = Form("Description");
            var categoryName = Form("ProductCategoryName");
            if (categoryName.Length == 0) categoryName = Form("pp");

            if (name.Length == 0) { error = "请输入商品名称"; return null; }
            if (name.Length > 60) { error = "商品名称不可超过 60 个字"; return null; }
            if (made.Length == 0) { error = "请输入商品制造商"; return null; }
            if (made.Length > 60) { error = "制造商不可超过 60 个字"; return null; }
            if (description.Length == 0) { error = "请输入商品简介"; return null; }
            if (description.Length > 250) { error = "商品简介请勿超过 250 个字"; return null; }

            decimal price;
            if (!decimal.TryParse(Form("Price"), out price) || price <= 0 || price > 1000000)
            {
                error = "请输入合法的商品售价（0.01 ~ 1,000,000）";
                return null;
            }

            decimal? originalPrice = null;
            var originalText = Form("OriginalPrice");
            if (originalText.Length > 0)
            {
                decimal op;
                if (decimal.TryParse(originalText, out op) && op > 0) originalPrice = op;
            }

            int amount;
            if (!int.TryParse(Form("Amount"), out amount) || amount < 0 || amount > 1000000)
            {
                error = "请输入合法的库存数量（0 ~ 1,000,000）";
                return null;
            }

            var category = db.ProductCategories.FirstOrDefault(c => c.Name == categoryName);
            if (category == null)
            {
                error = "请选择有效的商品类别";
                return null;
            }

            DateTime publishOn;
            if (!DateTime.TryParse(Form("PublishOn"), out publishOn))
                publishOn = DateTime.Now;

            return new Product
            {
                Name = name,
                Made = made,
                Description = description,
                Price = price,
                OriginalPrice = originalPrice,
                Amount = amount,
                ProductCategory = category,
                PublishOn = publishOn
            };
        }

        // 把上传的图片塞进商品对象里，不合法就把原因丢给 error
        private void AttachImage(Product product, HttpPostedFileBase image, out string error)
        {
            error = null;
            if (product == null || image == null || image.ContentLength <= 0) return;

            var uploaded = LXHelper.ReadImage(image, out error);
            if (!string.IsNullOrEmpty(error)) return;

            if (uploaded != null)
            {
                product.ImageData = uploaded.Data;
                product.ImageMimeType = uploaded.MimeType;
            }
        }

        private static int SafePage(int p)
        {
            return p < 1 ? 1 : p;
        }
    }
}
