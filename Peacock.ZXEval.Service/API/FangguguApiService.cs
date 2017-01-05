using System.Configuration;
using Peacock.ZXEval.Service.ApiModle;

namespace Peacock.ZXEval.Service.API
{
    public class FangguguApiService: BaseClientService
    {
        public FangguguApiService()
            : base(ConfigurationManager.AppSettings["FangguguURL"])
        {
        }

        /// <summary>
        /// 发送项目状态
        /// </summary>
        public ApiModelResponseResult SendStatus(ApiModelSendStatusRequest request)
        {
            return Post<ApiModelSendStatusRequest, ApiModelResponseResult>(request, "/BusinessDelegate/saveWeituopingguStates_noLogin");
        }

        /// <summary>
        /// 发送汇总数据
        /// </summary>
        public ApiModelResponseResult SendReport(ApiModelSendReportRequest request)
        {
            return Post<ApiModelSendReportRequest, ApiModelResponseResult>(request, "/BusinessDelegate/saveWeituopingguhuizongxinxi");
        }

        /// <summary>
        /// 发送复估结果
        /// </summary>
        public ApiModelResponseResult SendRevaluationResult(ApiModelRevaluationResultRequest request)
        {
            return Post<ApiModelRevaluationResultRequest, ApiModelResponseResult>(request, "/CollateralManage/revaluationresultpush");
        }

        /// <summary>
        /// 发送复估异议认可结果
        /// </summary>
        public ApiModelResponseResult SendConsultResult(ApiModelConsultResultRequest request)
        {
            return Post<ApiModelConsultResultRequest, ApiModelResponseResult>(request, "/CollateralManage/objectionreply");
        }
    }
}
