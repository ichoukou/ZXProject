using System;

namespace Peacock.ZXEval.Data.Entities
{
    /// <summary>
    /// 复估项目表
    /// </summary>
    public class RevaluationItem
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
        /// 复估单ID
        /// </summary>
        public long RevaluationId { get; set; }

        /// <summary>
        /// 复估单
        /// </summary>
        public virtual Revaluation Revaluation { get; set; }

        /// <summary>
        /// 经营机构
        /// </summary>
        public string OperationOrganization { get; set; }

        /// <summary>
        /// 借款人名称
        /// </summary>
        public string BorrowerName { get; set; }

        /// <summary>
        /// 授信余额（万元）
        /// </summary>
        public decimal CreditBalance { get; set; }

        /// <summary>
        /// 五级分类
        /// </summary>
        public string FiveCategories { get; set; }

        /// <summary>
        /// 合同到期日
        /// </summary>
        public DateTime ContractExpirationDate { get; set; }

        /// <summary>
        /// 押品类型
        /// </summary>
        public string PropertyType { get; set; }

        /// <summary>
        /// 押品地址
        /// </summary>
        public string PledgeAddress { get; set; }

        /// <summary>
        /// 初估价值(元)
        /// </summary>
        public decimal InitialEstimateValue { get; set; }

        /// <summary>
        /// 初估评估机构名称
        /// </summary>
        public string InitialEstimateOrganization { get; set; }

        /// <summary>
        /// 初估时间
        /// </summary>
        public DateTime InitialEstimateTime { get; set; }

        /// <summary>
        /// 管户客户经理
        /// </summary>
        public string CustomerAccountManager { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        public string ContactNumber { get; set; }

        /// <summary>
        /// 协议号
        /// </summary>
        public string ProtocolNumber { get; set; }

        /// <summary>
        /// 客户号
        /// </summary>
        public string CustomerNumber { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public string ProjectNo { get; set; }

        /// <summary>
        /// 重估时点
        /// </summary>
        public DateTime? RevaluationTime { get; set; }

        /// <summary>
        /// 复估价值
        /// </summary>
        public decimal? RevaluationValue { get; set; }

        /// <summary>
        /// 变动说明
        /// </summary>
        public string ChangeDescription { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 重估差额
        /// </summary>
        public decimal? RevaluationDifference { get; set; }

        /// <summary>
        /// 重估涨幅
        /// </summary>
        public decimal? RevaluationIncrease { get; set; }

        /// <summary>
        /// 是否质询
        /// </summary>
        public bool IsConsult { get; set; }

        /// <summary>
        /// 是否取消质询
        /// </summary>
        public bool? IsCancelConsult { get; set; }

        /// <summary>
        /// 是否认可
        /// </summary>
        public bool? IsApprove { get; set; }

        /// <summary>
        /// 质询回复
        /// </summary>
        public string ConsultReply { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        //公司是否已扫描
        public bool? IsScan { get; set; }

        //公司扫描时间 
        public DateTime? ScanTime { get; set; }

        /// <summary>
        /// 初估评估机构Id
        /// </summary>
        public long? InitialEstimateCompanyId { get; set; }

        /// <summary>
        /// 初估评估机构
        /// </summary>
        public virtual Company InitialEstimateCompany { get; set; }
    }

    internal class RevaluationItemConfig : EntityConfig<RevaluationItem>
    {
        internal RevaluationItemConfig()
        {
            ToTable("RevaluationItem");
            HasKey(x => x.TId);
            HasRequired(x => x.Revaluation).WithMany(x => x.RevaluationItems).HasForeignKey(x => x.RevaluationId);
            HasOptional(x => x.InitialEstimateCompany).WithMany().HasForeignKey(x => x.InitialEstimateCompanyId);
        }
    }
}
