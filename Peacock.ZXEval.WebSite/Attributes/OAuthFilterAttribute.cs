using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection.Emit;
using System.Text;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using Peacock.ZXEval.DataAdapter;
using Peacock.ZXEval.DataAdapter.Interface;
using Peacock.ZXEval.Model.ApiModel;
using RsaHelperSdk;

namespace Peacock.ZXEval.WebSite.Attributes
{
    public class OAuthFilterAttribute : AuthorizeAttribute
    {
        protected static readonly IUserAdapter UserService = ConditionFactory.Conditions.Resolve<IUserAdapter>();

        //重写基类的验证方式，加入数字签名 
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            //string userKeyId_Sample = string.Format("{0}", "f2b3701d363d4975846c727eb2046ebf");
            //string userAccessKey_Sample = string.Format("{0}", "626788bc5c3d46179f79f03e808c775d");
            //string timestamp_Sample = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds).ToString();
            //string signValue_Sample = HttpUtility.UrlEncode(RsaHelperSdk.RsaHelper.computeSignature(userKeyId_Sample, timestamp_Sample, userAccessKey_Sample));
            var request = ((HttpContextWrapper)actionContext.Request.Properties["MS_HttpContext"]).Request;

            IDictionary<string, string> parameters = new Dictionary<string, string>();
            string userkeyId = string.Empty, timestamp = string.Empty, signature = string.Empty;

            if (request.HttpMethod == "GET" || request.HttpMethod == "POST")
            {
                foreach (var para in request.QueryString.AllKeys)
                {
                    var key = para.ToLower();
                    switch (key)
                    {
                        case "userkeyid":
                            userkeyId = string.Format("{0}", HttpUtility.UrlDecode(request.QueryString["userkeyid"]));
                            break;
                        case "time":
                            timestamp = string.Format("{0}", HttpUtility.UrlDecode(request.QueryString["time"]));
                            break;
                        case "signature":
                            signature = string.Format("{0}", HttpUtility.UrlDecode(request.QueryString["signature"]));
                            break;
                        default:
                            parameters.Add(para, HttpUtility.UrlDecode(request.QueryString[para], Encoding.UTF8));
                            break;
                    }
                }
            }
            var isAuthorized = ValidateTicket(userkeyId, timestamp, signature);
            if (isAuthorized)
            {
                base.IsAuthorized(actionContext);
            }
            else
            {
                ResponseResult data = new ResponseResult()
                {
                    Code = (int)HttpStatusCode.Unauthorized,
                    Message = "数字签名验证不通过！"
                };
                HttpResponseMessage message = new HttpResponseMessage()
                {
                    Content = new StringContent(JsonConvert.SerializeObject(data)),
                    StatusCode = System.Net.HttpStatusCode.OK //返回200 ，标识其请求成功

                };
                var exc = new HttpResponseException(message);
                throw exc;
            }
        }

        //校验票据（数据库数据匹配）  
        private bool ValidateTicket(string userKeyId, string time, string signature)
        {
            if (string.IsNullOrEmpty(userKeyId))
            {
                return false;
            }
            var user= UserService.GetUserByKeyId(userKeyId);
            if (user == null)
            {
                return false;
            }
            string result = RsaHelper.computeSignature(userKeyId, time, user.UserAccessKey);            
            return result == signature;
        }
    }
}