using System;
using System.Web;
using System.Web.Mvc;

namespace LXShopping.Infrastructure
{
    // 图标都放这里，直接把 svg 代码拼成字符串，颜色随文字变
    public static class LXIcon
    {
        private const string S = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" width=\"{0}\" height=\"{0}\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.8\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\">{1}</svg>";

        private static IHtmlString Build(int size, string body)
        {
            return new HtmlString(string.Format(S, size, body));
        }

        public static IHtmlString Cart(int size = 20) { return Build(size, "<circle cx=\"9\" cy=\"20\" r=\"1.4\"/><circle cx=\"18\" cy=\"20\" r=\"1.4\"/><path d=\"M2 3h3l2.4 11.2a2 2 0 0 0 2 1.6h8.2a2 2 0 0 0 2-1.6L21 7H6\"/>"); }
        public static IHtmlString Search(int size = 20) { return Build(size, "<circle cx=\"11\" cy=\"11\" r=\"7\"/><path d=\"M20 20l-3.6-3.6\"/>"); }
        public static IHtmlString User(int size = 20) { return Build(size, "<circle cx=\"12\" cy=\"8\" r=\"4\"/><path d=\"M4 21a8 8 0 0 1 16 0\"/>"); }
        public static IHtmlString Menu(int size = 22) { return Build(size, "<path d=\"M3 6h18M3 12h18M3 18h18\"/>"); }
        public static IHtmlString Close(int size = 20) { return Build(size, "<path d=\"M6 6l12 12M18 6L6 18\"/>"); }
        public static IHtmlString Home(int size = 20) { return Build(size, "<path d=\"M3 10.5 12 3l9 7.5\"/><path d=\"M5 9.5V21h14V9.5\"/>"); }
        public static IHtmlString Grid(int size = 20) { return Build(size, "<rect x=\"3\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.6\"/><rect x=\"14\" y=\"3\" width=\"7\" height=\"7\" rx=\"1.6\"/><rect x=\"3\" y=\"14\" width=\"7\" height=\"7\" rx=\"1.6\"/><rect x=\"14\" y=\"14\" width=\"7\" height=\"7\" rx=\"1.6\"/>"); }
        public static IHtmlString Chart(int size = 20) { return Build(size, "<path d=\"M3 21h18\"/><rect x=\"5\" y=\"11\" width=\"3.4\" height=\"7\" rx=\"1\"/><rect x=\"10.3\" y=\"6\" width=\"3.4\" height=\"12\" rx=\"1\"/><rect x=\"15.6\" y=\"14\" width=\"3.4\" height=\"4\" rx=\"1\"/>"); }
        public static IHtmlString Box(int size = 20) { return Build(size, "<path d=\"M12 2.6 21 7v10l-9 4.4L3 17V7z\"/><path d=\"M12 12.4 21 7M12 12.4 3 7M12 12.4v9\"/>"); }
        public static IHtmlString Users(int size = 20) { return Build(size, "<circle cx=\"9\" cy=\"8\" r=\"3.4\"/><path d=\"M2.5 20a6.6 6.6 0 0 1 13 0\"/><path d=\"M16.5 5.2a3.4 3.4 0 0 1 0 6.6M18 20a6.6 6.6 0 0 0-2-4.7\"/>"); }
        public static IHtmlString Orders(int size = 20) { return Build(size, "<path d=\"M7 3h10a2 2 0 0 1 2 2v16l-7-3.4L5 21V5a2 2 0 0 1 2-2z\"/>"); }
        public static IHtmlString Truck(int size = 20) { return Build(size, "<path d=\"M3 6.5h10.5v9.5H3z\"/><path d=\"M13.5 9.5H18l3 3.2v3.3h-7.5z\"/><circle cx=\"7\" cy=\"18.5\" r=\"1.7\"/><circle cx=\"17\" cy=\"18.5\" r=\"1.7\"/>"); }
        public static IHtmlString Tag(int size = 20) { return Build(size, "<path d=\"M20.6 12.6 12.6 20.6a1.4 1.4 0 0 1-2 0l-7.2-7.2a1.4 1.4 0 0 1-.4-1V4.4A1.4 1.4 0 0 1 4.4 3h8a1.4 1.4 0 0 1 1 .4l7.2 7.2a1.4 1.4 0 0 1 0 2z\"/><circle cx=\"8.4\" cy=\"8.4\" r=\"1.4\"/>"); }
        public static IHtmlString Check(int size = 20) { return Build(size, "<path d=\"M4 12.6 9.2 18 20 6.4\"/>"); }
        public static IHtmlString Shield(int size = 20) { return Build(size, "<path d=\"M12 3l7.5 3v6c0 4.5-3.2 7.7-7.5 9-4.3-1.3-7.5-4.5-7.5-9V6z\"/><path d=\"M9 12.2 11.2 14.4 15.4 10\"/>"); }
        public static IHtmlString Info(int size = 20) { return Build(size, "<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 11v6\"/><circle cx=\"12\" cy=\"7.6\" r=\"1\"/>"); }
        public static IHtmlString Trash(int size = 18) { return Build(size, "<path d=\"M4 7h16\"/><path d=\"M9 7V4.6h6V7\"/><path d=\"M6.5 7l1 13h9l1-13\"/><path d=\"M10.5 11v5M13.5 11v5\"/>"); }
        public static IHtmlString Edit(int size = 18) { return Build(size, "<path d=\"M4 20h4l10-10-4-4L4 16z\"/><path d=\"M14 6l4 4\"/>"); }
        public static IHtmlString Eye(int size = 18) { return Build(size, "<path d=\"M2 12s3.6-6.5 10-6.5S22 12 22 12s-3.6 6.5-10 6.5S2 12 2 12z\"/><circle cx=\"12\" cy=\"12\" r=\"2.6\"/>"); }
        public static IHtmlString Plus(int size = 18) { return Build(size, "<path d=\"M12 5v14M5 12h14\"/>"); }
        public static IHtmlString Minus(int size = 18) { return Build(size, "<path d=\"M5 12h14\"/>"); }
        public static IHtmlString ArrowRight(int size = 18) { return Build(size, "<path d=\"M5 12h13M12.5 5.5 19 12l-6.5 6.5\"/>"); }
        public static IHtmlString Refresh(int size = 18) { return Build(size, "<path d=\"M20 12a8 8 0 1 1-2.6-5.9\"/><path d=\"M20 4v5h-5\"/>"); }
        public static IHtmlString Logout(int size = 18) { return Build(size, "<path d=\"M15 4h3.5A1.5 1.5 0 0 1 20 5.5v13a1.5 1.5 0 0 1-1.5 1.5H15\"/><path d=\"M10 8 6 12l4 4\"/><path d=\"M6 12h9\"/>"); }
        public static IHtmlString Phone(int size = 18) { return Build(size, "<path d=\"M5 3.5h3.4l1.4 4-2 1.4a11 11 0 0 0 5.3 5.3l1.4-2 4 1.4V19a1.6 1.6 0 0 1-1.8 1.6A16.5 16.5 0 0 1 3.4 5.3 1.6 1.6 0 0 1 5 3.5z\"/>"); }
        public static IHtmlString Location(int size = 18) { return Build(size, "<path d=\"M12 21s7-5.4 7-11a7 7 0 1 0-14 0c0 5.6 7 11 7 11z\"/><circle cx=\"12\" cy=\"10\" r=\"2.6\"/>"); }
        public static IHtmlString Spark(int size = 18) { return Build(size, "<path d=\"M12 3l1.9 5.1L19 10l-5.1 1.9L12 17l-1.9-5.1L5 10l5.1-1.9z\"/>"); }
        public static IHtmlString Wallet(int size = 20) { return Build(size, "<rect x=\"3\" y=\"6\" width=\"18\" height=\"13\" rx=\"2.4\"/><path d=\"M3 10h18\"/><circle cx=\"17\" cy=\"14.5\" r=\"1.2\"/>"); }
        public static IHtmlString Star(int size = 16) { return Build(size, "<path d=\"M12 3.4l2.7 5.6 6.1.9-4.4 4.3 1 6.1-5.4-2.9-5.4 2.9 1-6.1L3.2 9.9l6.1-.9z\"/>"); }

        // 按名字返回分类图标，对不上就给个方格
        public static IHtmlString Category(string key, int size = 24)
        {
            switch ((key ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "digital": return Spark(size);
                case "appliance": return Home(size);
                case "fashion": return Tag(size);
                case "beauty": return Star(size);
                case "food": return Wallet(size);
                case "life": return Box(size);
                case "baby": return Users(size);
                case "sports": return Shield(size);
                default: return Grid(size);
            }
        }
    }
}
