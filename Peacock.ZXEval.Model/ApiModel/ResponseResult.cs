using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.ZXEval.Model.ApiModel
{
    public class ResponseResult
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public object Data { get; set; }

        public string Others { get; set; }
    }
}
