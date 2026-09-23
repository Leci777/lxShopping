using System;
using System.Data.Entity;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using LXShopping.Migrations;
using LXShopping.Models;

namespace LXShopping
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // 数据库跟着模型自动升级，不用自己写建表语句
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<LXShoppingContext, Configuration>());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // 网站启动时先建库、写种子数据，不然第一次访问会超时
            try
            {
                using (var db = new LXShoppingContext())
                {
                    db.Database.Initialize(false);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("数据库初始化失败：" + ex);
            }
        }
    }
}
