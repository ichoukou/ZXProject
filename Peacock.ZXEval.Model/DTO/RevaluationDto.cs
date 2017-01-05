using System;

namespace Peacock.ZXEval.Model.DTO
{
    /// <summary>
    /// 复估表Dto
    /// </summary>
    public class RevaluationDto
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
        public CompanyModel Company { get; set; }

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

        //公司是否已扫描
        public bool? IsScan { get; set; }

        //公司扫描时间 
        public DateTime? ScanTime { get; set; }

        //可否复估完成
        public bool canFinish { get; set; }
    }
}
