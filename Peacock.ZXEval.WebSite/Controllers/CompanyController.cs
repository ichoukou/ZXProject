using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using Peacock.Common.Exceptions;
using Peacock.Common.Helper;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.Model.Enum;
using Peacock.ZXEval.WebSite.Help;
using Peacock.ZXEval.WebSite.Models;

namespace Peacock.ZXEval.WebSite.Controllers
{
    public class CompanyController : BaseController
    {
        /// <summary>
        /// 公司管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.BusinessType = EnumberHelper.EnumToList<BusinessTypeEnum>();
            return View();
        }

        /// <summary>
        /// 获取公司列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCompanyList(CompanyCondition condition, List<string> buninessTypes, int index, int rows)
        {
            int total;
            var result = CompanyService.GetCompanyList(condition,buninessTypes, index, rows, out total);
            var companyIds = result.Select(x => x.Id).ToList();
            //用户
            var companyUsers = UserService.GetUserList(companyIds);
            //业务类型
            var companyBusinessTypes = CompanyBaseDataService.GetCompanyBuninessTypeList(companyIds);     
            return Json(new
            {
                rows = result.Select(x =>
                {
                    var user = companyUsers.Where(y => y.CompanyId == x.Id).FirstOrDefault();
                    var businessTypeList = companyBusinessTypes.Where(y => y.CompanyId == x.Id).Select(z => z.ConfigName).ToArray();
                    return new
                    {
                        x.Id,
                        x.CompanyName,
                        UserName = user==null?"":string.Format("{0}", user.UserName),
                        BusinessTypes = string.Join(",", businessTypeList),
                        UserKeyId = string.Format("{0}", user.UserKeyId),
                        UserAccessKey = string.Format("{0}", user.UserAccessKey),
                        CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", x.CreateTime),
                        x.IsEnabled,
                        Status = x.IsEnabled ? "可用" : "禁用"
                    };
                }),
                total
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加公司
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.BusinessType = EnumberHelper.EnumToList<BusinessTypeEnum>();
            return View();
        }

        /// <summary>
        /// 添加公司-保存
        /// </summary>
        /// <param name="company"></param>
        /// <param name="user"></param>
        /// <param name="BuninessType"></param>
        /// <returns></returns>
        public JsonResult Save(CompanyModel company, UserModel user, List<string> BuninessType)
        {
            return ExceptionCatch.Invoke(() =>
            {
                user.Password = CryptTools.Md5(user.Password);
                CompanyService.Create(company, user, BuninessType);
            });
        }

        /// <summary>
        /// 修改公司
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit(long id)
        {
            ViewBag.BusinessType = EnumberHelper.EnumToList<BusinessTypeEnum>();
            ViewBag.Company = CompanyService.GetByCompanyId(id);
            ViewBag.User = UserService.GetUserList(id).FirstOrDefault();
            ViewBag.BusinessTypes = CompanyBaseDataService.GetCompanyBuninessTypeList(id).Select(x=>x.ConfigName).ToList();
            return View();
        }


        /// <summary>
        /// 修改公司-保存
        /// </summary>
        /// <param name="company"></param>
        /// <param name="BuninessType"></param>
        /// <returns></returns>
        public JsonResult SaveInfo(CompanyModel company,UserModel user, List<string> BuninessType)
        {
            return ExceptionCatch.Invoke(() =>
            {
                CompanyService.Save(company, user,BuninessType);
            });
        }


        /// <summary>
        /// 启用，禁用
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public JsonResult SaveStatus(long id, bool status)
        {
            return ExceptionCatch.Invoke(() =>
            {
               CompanyService.SaveStatus(id,status);
            });
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public JsonResult ChangePassword(long companyId, string password)
        {
            return ExceptionCatch.Invoke(() =>
            {
                var user = UserService.GetUserList(companyId).FirstOrDefault();
                if (user == null)
                {
                    throw new ServiceException("公司不存在用户");
                }
                UserService.ChangePassword(user.Id,password);
            });
        }

    }
}
