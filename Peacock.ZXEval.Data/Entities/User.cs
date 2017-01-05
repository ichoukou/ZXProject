using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.ZXEval.Data.Entities
{
    public class User
    {

        /// <summary>
        /// 用户ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }
       
        /// <summary>
        /// 是否管理员 
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 所属公司ID
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public virtual Company Company { get; set; }
       
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsEnabled { get; set; }


        /// <summary>
        /// 数字签名ID
        /// </summary>
        public string UserKeyId { get; set; }

        /// <summary>
        /// 数字签名密钥
        /// </summary>
        public string UserAccessKey { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string PhoneNumber { get; set; }

    }

    internal class UserConfig : EntityConfig<User>
    {
        internal UserConfig()
            : base(50)
        {
            base.ToTable("User");
            base.HasKey(x => x.Id);
            HasRequired(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
        }
    }
}
