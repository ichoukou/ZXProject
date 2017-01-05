using Newtonsoft.Json;
using Peacock.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Peacock.ZXEval.WebSite
{
    /// <summary>
    /// 操作日志记录
    /// </summary>
    public class OperationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="operationDesc"></param>
        public OperationAttribute(string operationDesc)
        {
            this.OperationDesc = operationDesc;
            this.IsLog = true;
        }
        public string OperationDesc { get; set; } 
        public bool IsLog { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (this.IsLog)
            {
               var actionName = filterContext.ActionDescriptor.ActionName;
                var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName; 
                StringBuilder sb = new StringBuilder(); 
                foreach(string key in  filterContext.HttpContext.Request.Form){
                    sb.AppendFormat("{0}={1}&", key, filterContext.HttpContext.Request.Form[key]);
                } 
               if (sb.Length>0)
                 sb=  sb.Remove(sb.Length - 1, 1);

               // LogHelper.Ilog(string.Format(" /{0}/{1}?{2}", controllerName, actionName, sb.ToString()), this.OperationDesc);
            }
        }
    } 
}