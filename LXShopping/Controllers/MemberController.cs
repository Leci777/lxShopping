using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using LXShopping.Infrastructure;
using LXShopping.Models;

namespace LXShopping.Controllers
{
    public class MemberController : BaseController
    {
        // 下面几个是注册相关的东西
        public ActionResult Register()
        {
            if (IsMemberLoggedIn) return RedirectToAction("Index", "Home");
            return View(new Member());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "Email,Password,Name,Nickname")] Member member)
        {
            var password = member.Password ?? string.Empty;
            var confirm = (Request.Form["ConfirmPassword"] ?? string.Empty);

            if (password.Length < 6 || password.Length > 20)
                ModelState.AddModelError("Password", "密码长度需在 6 ~ 20 位之间");
            if (!string.Equals(password, confirm, StringComparison.Ordinal))
                ModelState.AddModelError("ConfirmPassword", "两次输入的密码不一致");
            if (db.Members.Any(m => m.Email == member.Email))
                ModelState.AddModelError("Email", "您输入的 Email 已经有人注册过了！");

            if (!ModelState.IsValid) return View(member);

            member.Email = member.Email.Trim();
            member.Password = LXHelper.HashPassword(password);
            member.RegisterOn = DateTime.Now;
            member.AuthCode = LXHelper.RegisterMailEnabled ? Guid.NewGuid().ToString() : null;

            db.Members.Add(member);
            db.SaveChanges();

            if (member.AuthCode != null)
            {
                var sent = TrySendAuthCode(member);
                SetToast(sent
                    ? "注册成功！验证邮件已发送到 " + member.Email + "，请查收后完成验证"
                    : "注册成功！但验证邮件发送失败，请联系管理员为您开通账号", sent ? "success" : "warning");
            }
            else
            {
                SetToast("注册成功，现在就可以登录啦！", "success");
            }

            return RedirectToAction("Login");
        }

        // 邮件里的验证链接，清掉验证码就算通过
        public ActionResult ValidateRegister(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                SetToast("验证链接无效", "error");
                return RedirectToAction("Login");
            }

            var member = db.Members.FirstOrDefault(m => m.AuthCode == id);
            if (member != null)
            {
                member.AuthCode = null;
                db.SaveChanges();
                SetToast("会员验证成功，现在可以登录网站了！", "success");
            }
            else
            {
                SetToast("查无此验证码，可能已经验证过了", "info");
            }

            return RedirectToAction("Login");
        }

        // 下面几个是登录相关的东西
        public ActionResult Login(string returnUrl)
        {
            if (IsMemberLoggedIn)
                return RedirectToLocal(returnUrl);

            return View(new MemberLoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, string returnUrl, bool rememberMe = false)
        {
            email = (email ?? string.Empty).Trim();

            var member = ValidateUser(email, password);
            if (member == null)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(new MemberLoginViewModel { email = email, ReturnUrl = returnUrl });
            }

            FormsAuthentication.SetAuthCookie(member.Email, rememberMe);
            Session["memberid"] = member.Id;
            Session["name"] = string.IsNullOrWhiteSpace(member.Nickname) ? member.Name : member.Nickname;

            SetToast("欢迎回来，" + Session["name"] + "！", "success");
            return RedirectToLocal(returnUrl);
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            SetToast("您已安全退出", "info");
            return RedirectToAction("Index", "Home");
        }

        // 修改密码，在个人中心的安全设置里用
        [HttpPost]
        [MemberAuthorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel form)
        {
            var back = RedirectToAction("Index", "AboutMember", new { tab = "security" });

            var member = CurrentMember;
            if (member == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                var first = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault(m => !string.IsNullOrEmpty(m));
                SetToast(first ?? "请检查填写的内容", "error");
                return back;
            }

            if (!LXHelper.VerifyPassword(form.OldPassword, member.Password))
            {
                SetToast("当前密码不正确", "error");
                return back;
            }

            if (string.Equals(form.OldPassword, form.NewPassword, StringComparison.Ordinal))
            {
                SetToast("新密码不能与当前密码相同", "error");
                return back;
            }

            member.Password = LXHelper.HashPassword(form.NewPassword);
            db.SaveChanges();

            SetToast("密码修改成功，下次请使用新密码登录", "success");
            return back;
        }

        // 其它零碎的东西
        // 检查邮箱是不是已经被注册过了
        [HttpPost]
        public ActionResult CheckDup(string Email)
        {
            if (string.IsNullOrWhiteSpace(Email)) return Json(false);
            var exists = db.Members.Any(m => m.Email == Email.Trim());
            return Json(!exists);
        }

        // 会员头像。没传头像的人就返回一张文字头像顶着
        public ActionResult GetImage(int id)
        {
            var member = db.Members.FirstOrDefault(m => m.Id == id);
            if (member != null && member.ImageData != null && member.ImageData.Length > 0)
            {
                var mime = string.IsNullOrEmpty(member.ImageMimeType) ? "image/png" : member.ImageMimeType;
                return File(member.ImageData, mime);
            }

            var text = member == null ? "LX" : (string.IsNullOrWhiteSpace(member.Nickname) ? member.Name : member.Nickname);
            return File(LXHelper.BuildPlaceholderSvg(text, "LX 会员", id + 3), "image/svg+xml");
        }

        // 下面几个是内部用的小方法
        private Member ValidateUser(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "请输入账号与密码");
                return null;
            }

            var hash = LXHelper.HashPassword(password);
            var member = db.Members.FirstOrDefault(m => m.Email == email && m.Password == hash);

            if (member == null)
            {
                ModelState.AddModelError("", "您输入的账号或密码错误");
                return null;
            }

            if (member.AuthCode != null)
            {
                ModelState.AddModelError("", "您的账号尚未通过邮箱验证，请先查收注册邮件并点击验证链接");
                return null;
            }

            return member;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        private bool TrySendAuthCode(Member member)
        {
            try
            {
                var templatePath = Server.MapPath("~/App_Data/MemberRegisterEMailTemplate.htm");
                var mailBody = System.IO.File.Exists(templatePath)
                    ? System.IO.File.ReadAllText(templatePath)
                    : "亲爱的 {{Name}} 您好，请点击以下链接完成注册验证：{{AUTH_URL}}";

                mailBody = mailBody.Replace("{{Name}}", member.Name)
                                   .Replace("{{RegisterOn}}", member.RegisterOn.ToString("F"))
                                   .Replace("{{SiteName}}", LXHelper.SiteName);

                var authUrl = new UriBuilder(Request.Url)
                {
                    Path = Url.Action("ValidateRegister", "Member", new { id = member.AuthCode }),
                    Query = string.Empty
                }.ToString();

                mailBody = mailBody.Replace("{{AUTH_URL}}", authUrl);

                using (var msg = new MailMessage())
                {
                    msg.Subject = "【" + LXHelper.SiteName + "】会员注册确认信";
                    msg.Body = mailBody;
                    msg.IsBodyHtml = true;
                    msg.From = new MailAddress(LXHelper.App("SmtpUser"), LXHelper.SiteName);
                    msg.To.Add(member.Email);

                    using (var client = new SmtpClient(LXHelper.App("SmtpHost"), int.Parse(LXHelper.App("SmtpPort", "25"))))
                    {
                        client.UseDefaultCredentials = false;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.Credentials = new NetworkCredential(LXHelper.App("SmtpUser"), LXHelper.App("SmtpPassword"));
                        client.Send(msg);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("发送注册邮件失败：" + ex.Message);
                return false;
            }
        }
    }
}
