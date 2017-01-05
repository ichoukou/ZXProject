using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting;
using System.Web.Http;
using Microsoft.Data.Edm.Csdl;
using Newtonsoft.Json;
using Peacock.Common.Exceptions;
using Peacock.Common.Helper;
using Peacock.ZXEval.Model.ApiModel;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.WebSite.Attributes;
using Peacock.ZXEval.WebSite.Help;
using InvalidDataException = System.IO.InvalidDataException;

namespace Peacock.ZXEval.WebSite.ApiControllers
{   
    [RoutePrefix("api/company")]
    [OAuthFilter]
    public class CompanyApiController : BaseApiController
    {
        #region 评估任务
        /// <summary>
        /// 公司评估列表
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="scanType"></param>
        /// <returns></returns>
        [Route("GetProjectList")]
        [HttpGet]
        public ResponseResult GetProjectList(string userKeyId, int scanType)
        {
            LogHelper.Error(string.Format("调用公司评估列表API,userKeyId:{0},scanType:{1}", userKeyId, scanType), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                int total = 0;
                var projectList = ProjectService.GetProjectList(new ProjectCondition() {ScanType = scanType}, 0,int.MaxValue, out total, user.Id);

                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = projectList.Select(x =>
                {
                    var attachFiles=x.ProjectResources.Where(y=>y.ResourcesType==ResourcesEnum.附件).ToList().Select(z=>new
                    {
                         z.FileFormat,
                         z.FileName,
                         z.FilePath
                    });

                    return new
                    {
                        x.ProjectNo,
                        x.PropertyType,
                        x.CreditCustomer,
                        x.CreditAmount,
                        x.PledgeAddress,
                        x.AccountManagerTel,
                        x.EvalRange,
                        x.PledgePerson,
                        x.EvalContractPerson,
                        x.EvalContractTel,
                        x.HouseCertInfo,
                        x.LandCertInfo,
                        x.LoanPerson,
                        x.LoanTel,
                        x.IsFirstCredit,
                        x.CreditVariety,
                        x.EvalType,
                        AttachFiles = attachFiles
                    };
                });
                IList<long> idList = projectList.Where(x => !x.IsScan.HasValue && !x.ScanTime.HasValue).Select(x=>x.Id).ToList();
                if (idList.Any())
                {
                    ProjectService.SaveScanResult(idList);
                }
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用公司评估列表API错误日志:", ex);
            }
            return responseResult;
        }
         
        /// <summary>
        /// 获取项目详细信息
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        [Route("GetProjecDetail")]
        [HttpGet]
        public ResponseResult GetProjecDetail(string userKeyId, string projectNo)
        {
            LogHelper.Error(string.Format("调用项目详细信息API,userKeyId:{0},projectNo:{1}", userKeyId, projectNo), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = ProjectService.GetByProjectNo(projectNo, user.Id);               
                var attachFiles=project.ProjectResources.Where(x=>x.ResourcesType==ResourcesEnum.附件).ToList().Select(x=>new
                {
                     x.FileFormat,
                     x.FileName,
                     x.FilePath
                });

                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = new
                {
                    project.ProjectNo,
                    project.PropertyType,
                    project.CreditCustomer,
                    project.CreditAmount,
                    project.PledgeAddress,
                    project.AccountManagerTel,
                    project.EvalRange,
                    project.PledgePerson,
                    project.EvalContractPerson,
                    project.EvalContractTel,
                    project.HouseCertInfo,
                    project.LandCertInfo,
                    project.LoanPerson,
                    project.LoanTel,
                    project.IsFirstCredit,
                    project.CreditVariety,
                    project.EvalType,
                    AttachFiles=attachFiles
                };
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用项目详细信息API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 下载附件
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        [Route("DownLoadFile")]
        [HttpGet]
        public HttpResponseMessage DownLoadFile(string userKeyId,  string projectNo)
        {
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = ProjectService.GetByProjectNo(projectNo, user.Id);
                byte[] bytes = ProjectService.DownLoadFiles(project.Id, user.Id);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(bytes));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = string.Format("附件{0}_{1:yyyyMMddHHmmss}.zip", projectNo, DateTime.Now)
                };
                return response;
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// 项目受理
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("AcceptProject")]
        [HttpPost]
        public ResponseResult AcceptProject(string userKeyId, [FromBody]ProjectBaseRequest request)
        {
            LogHelper.Error(string.Format("调用项目受理API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = ProjectService.GetByProjectNo(request.ProjectNo,user.Id);
                ProjectService.AcceptProject(project.Id, user.Id,request.Note);
                responseResult.Code = 0;
                responseResult.Message = "success";                
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用项目受理API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 外业勘察
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("OuterTask")]
        [HttpPost]
        public ResponseResult OuterTask(string userKeyId, [FromBody]OuterTaskRequest request)
        {
            LogHelper.Error(string.Format("调用外业勘察API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = ProjectService.GetByProjectNo(request.ProjectNo, user.Id);
                var outerTask = new OuterTaskModel(){ AppointmentDate = request.AppointmentDate, FinishDate = request.FinishDate ,CreateTime = DateTime.Now};
                ProjectService.OperateOuterTask(project.Id, outerTask, user.Id, request.Note);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用外业勘察API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 报告预估
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("ReportEstimate")]
        [HttpPost]
        public ResponseResult ReportEstimate(string userKeyId, [FromBody]SummaryDataRequest request)
        {
            LogHelper.Error(string.Format("调用报告预估API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = ProjectService.GetByProjectNo(request.ProjectNo, user.Id);
                //预估报告
                byte[] resourceData;
                if (string.IsNullOrEmpty(request.ResourceUrl))
                {
                    throw new ServiceException("预估报告文件不能为空");
                }
                else
                {
                    resourceData = (new WebClient()).DownloadData(request.ResourceUrl);
                    if (resourceData.Length == 0)
                    {
                        throw new ServiceException("无法获取预估报告文件");
                    }
                    else
                    {
                        string fileName = System.IO.Path.GetFileName(request.ResourceUrl);

                        string result = ProjectService.UploadProjectResource(project.Id, fileName, resourceData, user.Id, ResourcesEnum.预估报告);
                        if (result != "上传成功!")
                        {
                            throw new ServiceException(result);
                        }
                    }
                }
                //保存
                var sourceType = request.GetType(); //源对象
                var sourcePropertys = sourceType.GetProperties();
                var data = new SummaryDataModel();//赋值对象
                var targetType = data.GetType();
                foreach (var property in targetType.GetProperties())
                {
                    if (!sourcePropertys.Select(x=>x.Name).Contains(property.Name))
                        continue;
                    var value = sourceType.GetProperty(property.Name).GetValue(request, null);
                    property.SetValue(data, value, null); 
                }
                ProjectService.OperateReportEstimate(project.Id, data, user.Id, request.Note);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用报告预估API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 报告准备
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("ReportPrepare")]
        [HttpPost]
        public ResponseResult ReportPrepare(string userKeyId, [FromBody]ProjectBaseRequest request)
        {
            LogHelper.Error(string.Format("调用报告准备API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = ProjectService.GetByProjectNo(request.ProjectNo, user.Id);
                ProjectService.OperateReportPrepare(project.Id, user.Id, request.Note);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用报告准备API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 报告发送
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("ReportSend")]
        [HttpPost]
        public ResponseResult ReportSend(string userKeyId, [FromBody]SummaryDataRequest request)
        {
            LogHelper.Error(string.Format("调用报告发送API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = ProjectService.GetByProjectNo(request.ProjectNo, user.Id);
                //正式报告
                byte[] resourceData;
                if (string.IsNullOrEmpty(request.ResourceUrl))
                {
                    throw new ServiceException("正式报告文件不能为空");
                }
                else
                {
                    resourceData = (new WebClient()).DownloadData(request.ResourceUrl);
                    if (resourceData.Length == 0)
                    {
                        throw new ServiceException("无法获取正式报告文件");
                    }
                    else
                    {
                        string fileName = System.IO.Path.GetFileName(request.ResourceUrl);
                        string result = ProjectService.UploadProjectResource(project.Id, fileName, resourceData, user.Id, ResourcesEnum.正式报告);
                        if (result != "上传成功!")
                        {
                            throw new ServiceException(result);
                        }
                    }
                }
                //保存
                var sourceType = request.GetType(); //源对象
                var sourcePropertys = sourceType.GetProperties();
                var data = new SummaryDataModel();  //赋值对象
                var targetType = data.GetType();
                var exceptPropertys = new string[] {"EstimateUnitPrice", "EstimateTotalPrice"};//剔除预估信息
                foreach (var property in targetType.GetProperties())
                {
                    if (!sourcePropertys.Select(x => x.Name).Contains(property.Name) || exceptPropertys.Contains(property.Name))
                        continue;
                    var value = sourceType.GetProperty(property.Name).GetValue(request, null);
                    property.SetValue(data, value, null);
                }
                ProjectService.OperateReportSend(project.Id, data, user.Id, request.Note);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用报告发送API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 导出汇总数据
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        [Route("ExportSummaryData")]
        [HttpGet]
        public HttpResponseMessage ExportSummaryData(string userKeyId, string projectNo)
        {
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = ProjectService.GetByProjectNo(projectNo, user.Id);

                byte[] bytes = ProjectService.DownLoadSummaryData(project.Id, user.Id);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(bytes));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = string.Format("汇总数据{0}_{1:yyyyMMddHHmmss}.xls", projectNo, DateTime.Now)
                };
                return response;
            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
        }
        #endregion

        #region 复估任务
        /// <summary>
        /// 公司复估单列表
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="scanType"></param>
        /// <returns></returns>
        [Route("GetRevaluationList")]
        [HttpGet]
        public ResponseResult GetRevaluationList(string userKeyId, int scanType)
        {
            LogHelper.Error(string.Format("调用公司复估单列表API,userKeyId:{0},scanType:{1}", userKeyId, scanType), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                int total = 0;
                var revaluationList = RevaluationService.GetRevaluationList4Api(new RevaluationCondition() { ScanType = scanType }, 0, int.MaxValue, out total, user.Id);
                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = revaluationList.Select(x =>
                {
                    var items = x.RevaluationItems.Select(y => new {
                        y.TId,
                        x.BusinessId,
                        y.RevaluationId,
                        y.OperationOrganization,
                        y.BorrowerName,
                        y.CreditBalance,
                        y.FiveCategories,
                        y.ContractExpirationDate,
                        y.PropertyType,
                        y.PledgeAddress,
                        y.InitialEstimateValue,
                        y.InitialEstimateOrganization,
                        y.InitialEstimateTime,
                        y.CustomerAccountManager,
                        y.ContactNumber,
                        y.ProtocolNumber,
                        y.CustomerNumber,
                        y.ProjectNo,
                        y.RevaluationTime,
                        y.RevaluationValue,
                        y.ChangeDescription,
                        y.Remark,
                        y.RevaluationDifference,
                        y.RevaluationIncrease,
                        y.IsConsult,
                        y.IsCancelConsult,
                        y.IsApprove,
                        y.ConsultReply,
                        y.CreateTime
                    }).ToList();
                    return new
                    {
                        x.TId,
                        x.BusinessId ,
                        x.RevaluationNo,
                        x.EvalType,
                        x.RevaluationName,
                        x.RevaluationStatus ,
                        x.CreateTime,
                        RevaluationItems = items
                    };
                });
                IList<long> idList = revaluationList.Where(x => !x.IsScan.HasValue && !x.ScanTime.HasValue).Select(x => x.TId).ToList();
                if (idList.Any())
                {
                    RevaluationService.SaveScanResult(idList);
                }
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用公司复估单列表API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 获取复估单详细信息
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="revaluationNo"></param>
        /// <returns></returns>
        [Route("GetRevaluationDetail")]
        [HttpGet]
        public ResponseResult GetRevaluationDetail(string userKeyId, string revaluationNo)
        {
            LogHelper.Error(string.Format("调用复估单详细信息API,userKeyId:{0},revaluationNo:{1}", userKeyId, revaluationNo), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var revaluation = RevaluationService.GetByRevaluationNo(revaluationNo, user.Id);
                var items = revaluation.RevaluationItems.Select(x => new
                {
                   x.TId,
                   x.BusinessId,
                   x.RevaluationId,
                   x.OperationOrganization,
                   x.BorrowerName,
                   x.CreditBalance,
                   x.FiveCategories,
                   x.ContractExpirationDate,
                   x.PropertyType,
                   x.PledgeAddress,
                   x.InitialEstimateValue,
                   x.InitialEstimateOrganization,
                   x.InitialEstimateTime,
                   x.CustomerAccountManager,
                   x.ContactNumber,
                   x.ProtocolNumber,
                   x.CustomerNumber,
                   x.ProjectNo,
                   x.RevaluationTime,
                   x.RevaluationValue,
                   x.ChangeDescription,
                   x.Remark,
                   x.RevaluationDifference,
                   x.RevaluationIncrease,
                   x.IsConsult,
                   x.IsCancelConsult,
                   x.IsApprove,
                   x.ConsultReply,
                   x.CreateTime
                }).ToList();
                
                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = new
                {
                    revaluation.TId,
                    revaluation.BusinessId,
                    revaluation.RevaluationNo,
                    revaluation.EvalType,
                    revaluation.RevaluationName,
                    revaluation.RevaluationStatus,
                    revaluation.CreateTime,
                    RevaluationItems = items
                };
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用公司复估单详细信息API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 复估单受理
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("AcceptRevaluation")]
        [HttpPost]
        public ResponseResult AcceptRevaluation(string userKeyId, [FromBody]RevaluationBaseRequest request)
        {
            LogHelper.Error(string.Format("调用复估单受理API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var revaluation = RevaluationService.GetByRevaluationNo(request.RevalutionNo, user.Id);
                RevaluationService.AcceptRevaluation(revaluation.TId, user.Id);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用复估单受理API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 完成复估单
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("FinishRevaluation")]
        [HttpPost]
        public ResponseResult FinishRevaluation(string userKeyId, [FromBody]RevaluationBaseRequest request)
        {
            LogHelper.Error(string.Format("调用完成复估单API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var revaluation = RevaluationService.GetByRevaluationNo(request.RevalutionNo, user.Id);
                RevaluationService.FinishRevaluation(revaluation.TId, user.Id);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用完成复估单API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 复估单项目导出
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="revalutionNo"></param>
        /// <returns></returns>
        [Route("ExportRevaluation")]
        [HttpGet]
        public HttpResponseMessage ExportRevaluation(string userKeyId, string revalutionNo)
        {
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var revaluation = RevaluationService.GetByRevaluationNo(revalutionNo, user.Id);
                var bytes = RevaluationService.ExportRevaluation(revaluation.TId, user.Id);
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StreamContent(new MemoryStream(bytes));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = string.Format("复估单{0}_{1:yyyyMMddHHmmss}.xls", revalutionNo, DateTime.Now)
                };
                return response;
            }
            catch(Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// 获取复估项目详细信息
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="revaluationNo"></param>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        [Route("GetRevaluationProject")]
        [HttpGet]
        public ResponseResult GetRevaluationProject(string userKeyId, string revaluationNo, string projectNo)
        {
            LogHelper.Error(string.Format("调用复估项目详细信息API,userKeyId:{0},revaluationNo:{1}", userKeyId, revaluationNo), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = RevaluationService.GetRevaluationItem(revaluationNo, projectNo, user.Id);
                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = new
                {
                    project.TId,
                    project.BusinessId,
                    project.RevaluationId,
                    project.OperationOrganization,
                    project.BorrowerName,
                    project.CreditBalance,
                    project.FiveCategories,
                    project.ContractExpirationDate,
                    project.PropertyType,
                    project.PledgeAddress,
                    project.InitialEstimateValue,
                    project.InitialEstimateOrganization,
                    project.InitialEstimateTime,
                    project.CustomerAccountManager,
                    project.ContactNumber,
                    project.ProtocolNumber,
                    project.CustomerNumber,
                    project.ProjectNo,
                    project.RevaluationTime,
                    project.RevaluationValue,
                    project.ChangeDescription,
                    project.Remark,
                    project.RevaluationDifference,
                    project.RevaluationIncrease,
                    project.IsConsult,
                    project.IsCancelConsult,
                    project.IsApprove,
                    project.ConsultReply,
                    project.CreateTime
                };
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用公司复估项目详细信息API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 复估单个项目
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("RevaluatingProject")]
        [HttpPost]
        public ResponseResult RevaluatingProject(string userKeyId, [FromBody]RevaluatingProjectRequest request)
        {
            LogHelper.Error(string.Format("调用复估单个项目API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                if (!request.RevaluationValue.HasValue)
                {
                    throw new ServiceException("复估价值不能为空！");
                }
                if (string.IsNullOrEmpty(request.ChangeDescription))
                {
                    throw new ServiceException("变动说明不能为空！");
                }
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = RevaluationService.GetRevaluationItem(request.RevalutionNo, request.ProjectNo, user.Id);
                var model = new RevaluationItemModel()
                {
                    TId = project.TId,
                    RevaluationValue = request.RevaluationValue,
                    ChangeDescription = request.ChangeDescription,
                    Remark = request.Remark
                };
                RevaluationService.RevaluatingRevaluationItem(model, user.Id);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用复估单个项目API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 批量复估项目
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("BatchRevalProject")]
        [HttpPost]
        public ResponseResult BatchRevalProject(string userKeyId, [FromBody]RevaluatingProjectBatchRequest request)
        {
            LogHelper.Error(string.Format("调用批量复估项目API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                if (string.IsNullOrEmpty(request.RevalutionNo))
                {
                    throw new ServiceException("复估单不能为空");
                }
                if (!request.RevaluationItems.Any())
                {
                    throw new ServiceException("复估项目不能为空");
                }
                var revaluationItems = new List<RevaluationItemModel>();
                foreach (var item in request.RevaluationItems)
                {
                    revaluationItems.Add(new RevaluationItemModel()
                    {
                        ProjectNo = item.ProjectNo,
                        RevaluationValue=item.RevaluationValue,
                        ChangeDescription = item.ChangeDescription,
                        Remark = item.Remark
                    });
                }
                RevaluationService.SaveBatchRevalProject(request.RevalutionNo, revaluationItems, user.Id);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用批量复估项目API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 公司复估认可列表
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="scanType"></param>
        /// <returns></returns>
        [Route("GetConsultList")]
        [HttpGet]
        public ResponseResult GetConsultList(string userKeyId, int scanType)
        {
            LogHelper.Error(string.Format("调用公司复估认可列表API,userKeyId:{0},scanType:{1}", userKeyId, scanType), null);
            var responseResult = new ResponseResult();
            try
            {
                var user = UserService.GetUserByKeyId(userKeyId);
                int total = 0;
                var projectList = RevaluationService.GetConsultList(new ConsultCondition() { ScanType = scanType }, 0, int.MaxValue, out total, user.Id);
                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = projectList.Select(x => new
                {
                    x.TId,
                    x.BusinessId,
                    x.RevaluationId,
                    x.OperationOrganization,
                    x.BorrowerName,
                    x.CreditBalance,
                    x.FiveCategories,
                    x.ContractExpirationDate,
                    x.PropertyType,
                    x.PledgeAddress,
                    x.InitialEstimateValue,
                    x.InitialEstimateOrganization,
                    x.InitialEstimateTime,
                    x.CustomerAccountManager,
                    x.ContactNumber,
                    x.ProtocolNumber,
                    x.CustomerNumber,
                    x.ProjectNo,
                    x.RevaluationTime,
                    x.RevaluationValue,
                    x.ChangeDescription,
                    x.Remark,
                    x.RevaluationDifference,
                    x.RevaluationIncrease,
                    x.IsConsult,
                    x.IsCancelConsult,
                    x.IsApprove,
                    x.ConsultReply,
                    x.CreateTime
                });
                IList<long> idList = projectList.Where(x => !x.IsScan.HasValue && !x.ScanTime.HasValue).Select(x => x.TId).ToList();
                if (idList.Any())
                {
                    RevaluationService.SaveScanProjectResult(idList);
                }
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用公司复估认可列表API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 复估认可
        /// </summary>
        /// <param name="userKeyId"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Route("RevaluationConsult")]
        [HttpPost]
        public ResponseResult RevaluationConsult(string userKeyId, [FromBody]RevaluationConsultRequest request)
        {
            LogHelper.Error(string.Format("调用复估认可API,userKeyId:{0},request:{1}", userKeyId, request.ToJson()), null);
            var responseResult = new ResponseResult();
            try
            {
                if (!request.IsApprove.HasValue)
                {
                    throw new ServiceException("是否认可不能为空！");
                }
                if (string.IsNullOrEmpty(request.ConsultReply))
                {
                    throw new ServiceException("认可理由不能为空！");
                }
                var user = UserService.GetUserByKeyId(userKeyId);
                var project = RevaluationService.GetRevaluationItem(request.RevalutionNo, request.ProjectNo, user.Id);
                var model = new RevaluationItemModel()
                {
                    TId = project.TId,
                    IsApprove = request.IsApprove,
                    ConsultReply = request.ConsultReply
                };
                RevaluationService.ApproveRevaluationItem(model, user.Id);
                responseResult.Code = 0;
                responseResult.Message = "success";
            }
            catch (ServiceException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用复估认可API错误日志:", ex);
            }
            return responseResult;
        }
        #endregion
    }

}
