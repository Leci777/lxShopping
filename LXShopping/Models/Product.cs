using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LXShopping.Models
{
    // 商品，对应数据库里的 Product 表
    [DisplayName("商品")]
    [DisplayColumn("Name")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("商品类别")]
        [Required(ErrorMessage = "请选择商品类别")]
        public virtual ProductCategory ProductCategory { get; set; }

        [DisplayName("商品名称")]
        [Required(ErrorMessage = "请输入商品名称")]
        [MaxLength(60, ErrorMessage = "商品名称不可超过 60 个字")]
        public string Name { get; set; }

        [DisplayName("商品图片")]
        public byte[] ImageData { get; set; }

        [DisplayName("图片类型")]
        public string ImageMimeType { get; set; }

        [DisplayName("库存数量")]
        [Required(ErrorMessage = "请输入商品库存数量")]
        [Range(0, 1000000, ErrorMessage = "库存数量必须介于 0 ~ 1,000,000 之间")]
        public int Amount { get; set; }

        [DisplayName("商品简介")]
        [Required(ErrorMessage = "请输入商品简介")]
        [MaxLength(250, ErrorMessage = "商品简介请勿输入超过 250 个字")]
        public string Description { get; set; }

        [DisplayName("制造商")]
        [Required(ErrorMessage = "请输入商品制造商")]
        [MaxLength(60, ErrorMessage = "制造商不可超过 60 个字")]
        public string Made { get; set; }

        [DisplayName("商品售价")]
        [Required(ErrorMessage = "请输入商品售价")]
        [Range(0.01, 1000000, ErrorMessage = "商品售价必须介于 0.01 ~ 1,000,000 之间")]
        public decimal Price { get; set; }

        [DisplayName("上架时间")]
        public DateTime? PublishOn { get; set; }

        [DisplayName("累计销量")]
        public int Sales { get; set; }

        [DisplayName("是否上架")]
        public bool IsPublished { get; set; }

        public Product()
        {
            IsPublished = true;
            PublishOn = DateTime.Now;
        }

        // 库存为 0 就是卖完了，前台显示已售罄、加购变灰
        public bool IsSoldOut
        {
            get { return Amount <= 0; }
        }

        // 原价，不填也行；比售价高时前台打「立减」标签
        [DisplayName("原价")]
        public decimal? OriginalPrice { get; set; }
    }
}
