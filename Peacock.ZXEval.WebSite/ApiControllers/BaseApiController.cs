using System.Web.Http;
using Peacock.ZXEval.DataAdapter;
using Peacock.ZXEval.DataAdapter.Interface;

namespace Peacock.ZXEval.WebSite.ApiControllers
{
    public class BaseApiController : ApiController
    {

        protected static readonly IProjectAdapter ProjectService = ConditionFactory.Conditions.Resolve<IProjectAdapter>();
        protected static readonly ICompanyAdapter CompanyService = ConditionFactory.Conditions.Resolve<ICompanyAdapter>();
        protected static readonly IRevaluationAdapter RevaluationService = ConditionFactory.Conditions.Resolve<IRevaluationAdapter>();
        protected static readonly IUserAdapter UserService = ConditionFactory.Conditions.Resolve<IUserAdapter>();


        protected override void Initialize(System.Web.Http.Controllers.HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }
    }
}