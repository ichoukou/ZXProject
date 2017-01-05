using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Peacock.ZXEval.Data.Entities;
using Peacock.ZXEval.DataAdapter.Interface;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.Service;

namespace Peacock.ZXEval.DataAdapter.Implement
{
    public class CompanyBaseDataAdapter : ICompanyBaseDataAdapter
    {

        /// <summary>
        /// 根据公司Id数组获取业务类型
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        public IList<CompanyBaseDataModel> GetCompanyBuninessTypeList(List<long> companyIds)
        {
            return CompanyBaseDataService.Instance.GetCompanyBuninessTypeList(companyIds).ToListModel<CompanyBaseDataModel, CompanyBaseData>();
        }

        /// <summary>
        /// 根据公司Id获取业务类型
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IList<CompanyBaseDataModel> GetCompanyBuninessTypeList(long companyId)
        {
            return CompanyBaseDataService.Instance.GetCompanyBuninessTypeList(companyId).ToListModel<CompanyBaseDataModel, CompanyBaseData>();
        }
    }
}
