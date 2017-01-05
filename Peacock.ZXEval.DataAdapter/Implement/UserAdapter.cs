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
    public class UserAdapter : IUserAdapter
    {
        /// <summary>
        /// 根据公司Id数组获取用户 
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        public IList<UserModel> GetUserList(List<long> companyIds)
        {
            return UserService.Instance.GetUserList(companyIds).ToListModel<UserModel,User>();
        }

        /// <summary>
        /// 根据公司Id获取用户 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IList<UserModel> GetUserList(long companyId)
        {
            return UserService.Instance.GetUserList(companyId).ToListModel<UserModel, User>();

        }

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        public void ChangePassword(long id, string password)
        {
            UserService.Instance.ChangePassword(id,password);
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public UserModel UserLogin(string userAccount, string password)
        {
            return UserService.Instance.UserLogin(userAccount, password).ToModel<UserModel>();
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public UserModel GetUser(long userId)
        {
            return UserService.Instance.GetUser(userId).ToModel<UserModel>();
        }


        /// <summary>
        /// 根据数据签名Id获取用户
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <returns></returns>
        public UserModel GetUserByKeyId(string userKeyId)
        {
            return UserService.Instance.GetUserByKeyId(userKeyId).ToModel<UserModel>();
        }
    }
}
