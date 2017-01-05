using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.ZXEval.Data.Entities
{
    public class CompanyBaseData
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public virtual Company Company { get; set; }

        /// <summary>
        /// 配置类型
        /// </summary>
        public ConfigTypeEnum ConfigType { get; set; }

        /// <summary>
        /// 配置名称
        /// </summary>
        public string ConfigName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
      
    }

    internal class CompanyBaseDataConfig : EntityConfig<CompanyBaseData>
    {
        internal CompanyBaseDataConfig()
            : base(200)
        {
            base.ToTable("CompanyBaseData");
            base.HasKey(x => x.Id);
            base.HasRequired(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
        }
    }

    public enum ConfigTypeEnum
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        BusinessType = 0,

    }
}
