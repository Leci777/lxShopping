using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace LXShopping.Infrastructure
{
    // 一些到处都要用的公共方法
    public static class LXHelper
    {
        // 加密密码时在密码后面拼的一串固定文字
        private const string PasswordSalt = "Lx#Shopping$2026@Salt*Key!";

        // 存 DbContext 用的 key
        public const string DbContextKey = "__LX_DBCONTEXT__";

        // 把密码加密成 40 位字符串，库里不存原密码
        public static string HashPassword(string rawPassword)
        {
            if (rawPassword == null) rawPassword = string.Empty;
            return FormsAuthentication.HashPasswordForStoringInConfigFile(PasswordSalt + rawPassword, "SHA1");
        }

        // 登录时用，把输入的密码加密后跟库里的比
        public static bool VerifyPassword(string rawPassword, string hashed)
        {
            if (string.IsNullOrEmpty(hashed)) return false;
            return string.Equals(HashPassword(rawPassword), hashed, StringComparison.OrdinalIgnoreCase);
        }

        // 读 Web.config 里的配置项，没配就用默认值
        public static string App(string key, string defaultValue = "")
        {
            var value = System.Configuration.ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        // 注册要不要发邮件验证
        public static bool RegisterMailEnabled
        {
            get { return string.Equals(App("EnableRegisterMail", "false"), "true", StringComparison.OrdinalIgnoreCase); }
        }

        // 网站的名字，Web.config 里配的
        public static string SiteName
        {
            get { return App("SiteName", "LX 优选商城"); }
        }

        // 把上传上来的图片读成一串字节存进数据库，不能超过 4MB
        public static UploadedImage ReadImage(HttpPostedFileBase file, out string error)
        {
            error = null;
            if (file == null || file.ContentLength <= 0) return null;

            if (file.ContentLength > 4 * 1024 * 1024)
            {
                error = "图片大小不可超过 4MB";
                return null;
            }

            var allowed = new[]
            {
                "image/jpeg", "image/jpg", "image/png", "image/gif",
                "image/bmp", "image/webp", "image/svg+xml"
            };

            var contentType = (file.ContentType ?? string.Empty).ToLowerInvariant();
            if (!allowed.Contains(contentType))
            {
                error = "仅支持 JPG / PNG / GIF / BMP / WEBP / SVG 格式的图片";
                return null;
            }

            var data = new byte[file.ContentLength];
            file.InputStream.Position = 0;
            var read = file.InputStream.Read(data, 0, file.ContentLength);
            if (read <= 0)
            {
                error = "读取图片失败，请重新上传";
                return null;
            }

            return new UploadedImage { Data = data, MimeType = contentType };
        }

        // 把金额格式化成 ¥1,234.00
        public static string Money(decimal value)
        {
            return "¥" + value.ToString("N2", CultureInfo.GetCultureInfo("zh-CN"));
        }

        // 生成订单号：LX + 时间 + 4 位随机数
        public static string NewOrderNo()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            return "LX" + DateTime.Now.ToString("yyyyMMddHHmmss") + rnd.Next(1000, 9999);
        }

        // 生成快递单号，随便拼 14 位字母数字
        public static string NewShipmentNo()
        {
            const string chars = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            var sb = new StringBuilder("LX");
            for (var i = 0; i < 14; i++) sb.Append(chars[rnd.Next(chars.Length)]);
            return sb.ToString();
        }

        // 商品没图就画一张 svg 顶上，免得页面出现破图
        public static byte[] BuildPlaceholderSvg(string text, string subText, int seed)
        {
            var palettes = new[]
            {
                new[] { "#5B4BFF", "#12C2B4" },
                new[] { "#FF6B6B", "#FFB84D" },
                new[] { "#2E7CF6", "#5B4BFF" },
                new[] { "#11998E", "#38EF7D" },
                new[] { "#F76B8A", "#F8B195" },
                new[] { "#6A5ACD", "#FF7AB6" },
                new[] { "#0F766E", "#22D3EE" },
                new[] { "#B45309", "#FBBF24" }
            };
            var p = palettes[Math.Abs(seed) % palettes.Length];
            var title = (text ?? string.Empty).Trim();
            var initial = string.IsNullOrEmpty(title) ? "LX" : title.Substring(0, 1);
            var word = HttpUtility.HtmlEncode(title.Length > 10 ? title.Substring(0, 10) + "…" : title);
            var sub = HttpUtility.HtmlEncode(subText ?? string.Empty);

            var sb = new StringBuilder();
            sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"600\" height=\"600\" viewBox=\"0 0 600 600\">");
            sb.Append("<defs>");
            sb.Append("<linearGradient id=\"g\" x1=\"0\" y1=\"0\" x2=\"1\" y2=\"1\">");
            sb.Append("<stop offset=\"0\" stop-color=\"").Append(p[0]).Append("\"/>");
            sb.Append("<stop offset=\"1\" stop-color=\"").Append(p[1]).Append("\"/>");
            sb.Append("</linearGradient>");
            sb.Append("<radialGradient id=\"h\" cx=\"0.5\" cy=\"0.42\" r=\"0.55\">");
            sb.Append("<stop offset=\"0\" stop-color=\"#ffffff\" stop-opacity=\"0.28\"/>");
            sb.Append("<stop offset=\"1\" stop-color=\"#ffffff\" stop-opacity=\"0\"/>");
            sb.Append("</radialGradient>");
            sb.Append("</defs>");
            sb.Append("<rect width=\"600\" height=\"600\" fill=\"url(#g)\"/>");
            sb.Append("<rect width=\"600\" height=\"600\" fill=\"url(#h)\"/>");
            sb.Append("<circle cx=\"480\" cy=\"120\" r=\"150\" fill=\"#ffffff\" fill-opacity=\"0.10\"/>");
            sb.Append("<circle cx=\"110\" cy=\"510\" r=\"190\" fill=\"#000000\" fill-opacity=\"0.08\"/>");
            sb.Append("<text x=\"300\" y=\"290\" text-anchor=\"middle\" font-size=\"190\" font-family=\"Segoe UI, Microsoft YaHei, sans-serif\" font-weight=\"700\" fill=\"#ffffff\" fill-opacity=\"0.95\">")
              .Append(HttpUtility.HtmlEncode(initial)).Append("</text>");
            sb.Append("<text x=\"300\" y=\"380\" text-anchor=\"middle\" font-size=\"38\" font-family=\"Microsoft YaHei, Segoe UI, sans-serif\" fill=\"#ffffff\" fill-opacity=\"0.92\">")
              .Append(word).Append("</text>");
            sb.Append("<text x=\"300\" y=\"430\" text-anchor=\"middle\" font-size=\"24\" font-family=\"Microsoft YaHei, Segoe UI, sans-serif\" fill=\"#ffffff\" fill-opacity=\"0.72\">")
              .Append(sub).Append("</text>");
            sb.Append("<text x=\"300\" y=\"545\" text-anchor=\"middle\" font-size=\"26\" letter-spacing=\"8\" font-family=\"Segoe UI, sans-serif\" fill=\"#ffffff\" fill-opacity=\"0.85\">LX</text>");
            sb.Append("</svg>");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        // 把图片字节拼成 data:image/... 字符串
        public static string ToDataUri(byte[] data, string mimeType)
        {
            if (data == null || data.Length == 0) return null;
            var mime = string.IsNullOrEmpty(mimeType) ? "image/png" : mimeType;
            return "data:" + mime + ";base64," + Convert.ToBase64String(data);
        }

        // 文字太长了就截掉，后面加个省略号
        public static string Ellipsis(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "…";
        }
    }

    // 上传图片读出来的结果
    public class UploadedImage
    {
        public byte[] Data { get; set; }
        public string MimeType { get; set; }
    }
}
