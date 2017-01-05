using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.DataAdapter.Interface
{
    public interface ICompanyBaseDataAdapter
    {
        /// <summary>
        /// 根据公司Id数组获取业务类型
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        IList<CompanyBaseDataModel> GetCompanyBuninessTypeList(List<long> companyIds);

        /// <summary>
        /// 根据公司Id获取业务类型
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        IList<CompanyBaseDataModel> GetCompanyBuninessTypeList(long companyId);
    }
}
