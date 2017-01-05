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
    public class UserService : SingModel<UserService>
    {

        private UserService()
        {

        }

        /// <summary>
        /// 根据公司Id数组获取用户 
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        public IList<User> GetUserList(List<long> companyIds)
        {
            var query = UserRepository.Instance.Find(x => companyIds.Contains(x.CompanyId));
            return query.ToList();
        }

        /// <summary>
        /// 根据公司Id获取用户 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IList<User> GetUserList(long companyId)
        {
            var query = UserRepository.Instance.Find(x => x.CompanyId == companyId);
            return query.ToList();
        }

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        public void ChangePassword(long id, string password)
        {
            var user = UserRepository.Instance.Find(x => x.Id == id).FirstOrDefault();
            user.Password = CryptTools.Md5(password);
            UserRepository.Instance.Save(user);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public User UserLogin(string userAccount, string password)
        {
            var user = UserRepository.Instance.Find(x => x.UserName.ToLower() == userAccount.ToLower()).FirstOrDefault();
            if (user == null)
            {
                throw new ServiceException("用户不存在！");
            }
            if (user.Password != password)
            {
                throw new ServiceException("密码错误！");
            }
            if (user.IsEnabled == false)
            {
                throw new ServiceException("用户已禁用，请联系系统管理员！");
            }
            return user;
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetUser(long userId)
        {
            return UserRepository.Instance.Find(x => x.Id == userId).FirstOrDefault();
        }


        /// <summary>
        /// 根据数据签名Id获取用户
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <returns></returns>
        public User GetUserByKeyId(string userKeyId)
        {
            return UserRepository.Instance.Find(x => x.UserKeyId == userKeyId).FirstOrDefault();
        }
    }
}
