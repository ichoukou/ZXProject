using System;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.Model.Condition
{
    public class RevaluationCondition
    {
        /// <summary>
        /// 复估单号
        /// </summary>
        public string RevaluationNo { get; set; }

        /// <summary>
        /// 复估名称
        /// </summary>
        public string RevaluationName { get; set; }

        /// <summary>
        /// 业务类型
        /// </summary>
        public string EvalType { get; set; }

        /// <summary>
        /// 当前进度
        /// </summary>
        public RevaluationStatusEnum? RevaluationStatus { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? CreateTimeFrom { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? CreateTimeTo { get; set; }

        /// <summary>
        /// 公司扫描类型 0-全部 1-获取未曾扫描记录
        /// </summary>
        public int ScanType { get; set; }
    }
}
