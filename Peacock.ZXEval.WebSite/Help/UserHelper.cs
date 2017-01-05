using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Peacock.Common.Helper;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.WebSite.Help
{
    public  class UserHelper
    {
        /// <summary>
        /// 赋权
        /// </summary>
        /// <param name="user"></param>
        public static void SetAuth(UserModel user)
        {
            //直接把用户信息存入Cookie
            IsoDateTimeConverter timeFormat = new IsoDateTimeConverter();
            timeFormat.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string strValues = JsonConvert.SerializeObject(user, Newtonsoft.Json.Formatting.Indented, timeFormat);
            strValues = strValues.Replace("\r\n", "");
            //创建ticket
            var ticket = new FormsAuthenticationTicket(2, user.UserName, DateTime.Now, DateTime.Now.AddDays(FormsAuthentication.Timeout.Days), true, strValues);
            //加密ticket
            var cookieValue = FormsAuthentication.Encrypt(ticket);

            CookieHelper.WriteCookie(FormsAuthentication.FormsCookieName, cookieValue);
        }


        /// <summary>
        /// 获取当前用户
        /// </summary>
        /// <returns></returns>
        public static UserModel GetCurrentUser()
        {
            UserModel user = null;
            string strCookie = CookieHelper.GetCookie(FormsAuthentication.FormsCookieName);
            if (!string.IsNullOrWhiteSpace(strCookie))
            {
                user = JsonConvert.DeserializeObject<UserModel>(FormsAuthentication.Decrypt(strCookie).UserData);
            }
            return user;
        }
    }
}