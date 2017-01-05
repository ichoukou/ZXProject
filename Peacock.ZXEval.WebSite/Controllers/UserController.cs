using System.Web.Security;
using Newtonsoft.Json;
using Peacock.Common.Helper;
using System.Web.Mvc;
using System;
using Peacock.ZXEval.DataAdapter;
using Peacock.ZXEval.DataAdapter.Interface;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.WebSite.Help;

namespace Peacock.ZXEval.WebSite.Controllers
{

    public class UserController : Controller
    {
        private string rememberCookieName = "RememberUser";
        protected static readonly IUserAdapter UserService = ConditionFactory.Conditions.Resolve<IUserAdapter>();

        public ActionResult Login()
        {
            string rememberCookie = CookieHelper.GetCookie(rememberCookieName);
            if (!string.IsNullOrEmpty(rememberCookie))
            {
                var rememberUser = JsonConvert.DeserializeObject<UserRememberModel>(DESEncrypt.Decrypt(rememberCookie));
                ViewBag.UserName = rememberUser.UserName;
                ViewBag.Password = rememberUser.Password;
                ViewBag.RememberPassword = rememberUser.IsRemember;
            }
            else
            {
                ViewBag.RememberPassword = false;
            }
            return View();

        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="password"></param>
        /// <param name="validateCode"></param>
        /// <param name="remember"></param>
        /// <returns></returns>
        public ActionResult UserLogin(string userAccount, string password, string validateCode, bool remember)
        {
            return ExceptionCatch.Invoke(() =>
            {
                string registerCode = string.Format("{0}", CookieHelper.GetCookie("RegisterCode"));
                if (String.Compare(validateCode, DESEncrypt.Decrypt(registerCode), true) != 0)
                {
                    throw new ApplicationException("验证码有误");
                }
                //记住密码
                var rememberUser = new UserRememberModel{ UserName = userAccount, Password = password, IsRemember = remember };
                if (remember)
                {                   
                    string strValues = DESEncrypt.Encrypt(JsonConvert.SerializeObject(rememberUser));
                    CookieHelper.WriteCookie(rememberCookieName,strValues, DateTime.Now.AddDays(7));
                }
                else
                {
                    CookieHelper.RemoveCookie(rememberCookieName);
                }
                //登录逻辑
                var user = UserService.UserLogin(userAccount, CryptTools.Md5(password));
                UserHelper.SetAuth(user);               
            });
        }

       
        /// <summary>
        /// 退出
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            return ExceptionCatch.Invoke(() =>
            {
                CookieHelper.RemoveCookie(FormsAuthentication.FormsCookieName);
                FormsAuthentication.SignOut(); 
            });
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        /// 密码是否正确
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public JsonResult ChkPassword(string password)
        {
            bool ismatch = false;
            if (!string.IsNullOrWhiteSpace(password))
            {
                var curUserId = UserHelper.GetCurrentUser().Id;
                var user = UserService.GetUser(curUserId);
                ismatch = MyRequest.Md5(password) == user.Password ? true : false;
            }
            return Json(ismatch, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 保存密码
        /// </summary>
        /// <returns></returns>
        public ActionResult SavePassword(string newPassword)
        {
            return ExceptionCatch.Invoke(() =>
            {
                var curUser = UserHelper.GetCurrentUser();
                UserService.ChangePassword(curUser.Id, newPassword);
            });
        }

        /// <summary>
        /// 图片验证码
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ValidateCode(int regType)
        {
            var validateCodeHelper = new ValidationCodeHelper();
            string code = validateCodeHelper.CreateCode(2, 4);
            const string cookieName = "RegisterCode";
            CookieHelper.WriteCookie(cookieName, DESEncrypt.Encrypt(code));
            CookieHelper.SetCookieExpires(cookieName, DateTime.Now.AddMinutes(30));
            byte[] bytes = validateCodeHelper.CreateImage(code);
            return File(bytes, @"image/jpeg");
        }
    }
}
