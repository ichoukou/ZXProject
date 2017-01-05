using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Peacock.ZXEval.DataAdapter;
using Peacock.ZXEval.DataAdapter.Interface;


namespace Peacock.ZXEval.WebApi.ApiControllers
{
    public class BaseApiController : ApiController
    {

        protected static readonly IProjectAdapter ProjectService = ConditionFactory.Conditions.Resolve<IProjectAdapter>();
        protected static readonly ICompanyAdapter CompanyService = ConditionFactory.Conditions.Resolve<ICompanyAdapter>();


        protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }
    }
}