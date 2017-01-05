using Peacock.Common.Helper;
using Peacock.ZXEval.DataAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Peacock.Common.Util;

namespace Peacock.ZXEval.WebSite
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //AuthConfig.RegisterAuth();
          
            //日志初始化
            var path = Server.MapPath("~/log4net.xml");
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(path));

            //实体转换初始化
            MapperConfig.Init();

            //JS与CSS文件版本号
            Application["JSVersion"] = StrUtil.GetRandomNumb(6);
        }

        /// <summary>
        /// 捕捉错误
        /// 用户错误编码为21190518
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(object sender, EventArgs e)
        {
            //Exception ex = Server.GetLastError();
            //LogHelper.Ilog(ex.Source + ex.StackTrace);
        }
    }
}