using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.Model.ApiModel
{

    public class DelegateRequest
    {
        /// <summary>
        /// 交易编号
        /// </summary>
        public string bussinessId { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string bussinessType { get; set; }

        /// <summary>
        /// 业务数据
        /// </summary>
        public ProjectRequest bussinessForm { get; set; }

        /// <summary>
        /// 附件信息
        /// </summary>
        public ProjectResourceRequest[] files { get; set; }

    }

    public class ProjectRequest
    {
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
        public string CreditAmount { get; set; }

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
        public long? CompanyId { get; set; }

        /// <summary>
        /// 项目所属部门ID
        /// </summary>
        public string CompanyName { get; set; }
    }

    public class ProjectResourceRequest
    {
        /// <summary>
        /// 文件格式
        /// </summary>
        public string FileFormat{ get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName{ get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath{ get; set; }
    }
}
