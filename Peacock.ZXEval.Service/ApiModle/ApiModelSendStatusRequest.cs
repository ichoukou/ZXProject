using System;
using System.Configuration;

namespace Peacock.ZXEval.Service.ApiModle
{
    public class ApiModelSendStatusRequest
    {
        public ApiModelSendStatusRequest()
        {
            VerificationCode = ConfigurationManager.AppSettings["VerificationCode"];
            operationTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            city = "";
            note = "";
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
        /// 报告状态
        /// </summary>
        public string status { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string note { get; set; }
        /// <summary>
        /// 固定验证码 
        /// </summary>
        public string VerificationCode { get; set; }
        /// <summary>
        /// 城市名称
        /// </summary>
        public string city { get; set; }
        /// <summary>
        /// 反馈说明
        /// </summary>
        public string feedback_description { get; set; }
    }
}
