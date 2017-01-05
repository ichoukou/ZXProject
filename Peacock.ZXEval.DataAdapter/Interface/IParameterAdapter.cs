using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.DataAdapter.Interface
{
    public interface IParameterAdapter
    {
        /// <summary>
        /// 根据父键名称获取参数信息列表
        /// </summary>
        /// <returns></returns>
        IList<ParameterModel> GetListByParentName(string parentName);
    }
}
