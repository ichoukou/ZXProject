using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.ZXEval.Data.Entities
{
    public class Company
    {
        /// <summary>
        /// 公司ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否可用
        /// </summary>
        public bool IsEnabled { get; set; }
    }

    internal class CompanyConfig : EntityConfig<Company>
    {
        internal CompanyConfig()
            : base(200)
        {
            base.ToTable("Company");
            base.HasKey(x => x.Id);          
        }
    }
}
