using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Peacock.Common.Helper;
using System.Configuration;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Web;
using Peacock.ZXEval.Model.ApiModel;
using Peacock.ZXEval.WebSite.Help;

namespace Peacock.ZXEval.WebSite.Controllers
{

    public class HomeController : BaseController
    {


        public ActionResult Index()
        {
            ViewBag.CurrentUser = UserHelper.GetCurrentUser();
            return View();
        }

        public ActionResult Text(int id)
        {
            var bussinessId = Guid.NewGuid().ToString().Replace("-", "").ToLower();
            var tips = string.Empty;
            for (int j=0;j<10;j++)
            {
                var test = new RevaluateRequest();
                test.bussinessType = "复估";
                test.bussinessId = bussinessId;
                test.CompanyName = "测试公司";
                test.RevaluationName = "测试公司10086";
                test.EvalType = "对公业务";
                var list = new List<RevaluationItemRequest>();
                for (int i = 0; i < id; i++)
                {
                    list.Add(new RevaluationItemRequest()
                    {
                        BussinessId = Guid.NewGuid().ToString().Replace("-", "").ToLower(),
                        OperationOrganization = "中信机构",
                        BorrowerName = "李明",
                        CreditBalance = "10",
                        FiveCategories = "一级",
                        ContractExpirationDate = "2025-01-05",
                        PropertyType = "工业厂房",
                        PledgeAddress = "成都市工业大道北1号",
                        InitialEstimateValue = "100",
                        InitialEstimateOrganization = "仁达南京",
                        InitialEstimateTime = "2015-01-05 10:56:33.000",
                        CustomerAccountManager = "刘备",
                        ContactNumber = "13800138000",
                        ProtocolNumber = "123123123",
                        CustomerNumber = "321321321",
                        //ProjectNo = i%2 == 0 ? "494184046000" + i : ""
                        ProjectNo = ""
                    });
                }
                test.RevaluationItems = list;
                test.IsComplete = j == 9;
                var request = (HttpWebRequest)WebRequest.Create("http://localhost:3197/api/project/revaluate");
                request.Method = "POST";
                request.ContentType = "application/json";
                var postDataStr = JsonConvert.SerializeObject(test);
                byte[] requestBytes = Encoding.UTF8.GetBytes(postDataStr);
                request.ContentLength = requestBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(requestBytes, 0, requestBytes.Length);
                requestStream.Close();
                request.Credentials = CredentialCache.DefaultCredentials;
                var timer = new Stopwatch();
                timer.Start();
                var response = (HttpWebResponse)request.GetResponse();
                timer.Stop();
                tips = string.Format("{0}{1}秒\n", tips, timer.ElapsedMilliseconds/1000);
            }
            return Content(tips);
        }
    }
}
