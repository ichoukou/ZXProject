using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Peacock.Common.Exceptions;
using Peacock.Common.Helper;
using Peacock.ZXEval.Data.Entities;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Repository.Repositories;
using Peacock.ZXEval.Service.Base;

namespace Peacock.ZXEval.Service
{
    public class CompanyBaseDataService : SingModel<CompanyBaseDataService>
    {

        private CompanyBaseDataService()
        {
             
        }

        /// <summary>
        /// 根据公司Id数组获取业务类型
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        public IList<CompanyBaseData> GetCompanyBuninessTypeList(List<long> companyIds)
        {
            var query = CompanyBaseDataRepository.Instance.Find(x => companyIds.Contains(x.CompanyId) && x.ConfigType==ConfigTypeEnum.BusinessType);
            return query.ToList();
        }

        /// <summary>
        /// 根据公司Id获取业务类型
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IList<CompanyBaseData> GetCompanyBuninessTypeList(long companyId)
        {
            var query = CompanyBaseDataRepository.Instance.Find(x => x.CompanyId == companyId && x.ConfigType == ConfigTypeEnum.BusinessType);
            return query.ToList();
        } 
    }
}
