using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NPOI.HSSF.Record.Chart;
using Peacock.Common.Exceptions;
using Peacock.Common.Helper;
using Peacock.ZXEval.Data.Entities;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Repository.Repositories;
using Peacock.ZXEval.Service.Base;

namespace Peacock.ZXEval.Service
{
    public class CompanyService : SingModel<CompanyService>
    {

        private CompanyService()
        {
             
        }

       /// <summary>
        /// 获取公司列表
       /// </summary>
       /// <param name="condition"></param>
       /// <param name="buninessTypes"></param>
       /// <param name="index"></param>
       /// <param name="size"></param>
       /// <param name="total"></param>
       /// <returns></returns>
        public IList<Company> GetCompanyList(CompanyCondition condition, List<string> buninessTypes, int index, int size, out int total)
        {
            var query = CompanyRepository.Instance.Source;
            if (!string.IsNullOrEmpty(condition.CompanyName))
            {
                query = query.Where(x => x.CompanyName.Contains(condition.CompanyName));
            }
           
            if (condition.IsEnabled.HasValue)
            {
                query = query.Where(x => x.IsEnabled == condition.IsEnabled.Value);
            }

           if (buninessTypes != null)
           {
               foreach (var businessType in buninessTypes)
               {
                   var busQuery = CompanyBaseDataRepository.Instance.Source.Where(x => x.ConfigName == businessType && x.ConfigType == ConfigTypeEnum.BusinessType).Select(x => x.CompanyId);
                   query = query.Where(x => busQuery.Contains(x.Id));
               }
           }
            query = query.OrderByDescending(x => x.Id);
            return CompanyRepository.Instance.FindForPaging(size, index, query, out total).ToList();
        }

        /// <summary>
        /// 通过ID获取公司信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Company GetByCompanyId(long id)
        {
            var query = CompanyRepository.Instance.Find(x => x.Id == id);
            return query.FirstOrDefault();
        }

      /// <summary>
        /// 添加公司
      /// </summary>
      /// <param name="company"></param>
      /// <param name="user"></param>
      /// <param name="businessTypes"></param>
        public void Create(Company company,User user,List<string> businessTypes )
        {
            if (string.IsNullOrEmpty(company.CompanyName))
            {
                throw new ServiceException("公司名称不能为空！");
            }
            if (string.IsNullOrEmpty(user.UserName))
            {
                throw new ServiceException("用户名称不能为空！");
            }
            if (string.IsNullOrEmpty(user.Password))
            {
                throw new ServiceException("密码不能为空！");
            }
            var companyExists = CompanyRepository.Instance.Find(x =>x.CompanyName.ToLower()==company.CompanyName.ToLower()).Any();
            if (companyExists)
            {
                throw new ServiceException("已经存在相同的公司名称，不能进行此操作！");
            }
            var userExists = UserRepository.Instance.Find(x => x.UserName.ToLower() == user.UserName.ToLower()).Any();
            if (userExists)
            {
                throw new ServiceException("已经存在相同的用户名称，不能进行此操作！");
            }

           CompanyRepository.Instance.Transaction(() =>
           {
               company.CreateTime = DateTime.Now;
               company.IsEnabled = true;              
               var dbCompany = CompanyRepository.Instance.InsertReturnEntity(company);

               user.CreateTime = DateTime.Now;
               user.CompanyId = dbCompany.Id;
               user.IsEnabled = true;
               user.IsAdmin = false;
               user.UserKeyId = string.Format("{0:N}", Guid.NewGuid());
               user.UserAccessKey = string.Format("{0:N}", Guid.NewGuid());
               user.PhoneNumber = user.PhoneNumber;
               UserRepository.Instance.Insert(user);

               if (businessTypes!=null)
               {
                   foreach (var businessType in businessTypes)
                   {
                       var companyBaseData = new CompanyBaseData();
                       companyBaseData.ConfigName = businessType;
                       companyBaseData.ConfigType = ConfigTypeEnum.BusinessType;
                       companyBaseData.CompanyId = dbCompany.Id;
                       companyBaseData.CreateTime = DateTime.Now;
                       CompanyBaseDataRepository.Instance.Insert(companyBaseData);
                   }
                }
           });
        }

        /// <summary>
        /// 保存公司
        /// </summary>
        /// <param name="company"></param>
        /// <param name="user"></param>
        /// <param name="businessTypes"></param>
        public void Save(Company company,User user, List<string> businessTypes)
        {
            if (string.IsNullOrEmpty(company.CompanyName))
            {
                throw new ServiceException("公司名称不能为空！");
            }
            var companyExists = CompanyRepository.Instance.Find(x => x.CompanyName.ToLower() == company.CompanyName.ToLower() && x.Id!=company.Id).Any();
            if (companyExists)
            {
                throw new ServiceException("已经存在相同的公司名称，不能进行此操作！");
            }

            var userEntity = UserRepository.Instance.Find(x => x.CompanyId == company.Id).FirstOrDefault();
           
            if (userEntity == null)
            {
                throw new ServiceException("找不到对应的管理员，不能进行此操作！");
            }
           
            var companyBaseDatas = CompanyBaseDataService.Instance.GetCompanyBuninessTypeList(company.Id);
            foreach (var companyBaseData in companyBaseDatas)
            {
                CompanyBaseDataRepository.Instance.Delete(companyBaseData);
            }
           
            CompanyRepository.Instance.Transaction(() =>
            {
                CompanyRepository.Instance.Save(company);
                if (businessTypes != null)
                {
                    foreach (var businessType in businessTypes)
                    {
                        var companyBaseData = new CompanyBaseData();
                        companyBaseData.ConfigName = businessType;
                        companyBaseData.ConfigType = ConfigTypeEnum.BusinessType;
                        companyBaseData.CompanyId = company.Id;
                        companyBaseData.CreateTime = DateTime.Now;
                        CompanyBaseDataRepository.Instance.Insert(companyBaseData);
                    }
                }

                userEntity.PhoneNumber = user.PhoneNumber;
                UserRepository.Instance.Save(userEntity);
            });
        }

        /// <summary>
        /// 状态变更
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        public void SaveStatus(long id, bool status)
        {
            var company = CompanyRepository.Instance.Find(x => x.Id == id).FirstOrDefault();
            if (company == null)
            {
                throw new ServerException("公司不存在");
            }

            company.IsEnabled = status;
            CompanyRepository.Instance.Save(company);

            var user = UserService.Instance.GetUserList(id).FirstOrDefault();
            user.IsEnabled = status;
            UserRepository.Instance.Save(user);
        }

        /// <summary>
        ///  根据业务类型获取所有评估公司列表
        /// </summary>
        /// <param name="businessType"></param>
        /// <returns></returns>
        public IList<Company> GetCompanyList(string businessType)
        {
            var query = CompanyRepository.Instance.Source;
            query = query.Where(x => x.IsEnabled == true);
            if (!string.IsNullOrEmpty(businessType))
            {
                var busList = businessType.Split(',').ToList();
                foreach (var item in busList)
                {
                    var busQuery = CompanyBaseDataRepository.Instance.Source.Where(x => x.ConfigName == item && x.ConfigType == ConfigTypeEnum.BusinessType).Select(x => x.CompanyId);
                    query = query.Where(x => busQuery.Contains(x.Id));
                }
            }
            query = query.OrderByDescending(x => x.Id);
            return query.ToList();
        }       
    }
}
