using System.Collections.Generic;
using Peacock.ZXEval.Data.Entities;
using Peacock.ZXEval.DataAdapter.Interface;
using Peacock.ZXEval.Model.ApiModel;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.Service;

namespace Peacock.ZXEval.DataAdapter.Implement
{
    public class ProjectAdapter : IProjectAdapter
    {
        /// <summary>
        /// 获取项目
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ProjectModel GetProjectById(long projectId, long userId)
        {
            return ProjectService.Instance.GetProjectById(projectId, userId).ToModel<ProjectModel>();
        }

        /// <summary>
        /// 项目列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IList<ProjectModel> GetProjectList(ProjectCondition condition, int index, int size, out int total, long userId)
        {
            return
                ProjectService.Instance.GetProjectList(condition, index, size, out total, userId)
                    .ToListModel<ProjectModel, Project>();
        }

        /// <summary>
        /// 项目受理
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool AcceptProject(long projectId, long userId, string note)
        {
            return ProjectService.Instance.AcceptProject(projectId, userId, note);
        }

        /// <summary>
        /// 外业勘察
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool OperateOuterTask(long projectId, OuterTaskModel entity, long userId, string note)
        {
            return ProjectService.Instance.OperateOuterTask(projectId, entity.ToModel<OuterTask>(), userId, note);
        }

        /// <summary>
        /// 报告预估
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool OperateReportEstimate(long projectId, SummaryDataModel entity, long userId, string note)
        {
            return ProjectService.Instance.OperateReportEstimate(projectId, entity.ToModel<SummaryData>(), userId, note);
        }

        /// <summary>
        /// 报告准备
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool OperateReportPrepare(long projectId, long userId, string note)
        {
            return ProjectService.Instance.OperateReportPrepare(projectId, userId, note);
        }

        ///// <summary>
        ///// 报告审核
        ///// </summary>
        ///// <param name="projectId"></param>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //public bool OperateReportAudit(long projectId, long userId)
        //{
        //    return ProjectService.Instance.OperateReportAudit(projectId, userId);
        //}

        /// <summary>
        /// 发送报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool OperateReportSend(long projectId, SummaryDataModel entity, long userId, string note)
        {
            return ProjectService.Instance.OperateReportSend(projectId, entity.ToModel<SummaryData>(), userId, note);
        }


        /// <summary>
        /// 根据业务编号获取项目
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        public ProjectModel GetProject(string businessId)
        {
            return ProjectService.Instance.GetProject(businessId).ToModel<ProjectModel>();
        }

        /// <summary>
        /// 接单
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
        public ProjectModel ReceiveProject(ProjectRequest entity, IList<ProjectResourceRequest> resources, string companyName)
        {
            return
                ProjectService.Instance.ReceiveProject(entity.ToModel<Project>(),
                    resources.ToListModel<ProjectResource, ProjectResourceRequest>(), companyName)
                    .ToModel<ProjectModel>();
        }

        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        public void RevokeProject(long id, string reason)
        {
            ProjectService.Instance.RevokeProject(id, reason);
        }

        /// <summary>
        /// 下载资料
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public byte[] DownLoadFiles(long projectId, long userId)
        {
            return ProjectService.Instance.DownLoadFiles(projectId, userId);
        }

        /// <summary>
        /// 导出汇总数据
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public byte[] DownLoadSummaryData(long projectId, long userId)
        {
            return ProjectService.Instance.DownLoadSummaryData(projectId, userId);
        }

        /// <summary>
        /// 上传报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        /// <param name="userId"></param>
        /// <param name="reportType"></param>
        public string UploadProjectResource(long projectId, string fileName, byte[] fileByte, long userId, Model.DTO.ResourcesEnum reportType = Model.DTO.ResourcesEnum.正式报告)
        {
            return ProjectService.Instance.UploadProjectResource(projectId, fileName, fileByte, userId,
                reportType.ToModel<Data.Entities.ResourcesEnum>());
        }

        /// <summary>
        /// 获取最新的上传报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="reportType"></param>
        public ProjectResourceModel GetLastUploadReportResource(long projectId, Model.DTO.ResourcesEnum reportType = Model.DTO.ResourcesEnum.正式报告)
        {
            return ProjectService.Instance.GetLastUploadReportResource(projectId, reportType.ToModel<Data.Entities.ResourcesEnum>()).ToModel<ProjectResourceModel>();
        }

        /// <summary>
        /// 获取项目资源
        /// </summary>
        /// <param name="resourceId"></param>
        public ProjectResourceModel GetResourceById(long resourceId)
        {
            return ProjectService.Instance.GetResourceById(resourceId).ToModel<ProjectResourceModel>();
        }

        /// <summary>
        /// 保存扫描记录
        /// </summary>
        /// <param name="idList"></param>
        public void SaveScanResult(IList<long> idList)
        {
            ProjectService.Instance.SaveScanResult(idList);
        }

        /// <summary>
        /// 根据流水号获取项目
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ProjectModel GetByProjectNo(string projectNo, long userId)
        {
            return ProjectService.Instance.GetByProjectNo(projectNo, userId).ToModel<ProjectModel>();
        }
    }
}
