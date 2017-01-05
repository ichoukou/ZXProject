using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Peacock.Common.Helper
{
    public static class MyRequest
    {
        /// <summary>
        /// 密码，取密文
        /// </summary>
        /// <param name="psw">密码原文</param>
        /// <returns>加密后的字符串</returns>
        public static string GetJiaMiString(string psw)
        {
            string text = "pep" + psw;
            string result = GetMd5Hash(text);
            return result.Substring(5) + result.Substring(0, 5);
        }

        /// <summary>
        /// 根据记录ID和密码，取密文
        /// </summary>
        /// <param name="rid">记录ID</param>
        /// <param name="psw">密码原文</param>
        /// <returns>加密后的字符串</returns>
        public static string GetJiaMiString(string rid, string psw)
        {
            string text = rid + "pep" + psw;
            string result = GetMd5Hash(text);
            return result.Substring(5) + result.Substring(0, 5);
        }

        /// <summary>
        /// 获取MD5加密后的密文字符串
        /// </summary>
        /// <param name="input">要加密的字符串，比如密码</param>
        /// <returns>返回加密后的密文字符串</returns>
        public static string GetMd5Hash(string input)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string Md5(string source)
        {
            // ReSharper disable PossibleNullReferenceException
            return System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(source, "MD5").ToLower();
            // ReSharper restore PossibleNullReferenceException
        }


    }
}
