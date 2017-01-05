using System;

namespace Peacock.ZXEval.Model.Condition
{
    public class ConsultCondition
    {
        /// <summary>
        /// 流水号
        /// </summary>
        public string ProjectNo { get; set; }

        /// <summary>
        /// 项目地址
        /// </summary>
        public string PledgeAddress { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string EvalType { get; set; }

        /// <summary>
        /// 押品类型
        /// </summary>
        public string PropertyType { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? RevaluationTimeFrom { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? RevaluationTimeTo { get; set; }

        /// <summary>
        /// 公司扫描类型 0-全部 1-获取未曾扫描记录
        /// </summary>
        public int ScanType { get; set; }
    }
}
