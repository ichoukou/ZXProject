using System.Collections.Generic;

namespace Peacock.ZXEval.Model.ApiModel
{
    public class RevaluateRequest
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
        /// 评估公司
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string EvalType { get; set; }

        /// <summary>
        /// 复估名称
        /// </summary>
        public string RevaluationName { get; set; }

        /// <summary>
        /// 是否传递完
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// 与复估项目的对应关系
        /// </summary>
        public List<RevaluationItemRequest> RevaluationItems;
    }

    public class RevaluationItemRequest
    {
        /// <summary>
        /// 业务ID
        /// </summary>
        public string BussinessId { get; set; }

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
        public string CreditBalance { get; set; }

        /// <summary>
        /// 五级分类
        /// </summary>
        public string FiveCategories { get; set; }

        /// <summary>
        /// 合同到期日
        /// </summary>
        public string ContractExpirationDate { get; set; }

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
        public string InitialEstimateValue { get; set; }

        /// <summary>
        /// 初估评估机构名称
        /// </summary>
        public string InitialEstimateOrganization { get; set; }

        /// <summary>
        /// 初估时间
        /// </summary>
        public string InitialEstimateTime { get; set; }

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
    }
}
