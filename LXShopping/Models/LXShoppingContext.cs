using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace LXShopping.Models
{
    public class LXShoppingContext : DbContext
    {
        public LXShoppingContext() : base("name=DefaultConnection")
        {
        }

        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<OrderHeader> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetailItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<MemberAddress> MemberAddresses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // 去掉 EF 默认把表名变复数的规则，让表名跟类名一样
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // 分类和商品：删分类不跟着删商品，得先移走商品
            modelBuilder.Entity<Product>()
                .HasRequired(p => p.ProductCategory)
                .WithMany(c => c.Products)
                .WillCascadeOnDelete(false);

            // 会员和购物车、商品的关系，都不级联删除
            modelBuilder.Entity<Cart>()
                .HasRequired(c => c.Member)
                .WithMany(m => m.Carts)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Cart>()
                .HasRequired(c => c.Product)
                .WithMany()
                .WillCascadeOnDelete(false);

            // 会员和收货地址
            modelBuilder.Entity<MemberAddress>()
                .HasRequired(a => a.Member)
                .WithMany(m => m.MemberAddresses)
                .WillCascadeOnDelete(false);

            // 订单和明细是绑在一起的，删订单的时候明细跟着一起删掉
            modelBuilder.Entity<OrderDetail>()
                .HasRequired(d => d.OrderHeader)
                .WithMany(o => o.OrderDetailItems)
                .WillCascadeOnDelete(true);

            // 明细和商品：不能级联删，不然以前的订单记录也没了
            modelBuilder.Entity<OrderDetail>()
                .HasRequired(d => d.Product)
                .WithMany()
                .WillCascadeOnDelete(false);

            // 订单和地址同理，不跟着删，不然历史订单查不到地址
            modelBuilder.Entity<OrderHeader>()
                .HasRequired(o => o.MemberAddress)
                .WithMany()
                .WillCascadeOnDelete(false);

            // 金额字段统一设成 18 位、小数点后 2 位
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Product>().Property(p => p.OriginalPrice).HasPrecision(18, 2);
            modelBuilder.Entity<OrderHeader>().Property(o => o.TotalPrice).HasPrecision(18, 2);
            modelBuilder.Entity<OrderDetail>().Property(d => d.Price).HasPrecision(18, 2);
            modelBuilder.Entity<OrderDetail>().Property(d => d.UnitPrice).HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
