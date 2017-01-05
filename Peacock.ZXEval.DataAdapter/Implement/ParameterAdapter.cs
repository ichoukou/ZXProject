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
    public class ParameterAdapter : IParameterAdapter
    {
        /// <summary>
        /// 根据父键名称获取参数信息列表
        /// </summary>
        /// <returns></returns>
        public IList<ParameterModel> GetListByParentName(string parentName)
        {
            return ParameterService.Instance.GetListByParentName(parentName).ToListModel<ParameterModel, Parameter>();
        }
    }
}
