using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using LXShopping.Infrastructure;
using LXShopping.Models;

namespace LXShopping.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<LXShoppingContext>
    {
        public Configuration()
        {
            // 模型改了数据库会自动跟着改，写完直接 F5 就能跑
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            ContextKey = "LXShopping.Models.LXShoppingContext";
        }

        protected override void Seed(LXShoppingContext context)
        {
            SeedAdmins(context);
            SeedMembers(context);
            SeedCategoriesAndProducts(context);
            SeedDemoOrders(context);
        }

        // 管理员账号
        private static void SeedAdmins(LXShoppingContext context)
        {
            // 已经有管理员了就不再插一遍，不然每次启动都多一条
            if (context.Admins.Any()) return;

            context.Admins.Add(new Admin
            {
                AdminName = "admin",
                Pwd = LXHelper.HashPassword("123456"),
                ChinaName = "李鑫",
                Role = "超级管理员"
            });
            context.Admins.Add(new Admin
            {
                AdminName = "lxadmin",
                Pwd = LXHelper.HashPassword("lx123456"),
                ChinaName = "李鑫",
                Role = "运营专员"
            });
            context.SaveChanges();
        }

        // 会员和他们的收货地址
        private static void SeedMembers(LXShoppingContext context)
        {
            if (!context.Members.Any())
            {
                var members = new[]
                {
                    new Member
                    {
                        Email = "demo@lx.com",
                        Password = LXHelper.HashPassword("123456"),
                        Name = "李鑫",
                        Nickname = "LX用户",
                        RegisterOn = DateTime.Now.AddDays(-46),
                        AuthCode = null
                    },
                    new Member
                    {
                        Email = "vip@lx.com",
                        Password = LXHelper.HashPassword("123456"),
                        Name = "张雨",
                        Nickname = "雨落",
                        RegisterOn = DateTime.Now.AddDays(-12),
                        AuthCode = null
                    },
                    new Member
                    {
                        Email = "newbie@lx.com",
                        Password = LXHelper.HashPassword("123456"),
                        Name = "王琪",
                        Nickname = "小琪",
                        RegisterOn = DateTime.Now.AddDays(-2),
                        AuthCode = null
                    }
                };
                context.Members.AddRange(members);
                context.SaveChanges();
            }

            if (!context.MemberAddresses.Any())
            {
                var all = context.Members.ToList();
                context.MemberAddresses.AddRange(new[]
                {
                    new MemberAddress
                    {
                        Member = all.First(m => m.Email == "demo@lx.com"),
                        ContactName = "李鑫",
                        ContactPhoneNo = "18523456789",
                        ContactAddress = "重庆市合川区人民公园 23 栋 502"
                    },
                    new MemberAddress
                    {
                        Member = all.First(m => m.Email == "demo@lx.com"),
                        ContactName = "李鑫（公司）",
                        ContactPhoneNo = "023-62888999",
                        ContactAddress = "重庆市渝北区财富中心 A 座 1666"
                    },
                    new MemberAddress
                    {
                        Member = all.First(m => m.Email == "vip@lx.com"),
                        ContactName = "张雨",
                        ContactPhoneNo = "13800001111",
                        ContactAddress = "四川省成都市武侯区天府大道中段 666 号"
                    }
                });
                context.SaveChanges();
            }
        }

        // 分类和商品
        private static void SeedCategoriesAndProducts(LXShoppingContext context)
        {
            if (!context.ProductCategories.Any())
            {
                var names = new[]
                {
                    new { Name = "数码电子", Icon = "digital" },
                    new { Name = "家用电器", Icon = "appliance" },
                    new { Name = "服饰鞋包", Icon = "fashion" },
                    new { Name = "美妆个护", Icon = "beauty" },
                    new { Name = "食品生鲜", Icon = "food" },
                    new { Name = "家居生活", Icon = "life" },
                    new { Name = "母婴玩具", Icon = "baby" },
                    new { Name = "运动户外", Icon = "sports" }
                };

                var sort = 0;
                foreach (var item in names)
                {
                    context.ProductCategories.Add(new ProductCategory
                    {
                        Name = item.Name,
                        Icon = item.Icon,
                        Sort = sort++
                    });
                }
                context.SaveChanges();
            }

            if (context.Products.Any()) return;

            // 每行顺序：名称, 类别, 制造商, 售价, 原价, 库存, 销量, 简介
            var rows = new[]
            {
                new object[] { "LX Pro 主动降噪无线耳机", "数码电子", "LXAUDIO", 899m, 1299m, 120, 386, "40dB 深度降噪，单次续航 12 小时，支持双设备同时连接。" },
                new object[] { "LX Watch S6 智能运动手表", "数码电子", "LXWATCH", 1299m, 1599m, 80, 214, "1.43 英寸 AMOLED 常亮屏，血氧与心率全天候监测。" },
                new object[] { "65W 氮化镓多口快充充电器", "数码电子", "LXPOWER", 129m, 199m, 500, 912, "2C1A 三口输出，折叠插脚，一台搞定手机与笔记本。" },

                new object[] { "智能变频空气循环扇", "家用电器", "LXHOME", 399m, 599m, 60, 173, "12 档柔风，涡轮增压循环，静音低至 22dB。" },
                new object[] { "迷你桌面静音加湿器", "家用电器", "LXHOME", 89m, 129m, 300, 640, "4L 大容量，上加水设计，缺水自动断电。" },
                new object[] { "便携折叠手持挂烫机", "家用电器", "LXHOME", 259m, 359m, 90, 128, "30 秒速热，双档蒸汽，出差旅行随手放进行李箱。" },

                new object[] { "轻薄三防羽绒外套", "服饰鞋包", "LXFIT", 699m, 999m, 70, 205, "90% 白鸭绒填充，防泼水面料，可收纳进随附收纳袋。" },
                new object[] { "复古老爹运动鞋", "服饰鞋包", "LXFIT", 359m, 499m, 150, 433, "厚底减震，头层牛皮拼接，久走不累脚。" },
                new object[] { "简约通勤双肩背包", "服饰鞋包", "LXFIT", 219m, 299m, 200, 517, "15.6 英寸独立笔记本仓，防泼水尼龙，背部透气减压。" },

                new object[] { "氨基酸温和洁面乳", "美妆个护", "LXBEAUTY", 79m, 119m, 400, 803, "弱酸性配方，洗后不紧绷，敏感肌也能每天使用。" },
                new object[] { "5% 烟酰胺亮肤精华", "美妆个护", "LXBEAUTY", 189m, 269m, 260, 392, "复配依克多因，28 天改善暗沉，肤感清爽好吸收。" },
                new object[] { "声波震动电动牙刷", "美妆个护", "LXBEAUTY", 149m, 229m, 330, 611, "五档模式，两分钟智能计时，一次充电用 60 天。" },

                new object[] { "精选阿拉比卡挂耳咖啡", "食品生鲜", "LXFOOD", 69m, 99m, 600, 1204, "中深烘焙，莓果与坚果香气，一盒 20 袋。" },
                new object[] { "每日坚果混合装 30 包", "食品生鲜", "LXFOOD", 89m, 139m, 480, 977, "七种坚果果干科学配比，独立小包锁鲜。" },
                new object[] { "高山云雾绿茶礼盒", "食品生鲜", "LXFOOD", 158m, 228m, 210, 265, "明前采摘，一芽一叶，礼盒装适合送人。" },

                new object[] { "记忆棉护颈睡眠枕", "家居生活", "LXLIFE", 159m, 239m, 180, 344, "慢回弹记忆棉，中间低两侧高的护颈曲线设计。" },
                new object[] { "北欧简约陶瓷餐具 16 件套", "家居生活", "LXLIFE", 249m, 349m, 120, 156, "高温釉下彩，可进微波炉与洗碗机。" },
                new object[] { "无火香薰精油礼盒", "家居生活", "LXLIFE", 99m, 159m, 340, 489, "三种香型可选，藤条挥发，留香约 60 天。" },

                new object[] { "婴儿有机棉连体衣", "母婴玩具", "LXBABY", 129m, 189m, 220, 308, "A 类婴幼儿标准有机棉，无骨缝制不磨皮肤。" },
                new object[] { "益智磁力积木 108 件", "母婴玩具", "LXBABY", 199m, 279m, 160, 271, "强磁吸附，圆角打磨，附赠搭建图册。" },
                new object[] { "儿童轻量平衡车", "母婴玩具", "LXBABY", 329m, 459m, 85, 132, "航空铝车架，可调座椅，适合 2-5 岁宝宝。" },

                new object[] { "便携折叠露营椅", "运动户外", "LXOUTDOOR", 169m, 249m, 240, 398, "7075 铝合金支架，承重 150kg，收纳仅重 0.9kg。" },
                new object[] { "专业防滑瑜伽垫 8mm", "运动户外", "LXOUTDOOR", 139m, 199m, 300, 452, "TPE 双层结构，正反双面防滑，附绑带。" },
                new object[] { "户外轻量冲锋衣", "运动户外", "LXOUTDOOR", 599m, 899m, 100, 187, "10000mm 防水指数，腋下透气拉链，可收纳进帽兜。" }
            };

            var categories = context.ProductCategories.ToDictionary(c => c.Name);
            var index = 0;
            foreach (var row in rows)
            {
                var name = (string)row[0];
                var category = categories[(string)row[1]];
                var made = (string)row[2];

                context.Products.Add(new Product
                {
                    Name = name,
                    ProductCategory = category,
                    Made = made,
                    Price = (decimal)row[3],
                    OriginalPrice = (decimal)row[4],
                    Amount = (int)row[5],
                    Sales = (int)row[6],
                    Description = (string)row[7],
                    PublishOn = DateTime.Now.AddDays(-(index + 3)),
                    IsPublished = true,
                    ImageData = LXHelper.BuildPlaceholderSvg(name, made, index),
                    ImageMimeType = "image/svg+xml"
                });
                index++;
            }
            context.SaveChanges();
        }

        // 两笔演示订单，一单已经发货了，另一单还没发
        private static void SeedDemoOrders(LXShoppingContext context)
        {
            if (context.Orders.Any()) return;

            var demo = context.Members.FirstOrDefault(m => m.Email == "demo@lx.com");
            if (demo == null) return;

            var address = context.MemberAddresses.FirstOrDefault(a => a.Member.Id == demo.Id);
            if (address == null) return;

            var earphone = context.Products.FirstOrDefault(p => p.Name.Contains("降噪无线耳机"));
            var coffee = context.Products.FirstOrDefault(p => p.Name.Contains("挂耳咖啡"));
            var bag = context.Products.FirstOrDefault(p => p.Name.Contains("双肩背包"));
            if (earphone == null || coffee == null || bag == null) return;

            // 第一单，编了快递单号，所以是已发货
            var order1 = new OrderHeader
            {
                OrderNo = LXHelper.NewOrderNo(),
                MemberAddress = address,
                BuyOn = DateTime.Now.AddDays(-9),
                Memo = "请在工作日送货，谢谢！",
                PayMethod = "在线支付",
                OrderDetailItems = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        Product = earphone,
                        ProductName = earphone.Name,
                        UnitPrice = earphone.Price,
                        Price = earphone.Price,
                        Amount = 1,
                        Shipmentnumber = LXHelper.NewShipmentNo()
                    },
                    new OrderDetail
                    {
                        Product = coffee,
                        ProductName = coffee.Name,
                        UnitPrice = coffee.Price,
                        Price = coffee.Price * 2,
                        Amount = 2,
                        Shipmentnumber = LXHelper.NewShipmentNo()
                    }
                }
            };
            order1.TotalPrice = order1.OrderDetailItems.Sum(d => d.Price);

            // 第二单，还没填快递单号，所以是待发货
            var order2 = new OrderHeader
            {
                OrderNo = LXHelper.NewOrderNo(),
                MemberAddress = address,
                BuyOn = DateTime.Now.AddDays(-2),
                Memo = "货到请电话联系",
                PayMethod = "在线支付",
                OrderDetailItems = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        Product = bag,
                        ProductName = bag.Name,
                        UnitPrice = bag.Price,
                        Price = bag.Price,
                        Amount = 1
                    }
                }
            };
            order2.TotalPrice = order2.OrderDetailItems.Sum(d => d.Price);

            context.Orders.Add(order1);
            context.Orders.Add(order2);
            context.SaveChanges();
        }
    }
}
