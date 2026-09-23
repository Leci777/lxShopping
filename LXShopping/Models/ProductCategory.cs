using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace LXShopping.Models
{
    // 商品分类
    [DisplayName("商品类别")]
    [DisplayColumn("Name")]
    public class ProductCategory
    {
        [Key]
        public int Id { get; set; }

        [DisplayName("类别名称")]
        [Required(ErrorMessage = "请输入商品类别名称")]
        [MaxLength(20, ErrorMessage = "类别名称不可超过 20 个字")]
        public string Name { get; set; }

        [DisplayName("类别图标")]
        [MaxLength(20)]
        [Description("用于前台展示的图标标识，例如 digital、food、life")]
        public string Icon { get; set; }

        [DisplayName("排序")]
        public int Sort { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        // 分类下已上架的商品数，首页会显示
        public int OnShelfCount
        {
            get { return Products == null ? 0 : Products.Count(p => p.IsPublished); }
        }
    }
}
