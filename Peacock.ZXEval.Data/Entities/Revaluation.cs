using System;
using System.Collections.Generic;

namespace Peacock.ZXEval.Data.Entities
{
    /// <summary>
    /// 复估表
    /// </summary>
    public class Revaluation
    {
        /// <summary>
        /// TId
        /// </summary>
        public long TId { get; set; }

        /// <summary>
        /// 业务ID
        /// </summary>
        public string BusinessId { get; set; }

        /// <summary>
        /// 复估单号
        /// </summary>
        public string RevaluationNo { get; set; }

        /// <summary>
        /// 所属公司ID
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public virtual Company Company { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string EvalType { get; set; }

        /// <summary>
        /// 复估名称
        /// </summary>
        public string RevaluationName { get; set; }

        /// <summary>
        /// 复估状态
        /// </summary>
        public RevaluationStatusEnum RevaluationStatus { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 与复估项目的对应关系
        /// </summary>
        public virtual ICollection<RevaluationItem> RevaluationItems { get; set; }

        //公司是否已扫描
        public bool? IsScan { get; set; }

        //公司扫描时间 
        public DateTime? ScanTime { get; set; }
    }

    public enum RevaluationStatusEnum
    {
        未接收完 = 0,
        未受理 = 1,
        复估受理 = 2,
        复估完成 = 3,
        已撤单 = 4
    }

    internal class RevaluationConfig : EntityConfig<Revaluation>
    {
        internal RevaluationConfig()
        {
            ToTable("Revaluation");
            HasKey(p => p.TId);
            HasRequired(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId);
            HasMany(x => x.RevaluationItems).WithRequired(x => x.Revaluation).HasForeignKey(x => x.RevaluationId);
        }
    }
}
