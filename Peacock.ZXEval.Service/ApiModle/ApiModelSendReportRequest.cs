using System;
using System.Configuration;

namespace Peacock.ZXEval.Service.ApiModle
{
    public class ApiModelSendReportRequest
    {
        public ApiModelSendReportRequest()
        {
            operationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 业务系统 报告编号
        /// </summary>
        public string yewuxitongbaogaoId { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public string operationTime { get; set; }

        /// <summary>
        /// 汇总数据
        /// </summary>
        public ApiModelSummaryDataRequest huizongxinxi { get; set; }

        /// <summary>
        /// 委托报告url
        /// </summary>
        public string baogaourl { get; set; }

        /// <summary>
        /// 报告类型
        /// </summary>
        public BaogaoTypeEnum baogaoType { get; set; }
    }

    public enum BaogaoTypeEnum
    {
        预估 = 0,
        正式 = 1
    }
}
