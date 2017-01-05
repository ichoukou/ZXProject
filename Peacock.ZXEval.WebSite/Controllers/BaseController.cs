using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Peacock.ZXEval.DataAdapter;
using System;
using System.Configuration;
using Newtonsoft.Json;
using System.Text;
using System.Net;
using System.IO;
using Peacock.Common.Helper;
using Peacock.ZXEval.DataAdapter.Interface;

namespace Peacock.ZXEval.WebSite.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
        protected static readonly string SuccessMessage = "操作成功";
        protected static readonly IUserAdapter UserService = ConditionFactory.Conditions.Resolve<IUserAdapter>();
        protected static readonly ICompanyAdapter CompanyService = ConditionFactory.Conditions.Resolve<ICompanyAdapter>();
        protected static readonly ICompanyBaseDataAdapter CompanyBaseDataService = ConditionFactory.Conditions.Resolve<ICompanyBaseDataAdapter>();
        protected static readonly IProjectAdapter ProjectService = ConditionFactory.Conditions.Resolve<IProjectAdapter>();
        protected static readonly IParameterAdapter ParameterService = ConditionFactory.Conditions.Resolve<IParameterAdapter>();
        protected static readonly IRevaluationAdapter RevaluationService = ConditionFactory.Conditions.Resolve<IRevaluationAdapter>();

        protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        {

            base.Initialize(requestContext);
        }
    }

}