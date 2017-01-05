using System.Collections.Generic;

namespace Peacock.ZXEval.Service.ApiModle
{
    public class ApiModelRevaluationResultRequest
    {
        /// <summary>
        /// 复估业务ID
        /// </summary>
        public string bussiness_ID;

        /// <summary>
        /// 是否全部推送完
        /// </summary>
        public bool is_complete;

        /// <summary>
        /// 复估结果集
        /// </summary>
        public List<ApiModelRevaluationItemResultRequest> result_list;
    }

    public class ApiModelRevaluationItemResultRequest
    {
        /// <summary>
        /// 复估结果记录ID
        /// </summary>
        public string record_ID;

        /// <summary>
        /// 复估结果值
        /// </summary>
        public string re_valuation_value;

        /// <summary>
        /// 复估时间
        /// </summary>
        public string re_valuation_time;

        /// <summary>
        /// 差额
        /// </summary>
        public string difference;

        /// <summary>
        /// 涨跌幅
        /// </summary>
        public string change_rate;

        /// <summary>
        /// 变动说明
        /// </summary>
        public string description_of_change;

        /// <summary>
        /// 备注
        /// </summary>
        public string notes;
    }
}
