using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.ZXEval.Model.DTO
{
    [Serializable]
    public class UserRememberModel
    {
        /// <summary>
        /// 账号
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
       
        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool IsRemember { get; set; }
    }
}
