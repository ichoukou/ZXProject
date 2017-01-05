using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Peacock.Common.Exceptions;
using Peacock.Common.Helper;
using Peacock.ZXEval.Model.ApiModel;
using Peacock.ZXEval.Model.DTO;
using InvalidDataException = System.IO.InvalidDataException;

namespace Peacock.ZXEval.WebSite.ApiControllers
{
    [RoutePrefix("api/project")]
    public class ProjectApiController : BaseApiController
    {
        /// <summary>
        /// 接单
        /// </summary>
        /// <param name="requestFrom"></param>
        /// <returns></returns>
        [Route("add")]
        [HttpPost]
        public ResponseResult AcceptProject(DelegateRequest requestFrom)
        {
            LogHelper.Error("调用接单API:" + requestFrom.ToJson(), null);
            var responseResult = new ResponseResult();
            try
            {
                if (string.IsNullOrEmpty(requestFrom.bussinessId))
                {
                    throw new InvalidDataException("必填项不能为空");
                }

                var project = requestFrom.bussinessForm;
                if (project == null)
                {
                    throw new InvalidDataException("bussinessForm参数不能为空");
                }
                project.BusinessId = requestFrom.bussinessId;
                //if (!project.CompanyId.HasValue)
                //{
                //    throw new InvalidDataException("CompanyId参数不能为空");
                //}

                IList<ProjectResourceRequest> resources = requestFrom.files;
                if (requestFrom.files == null)
                {
                    resources=new List<ProjectResourceRequest>();
                }
                var projectReturn = ProjectService.ReceiveProject(project, resources, requestFrom.bussinessForm.CompanyName);
                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = new { ProjectNo = projectReturn.ProjectNo };

            }
            catch (InvalidDataException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "bussinessForm,files参数为空或者格式不正确";
                LogHelper.Error("调用接单API错误日志:", ex);
            }
            return responseResult;
        }


        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="request">流水号/业务编号</param>
        [Route("revoke")]
        [HttpPost]
        public ResponseResult RevokeProject(RevokeRequest request)
        {
            LogHelper.Error("调用撤单API:" + request.ToJson(), null);
            var responseResult = new ResponseResult();
            try
            {
                if (string.IsNullOrEmpty(request.bussinessId))
                {
                    throw new InvalidDataException("流水号/业务编号不能为空");
                }
                var project = ProjectService.GetProject(request.bussinessId);
                if (project == null)
                {
                    throw new InvalidDataException("评估项目未找到");
                }

                ProjectService.RevokeProject(project.Id, request.reason);

                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = new { evalStatus = ((int)project.ProjectStatus).ToString() };

            }
            catch (InvalidDataException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用撤单API错误日志:", ex);
            }
            return responseResult;
        }


        /// <summary>
        ///  获取评估公司列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getcompany")]
        public ResponseResult GetCompanyList(CompanyRequest request)
        {
            LogHelper.Error("调用获取评估公司API:" + request.ToJson(), null);
            var responseResult = new ResponseResult();
            try
            {
                if (string.IsNullOrEmpty(request.evalType))
                {
                    throw new InvalidDataException("业务类型不能为空");
                }

                var result = CompanyService.GetCompanyList(request.evalType);
                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = new
                {
                    company = result.Select(x => new { x.Id, x.CompanyName })
                };
            }
            catch (InvalidDataException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = "error";
                LogHelper.Error("调用获取评估公司API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 接复估单
        /// </summary>
        /// <param name="requestFrom"></param>
        /// <returns></returns>
        [Route("revaluate")]
        [HttpPost]
        public ResponseResult ReceiveRevaluation(RevaluateRequest requestFrom)
        {
            LogHelper.Error("调用接复估单API:" + requestFrom.ToJson(), null);
            var responseResult = new ResponseResult();
            try
            {
                if (string.IsNullOrEmpty(requestFrom.bussinessId))
                {
                    throw new InvalidDataException("复估单bussinessId不能为空!");
                }

                if (string.IsNullOrEmpty(requestFrom.CompanyName))
                {
                    throw new InvalidDataException("复估单CompanyName不能为空!");
                }

                var revaluate = new RevaluationModel()
                {
                    BusinessId = requestFrom.bussinessId,
                    EvalType = requestFrom.EvalType,
                    RevaluationName = requestFrom.RevaluationName
                };
                List<RevaluationItemRequest> items = requestFrom.RevaluationItems;
                if (!items.Any())
                {
                    throw new InvalidDataException("复估项RevaluationItems不能为空!");
                }
                decimal tryDecimal;
                DateTime tryDateTime;
                if (items.Any(x => string.IsNullOrEmpty(x.BussinessId)))
                {
                    throw new InvalidDataException("复估项RevaluationItems中包括BussinessId为空的项目!");
                }
                if (items.Any(x => !decimal.TryParse(x.CreditBalance, out tryDecimal)))
                {
                    throw new InvalidDataException("复估项RevaluationItems中存在【授信余额】格式不正确!");
                }
                if (items.Any(x => !DateTime.TryParse(x.ContractExpirationDate, out tryDateTime)))
                {
                    throw new InvalidDataException("复估项RevaluationItems中存在【合同到期日】格式不正确!");
                }
                if (items.Any(x => !decimal.TryParse(x.InitialEstimateValue, out tryDecimal)))
                {
                    throw new InvalidDataException("复估项RevaluationItems中存在【初估价值(元)】格式不正确!");
                }
                if (items.Any(x => !DateTime.TryParse(x.InitialEstimateTime, out tryDateTime)))
                {
                    throw new InvalidDataException("复估项RevaluationItems中存在【初估时间】格式不正确!");
                }
                string revaluationNo;
                var result = RevaluationService.ReceiveRevaluation(revaluate, items, requestFrom.CompanyName,
                    requestFrom.IsComplete, out revaluationNo);
                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data =
                    new
                    {
                        revaluationNo,
                        projectNos = result.Select(x => new {BusinessId = x.Key, ProjectNo = x.Value})
                    };
            }
            catch (InvalidDataException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用接复估单API错误日志:", ex);
            }
            LogHelper.Error("接复估单返回结果:" + responseResult.ToJson(), null);
            return responseResult;
        }

        /// <summary>
        /// 复估价格异议
        /// </summary>
        /// <param name="requestFrom"></param>
        /// <returns></returns>
        [Route("consult")]
        [HttpPost]
        public ResponseResult ConsultRevaluation(ConsultRequest requestFrom)
        {
            LogHelper.Error("调用复估价格异议API:" + requestFrom.ToJson(), null);
            var responseResult = new ResponseResult();
            try
            {
                if (string.IsNullOrEmpty(requestFrom.BussinessId))
                {
                    throw new InvalidDataException("BussinessId不能为空");
                }
                var result = RevaluationService.ConsultRevaluation(requestFrom.BussinessId);
                responseResult.Code = result ? 0 : 1;
                responseResult.Message = result ? "success" : "unsuccess";
            }
            catch (InvalidDataException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用复估价格异议API错误日志:", ex);
            }
            return responseResult;
        }

        /// <summary>
        /// 撤销复估价格异议
        /// </summary>
        /// <param name="requestFrom"></param>
        /// <returns></returns>
        [Route("cancelconsult")]
        [HttpPost]
        public ResponseResult CancelConsultRevaluation(ConsultRequest requestFrom)
        {
            LogHelper.Error("调用撤销复估价格异议API:" + requestFrom.ToJson(), null);
            var responseResult = new ResponseResult();
            try
            {
                var result = RevaluationService.CancelConsultRevaluation(requestFrom.BussinessId);
                responseResult.Code = result ? 0 : 1;
                responseResult.Message = result ? "success" : "unsuccess";
            }
            catch (InvalidDataException ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                responseResult.Code = 1;
                responseResult.Message = ex.Message;
                LogHelper.Error("调用撤销复估价格异议API错误日志:", ex);
            }
            return responseResult;
        }
    }
}
