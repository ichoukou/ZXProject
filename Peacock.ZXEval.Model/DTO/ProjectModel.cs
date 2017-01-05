using System;
using System.Collections.Generic;

namespace Peacock.ZXEval.Model.DTO
{
    public class ProjectModel
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 评估编号
        /// </summary>
        public string ProjectNo { get; set; }

        /// <summary>
        /// 业务编号
        /// </summary>
        public string BusinessId { get; set; }

        /// <summary>
        /// 押品类型
        /// </summary>
        public string PropertyType { get; set; }

        /// <summary>
        /// 授信客户名称
        /// </summary>
        public string CreditCustomer { get; set; }

        /// <summary>
        /// 拟授信金额
        /// </summary>
        public decimal CreditAmount { get; set; }

        /// <summary>
        /// 抵押物详细地址
        /// </summary>
        public string PledgeAddress { get; set; }

        /// <summary>
        /// 客户经理联系电话
        /// </summary>
        public string AccountManagerTel { get; set; }

        /// <summary>
        /// 评估范围
        /// </summary>
        public string EvalRange { get; set; }

        /// <summary>
        /// 抵质押人名称
        /// </summary>
        public string PledgePerson { get; set; }

        /// <summary>
        /// 评估联系人名称
        /// </summary>
        public string EvalContractPerson { get; set; }

        /// <summary>
        /// 评估联系电话
        /// </summary>
        public string EvalContractTel { get; set; }

        /// <summary>
        ///  产权证信息（房证号）
        /// </summary>
        public string HouseCertInfo { get; set; }

        /// <summary>
        ///  产权证信息（土地证号）
        /// </summary>
        public string LandCertInfo { get; set; }

        /// <summary>
        /// 贷款联系人
        /// </summary>
        public string LoanPerson { get; set; }

        /// <summary>
        /// 贷款联系人电话
        /// </summary>
        public string LoanTel { get; set; }

        /// <summary>
        /// 是否在本行首次授信
        /// </summary>
        public bool IsFirstCredit { get; set; }

        /// <summary>
        /// 授信品种
        /// </summary>
        public string CreditVariety { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string EvalType { get; set; }

        /// <summary>
        /// 项目所属部门ID
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// 所属公司
        /// </summary>
        public CompanyModel Company{ get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
     
        /// <summary>
        /// 项目状态
        /// </summary>
        public ProjectStatusEnum ProjectStatus { get; set; }

        /// <summary>
        /// 撤单原因
        /// </summary>
        public string RevokeReason { get; set; }

        //公司是否已扫描
        public bool? IsScan { get; set; }

        //公司扫描时间 
        public DateTime? ScanTime { get; set; }

        //汇总数据
        public SummaryDataModel SummaryData { get; set; }

        //外采数据
        public OuterTaskModel OuterTask { get; set; }

        /// <summary>
        /// 与状态的对应关系
        /// </summary>
        public List<ProjectStateInfoModel> ProjectStateInfos { get; set; }

        /// <summary>
        /// 评估资料
        /// </summary>
        public List<ProjectResourceModel> ProjectResources { get; set; }
    }

    public enum ProjectStatusEnum
    {
        未受理 = 0,
        业务受理 = 1,
        外业勘察 = 2,
        报告预估 = 3,
        报告准备 = 4,
        //报告审核 = 5,
        发送报告 = 6,
        已撤单 = 7
    }
}
