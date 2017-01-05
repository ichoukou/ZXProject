using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Peacock.Common.Helper
{
    public class RegexHelper
    {
        /// <summary>
        /// 是否存在字母
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool isExistsLetter(string str)
        {
            return Regex.Matches(str, "[a-zA-Z]").Count > 0;
        }

    }
}
