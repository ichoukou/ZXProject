using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.DataAdapter.Interface
{
    public interface IUserAdapter
    {
        /// <summary>
        /// 根据公司Id数组获取用户 
        /// </summary>
        /// <param name="companyIds"></param>
        /// <returns></returns>
        IList<UserModel> GetUserList(List<long> companyIds);

        /// <summary>
        /// 根据公司Id获取用户 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        IList<UserModel> GetUserList(long companyId);

        /// <summary>
        /// 更改密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        void ChangePassword(long id, string password);

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="userAccount"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        UserModel UserLogin(string userAccount, string password);

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        UserModel GetUser(long userId);

        /// <summary>
        /// 根据数据签名Id获取用户
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <returns></returns>
        UserModel GetUserByKeyId(string userKeyId);
    }
}
