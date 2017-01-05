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
    public class ParameterService : SingModel<ParameterService>
    {

        private ParameterService()
        {

        }

        /// <summary>
        /// 根据父键名称获取参数信息列表
        /// </summary>
        /// <returns></returns>
        public IList<Parameter> GetListByParentName(string parentName)
        {
            var parentEntity = ParameterRepository.Instance.Find(x => x.Name == parentName).FirstOrDefault();
            if (parentEntity == null)
            {
                parentEntity=new Parameter(){Id=0};
            }
            return ParameterRepository.Instance.Find(x => x.ParentId == parentEntity.Id).ToList();
        }
    }
}
