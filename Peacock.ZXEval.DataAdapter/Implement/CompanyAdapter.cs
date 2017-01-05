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
    public class CompanyAdapter : ICompanyAdapter
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
        public IList<CompanyModel> GetCompanyList(CompanyCondition condition, List<string> buninessTypes, int index, int size,
            out int total)
        {
            return CompanyService.Instance.GetCompanyList(condition, buninessTypes, index, size, out total).ToListModel<CompanyModel, Company>();
        }

        /// <summary>
        /// 通过ID获取公司信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CompanyModel GetByCompanyId(long id)
        {
            return CompanyService.Instance.GetByCompanyId(id).ToModel<CompanyModel>();
        }

        /// <summary>
        ///  添加公司
        /// </summary>
        /// <param name="user"></param>
        /// <param name="company"></param>
        public void Create(CompanyModel company, UserModel user, List<string> businessTypes)
        {
            CompanyService.Instance.Create(company.ToModel<Company>(), user.ToModel<User>(), businessTypes);
        }

        /// <summary>
        /// 保存公司
        /// </summary>
        /// <param name="company"></param>
        /// <param name="businessTypes"></param>
        public void Save(CompanyModel company, UserModel user, List<string> businessTypes)
        {
            CompanyService.Instance.Save(company.ToModel<Company>(),user.ToModel<User>(), businessTypes);
        }

        /// <summary>
        /// 状态变更
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        public void SaveStatus(long id, bool status)
        {
            CompanyService.Instance.SaveStatus(id,status);
        }

        /// <summary>
        ///  根据业务类型获取所有评估公司列表
        /// </summary>
        /// <param name="businessType"></param>
        /// <returns></returns>
        public IList<CompanyModel> GetCompanyList(string businessType)
        {
            return CompanyService.Instance.GetCompanyList(businessType).ToListModel<CompanyModel,Company>();
        }
    }
}
