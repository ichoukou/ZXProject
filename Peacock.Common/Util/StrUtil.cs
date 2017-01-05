using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.Common.Util
{
    public class StrUtil
    {
        /// <summary>
        /// 获取0-9之间的随机数
        /// 
        /// </summary>
        /// <param name="randNumblenght">要获取的随机数长度</param>
        /// <returns>
        /// String,随机数
        /// </returns>
        public static string GetRandomNumb(int randNumblenght)
        {
            if (randNumblenght < 1)
                throw new ArgumentException("randNumblenght必须为大于0的正整数。");
            string str = "";
            byte[] data = new byte[randNumblenght];
            new RNGCryptoServiceProvider().GetBytes(data);
            for (int index = 0; index < randNumblenght; ++index)
                str = str + (object)(int)((double)Convert.ToInt32(data[index]) / 256.0 * 10.0);
            return str;
        }

    }
}
