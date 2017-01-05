using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Peacock.Common.Helper;
using Peacock.ZXEval.Service.ApiModle;

namespace Peacock.ZXEval.Service.API
{
    public class EciticApiService
    {
        private readonly string Url;

        public EciticApiService()
        {
            Url = ConfigurationManager.AppSettings["SmsURL"];
        }

        public bool SendSms(ApiModleSmsRequest data)
        {
            LogHelper.Error("短信发送内容：" + data.ToJson(), null);
            var request = (HttpWebRequest) WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            //var postDataStr = JsonConvert.SerializeObject(data);
            var postDataStr = string.Format("teleno={0}&msg={1}", data.teleno, data.msg);
            byte[] requestBytes = Encoding.UTF8.GetBytes(postDataStr);
            request.ContentLength = requestBytes.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(requestBytes, 0, requestBytes.Length);
            requestStream.Close();
            request.Credentials = CredentialCache.DefaultCredentials;
            var response = (HttpWebResponse) request.GetResponse();
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null)
                {
                    LogHelper.Error("调用中信短信接口失败", null);
                    return false;
                }
                using (var reader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    var reponse = reader.ReadToEnd();
                    LogHelper.Error("调用中信短信接口返回内容：" + reponse, null);
                    return reponse.ToUpper() == "OK";
                }
            }
        }
    }
}
