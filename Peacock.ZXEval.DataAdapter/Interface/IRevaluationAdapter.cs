using System.Collections.Generic;
using Peacock.ZXEval.Model.ApiModel;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.DataAdapter.Interface
{
    public interface IRevaluationAdapter
    {
        /// <summary>
        /// 接复估单
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="items"></param>
        /// <param name="companyName"></param>
        /// <param name="isComplete"></param>
        /// <param name="revaluationNo"></param>
        /// <returns></returns>
        Dictionary<string, string> ReceiveRevaluation(RevaluationModel entity, List<RevaluationItemRequest> items,
            string companyName, bool isComplete, out string revaluationNo);

        /// <summary>
        /// 复估列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<RevaluationDto> GetRevaluationList(RevaluationCondition condition, int index, int size,
            out int total, long userId);

        /// <summary>
        /// 复估列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<RevaluationModel> GetRevaluationList4Api(RevaluationCondition condition, int index, int size,
            out int total, long userId);

        /// <summary>
        /// 获取复估单
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        RevaluationModel GetRevaluationById(long revaluationId, long userId);

        /// <summary>
        /// 复估项列表
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="canFinish"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<RevaluationItemModel> GetRevaluationItemList(long revaluationId, int index, int size, out int total,
            out bool canFinish, long userId);

        /// <summary>
        /// 获取复估项
        /// </summary>
        /// <param name="revaluationItemId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        RevaluationItemModel GetRevaluationItemById(long revaluationItemId, long userId);

        /// <summary>
        /// 复估受理
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool AcceptRevaluation(long revaluationId, long userId);

        /// <summary>
        /// 单项复估
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool RevaluatingRevaluationItem(RevaluationItemModel entity, long userId);

        /// <summary>
        /// 导出复估
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        byte[] ExportRevaluation(long revaluationId, long userId);

        /// <summary>
        /// 导入复估结果
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        /// <param name="userId"></param>
        string UploadRevaluationResult(long revaluationId, string fileName, byte[] fileByte, long userId);

        /// <summary>
        /// 完成复估
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool FinishRevaluation(long revaluationId, long userId);

        /// <summary>
        /// 复估价格异议
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        bool ConsultRevaluation(string businessId);

        /// <summary>
        /// 撤销复估价格异议
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        bool CancelConsultRevaluation(string businessId);

        /// <summary>
        /// 复估异议列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<RevaluationItemModel> GetConsultList(ConsultCondition condition, int index, int size, out int total,
            long userId);

        /// <summary>
        /// 复估认可
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool ApproveRevaluationItem(RevaluationItemModel entity, long userId);

        /// <summary>
        /// 保存扫描记录
        /// </summary>
        /// <param name="idList"></param>
        void SaveScanResult(IList<long> idList);

        /// <summary>
        /// 获取复估单
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        RevaluationModel GetByRevaluationNo(string revaluationNo, long userId);

        /// <summary>
        /// 获取复估项
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="projectNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        RevaluationItemModel GetRevaluationItem(string revaluationNo, string projectNo, long userId);

        /// <summary>
        /// 保存扫描复估认可记录
        /// </summary>
        /// <param name="idList"></param>
        void SaveScanProjectResult(IList<long> idList);

        /// <summary>
        /// 批量复估保存
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="revalItems"></param>
        /// <param name="userId"></param>
        void SaveBatchRevalProject(string revaluationNo, IList<RevaluationItemModel> revalItems, long userId);
    }
}
