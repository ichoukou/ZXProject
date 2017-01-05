using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.ZXEval.Model.ApiModel
{

    public class RevokeRequest
    {
        /// <summary>
        /// 业务类型
        /// </summary>
        public string bussinessType { get; set; }

        /// <summary>
        /// 交易编号
        /// </summary>
        public string bussinessId { get; set; }

        /// <summary>
        /// 撤单原因
        /// </summary>
        public string reason { get; set; }
    }

}
