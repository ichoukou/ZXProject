using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.DataAdapter.Interface
{
    public interface ICompanyAdapter
    {

        /// <summary>
        /// 获取公司列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="buninessTypes"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        IList<CompanyModel> GetCompanyList(CompanyCondition condition, List<string> buninessTypes, int index, int size,
            out int total);

        /// <summary>
        /// 通过ID获取公司信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        CompanyModel GetByCompanyId(long id);

        /// <summary>
        ///  添加公司
        /// </summary>
        /// <param name="user"></param>
        /// <param name="company"></param>
        void Create(CompanyModel company, UserModel user, List<string> businessTypes);

        /// <summary>
        /// 保存公司
        /// </summary>
        /// <param name="company"></param>
        /// <param name="businessTypes"></param>
        void Save(CompanyModel company, UserModel user, List<string> businessTypes);

        /// <summary>
        /// 状态变更
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        void SaveStatus(long id, bool status);

        /// <summary>
        ///  根据业务类型获取所有评估公司列表
        /// </summary>
        /// <param name="businessType"></param>
        /// <returns></returns>
        IList<CompanyModel> GetCompanyList(string businessType);
    
    }
}
