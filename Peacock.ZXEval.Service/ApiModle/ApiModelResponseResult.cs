using System.Runtime.Serialization;

namespace Peacock.ZXEval.Service.ApiModle
{
    public class ApiModelResponseResult
    {
        public bool Success { get; set; }

        public string Msg { get; set; }

        public object Data { get; set; }

        public string Others { get; set; }
    }
}
