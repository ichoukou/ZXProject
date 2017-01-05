using System.Web;
using System.Web.Mvc;

namespace Peacock.ZXEval.WebSite
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new LogExceptionAttribute());
        }
    }
}