using System.Collections.Generic;
using System.Linq;
using Peacock.ZXEval.Data.Entities;
using Peacock.ZXEval.DataAdapter.Interface;
using Peacock.ZXEval.Model.ApiModel;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.Service;

namespace Peacock.ZXEval.DataAdapter.Implement
{
    public class RevaluationAdapter : IRevaluationAdapter
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
        public Dictionary<string, string> ReceiveRevaluation(RevaluationModel entity, List<RevaluationItemRequest> items,
            string companyName, bool isComplete, out string revaluationNo)
        {
            return
                RevaluationService.Instance.ReceiveRevaluation(entity.ToModel<Revaluation>(),
                    items.ToListModel<RevaluationItem, RevaluationItemRequest>(), companyName, isComplete, out revaluationNo);
        }

        /// <summary>
        /// 复估列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<RevaluationDto> GetRevaluationList(RevaluationCondition condition, int index, int size,
            out int total,long userId)
        {
            return
                RevaluationService.Instance.GetRevaluationList(condition, index, size, out total, userId)
                    .ToListModel<RevaluationDto, Revaluation>();
        }

        /// <summary>
        /// 复估列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<RevaluationModel> GetRevaluationList4Api(RevaluationCondition condition, int index, int size,
            out int total, long userId)
        {
            return
                RevaluationService.Instance.GetRevaluationList(condition, index, size, out total, userId)
                    .ToListModel<RevaluationModel, Revaluation>();
        }

        /// <summary>
        /// 获取复估单
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RevaluationModel GetRevaluationById(long revaluationId, long userId)
        {
            return RevaluationService.Instance.GetRevaluationById(revaluationId, userId).ToModel<RevaluationModel>();
        }

        /// <summary>
        /// 获取复估项
        /// </summary>
        /// <param name="revaluationItemId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RevaluationItemModel GetRevaluationItemById(long revaluationItemId, long userId)
        {
            return
                RevaluationService.Instance.GetRevaluationItemById(revaluationItemId, userId)
                    .ToModel<RevaluationItemModel>();
        }

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
        public IList<RevaluationItemModel> GetRevaluationItemList(long revaluationId, int index, int size, out int total,
            out bool canFinish, long userId)
        {
            return
                RevaluationService.Instance.GetRevaluationItemList(revaluationId, index, size, out total, out canFinish,
                    userId).ToListModel<RevaluationItemModel, RevaluationItem>();
        }

        /// <summary>
        /// 复估受理
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool AcceptRevaluation(long revaluationId, long userId)
        {
            return RevaluationService.Instance.AcceptRevaluation(revaluationId, userId);
        }

        /// <summary>
        /// 单项复估
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool RevaluatingRevaluationItem(RevaluationItemModel entity, long userId)
        {
            return RevaluationService.Instance.RevaluatingRevaluationItem(entity.ToModel<RevaluationItem>(), userId);
        }

        /// <summary>
        /// 导出复估
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public byte[] ExportRevaluation(long revaluationId, long userId)
        {
            return RevaluationService.Instance.ExportRevaluation(revaluationId, userId);
        }

        /// <summary>
        /// 导入复估结果
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        /// <param name="userId"></param>
        public string UploadRevaluationResult(long revaluationId, string fileName, byte[] fileByte, long userId)
        {
            return RevaluationService.Instance.UploadRevaluationResult(revaluationId, fileName, fileByte, userId);
        }

        /// <summary>
        /// 完成复估
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool FinishRevaluation(long revaluationId, long userId)
        {
            return RevaluationService.Instance.FinishRevaluation(revaluationId, userId);
        }

        /// <summary>
        /// 复估价格异议
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        public bool ConsultRevaluation(string businessId)
        {
            return RevaluationService.Instance.ConsultRevaluation(businessId);
        }

        /// <summary>
        /// 撤销复估价格异议
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        public bool CancelConsultRevaluation(string businessId)
        {
            return RevaluationService.Instance.CancelConsultRevaluation(businessId);
        }

        /// <summary>
        /// 复估异议列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<RevaluationItemModel> GetConsultList(ConsultCondition condition, int index, int size, out int total,
            long userId)
        {
            return
                RevaluationService.Instance.GetConsultList(condition, index, size, out total, userId)
                    .ToListModel<RevaluationItemModel, RevaluationItem>();
        }

        /// <summary>
        /// 复估认可
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool ApproveRevaluationItem(RevaluationItemModel entity, long userId)
        {
            return RevaluationService.Instance.ApproveRevaluationItem(entity.ToModel<RevaluationItem>(), userId);
        }

        /// <summary>
        /// 保存扫描记录
        /// </summary>
        /// <param name="idList"></param>
        public void SaveScanResult(IList<long> idList)
        {
            RevaluationService.Instance.SaveScanResult(idList);
        }

        /// <summary>
        /// 获取复估单
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RevaluationModel GetByRevaluationNo(string revaluationNo, long userId)
        {
            return RevaluationService.Instance.GetByRevaluationNo(revaluationNo, userId).ToModel<RevaluationModel>();
        }

        /// <summary>
        /// 获取复估项目
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="projectNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RevaluationItemModel GetRevaluationItem(string revaluationNo, string projectNo, long userId)
        {
            return RevaluationService.Instance.GetRevaluationItem(revaluationNo,projectNo,userId).ToModel<RevaluationItemModel>();
        }

        /// <summary>
        /// 保存扫描复估认可记录
        /// </summary>
        /// <param name="idList"></param>
        public void SaveScanProjectResult(IList<long> idList)
        {
            RevaluationService.Instance.SaveScanProjectResult(idList);
        }

        /// <summary>
        /// 批量复估保存
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="revalItems"></param>
        /// <param name="userId"></param>
        public void SaveBatchRevalProject(string revaluationNo, IList<RevaluationItemModel> revalItems, long userId)
        {
            RevaluationService.Instance.SaveBatchRevalProject(revaluationNo, revalItems.ToListModel<RevaluationItem, RevaluationItemModel>(), userId);
        }
    }
}
