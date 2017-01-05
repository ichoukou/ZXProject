using System.Collections.Generic;
using Peacock.ZXEval.Model.ApiModel;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;

namespace Peacock.ZXEval.DataAdapter.Interface
{
    public interface IProjectAdapter
    {
        /// <summary>
        /// 获取项目
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        ProjectModel GetProjectById(long projectId, long userId);

        /// <summary>
        /// 项目列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="total"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        IList<ProjectModel> GetProjectList(ProjectCondition condition, int index, int size, out int total, long userId);

        /// <summary>
        /// 项目受理
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool AcceptProject(long projectId, long userId, string note);

        /// <summary>
        /// 外业勘察
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool OperateOuterTask(long projectId, OuterTaskModel entity, long userId, string note);

        /// <summary>
        /// 报告预估
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool OperateReportEstimate(long projectId, SummaryDataModel entity, long userId, string note);

        /// <summary>
        /// 报告准备
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool OperateReportPrepare(long projectId, long userId, string note);

        ///// <summary>
        ///// 报告审核
        ///// </summary>
        ///// <param name="projectId"></param>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //bool OperateReportAudit(long projectId, long userId);

        /// <summary>
        /// 发送报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool OperateReportSend(long projectId, SummaryDataModel entity, long userId, string note);

        /// <summary>
        /// 根据业务编号获取项目
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        ProjectModel GetProject(string businessId);

        /// <summary>
        /// 接单
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="resources"></param>
        /// <returns></returns>
        ProjectModel ReceiveProject(ProjectRequest entity, IList<ProjectResourceRequest> resources, string companyName);
       
        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        void RevokeProject(long id, string reason);

        /// <summary>
        /// 下载资料
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        byte[] DownLoadFiles(long projectId, long userId);

        /// <summary>
        /// 导出汇总数据
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        byte[] DownLoadSummaryData(long projectId, long userId);

        /// <summary>
        /// 上传报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        /// <param name="userId"></param>
        /// <param name="reportType"></param>
        string UploadProjectResource(long projectId, string fileName, byte[] fileByte, long userId, ResourcesEnum reportType = ResourcesEnum.正式报告);

        /// <summary>
        /// 获取最新的上传报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="reportType"></param>
        ProjectResourceModel GetLastUploadReportResource(long projectId, ResourcesEnum reportType = ResourcesEnum.正式报告);

        /// <summary>
        /// 获取项目资源
        /// </summary>
        /// <param name="resourceId"></param>
        ProjectResourceModel GetResourceById(long resourceId);

        /// <summary>
        /// 保存扫描记录
        /// </summary>
        /// <param name="idList"></param>
        void SaveScanResult(IList<long> idList);

        /// <summary>
        /// 根据流水号获取项目
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        ProjectModel GetByProjectNo(string projectNo, long userId);
    }
}
