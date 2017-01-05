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
using InvalidDataException = System.IO.InvalidDataException;

namespace Peacock.ZXEval.WebApi.ApiControllers
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

                var project = JsonConvert.DeserializeObject<ProjectRequest>(requestFrom.bussinessForm);        
                if (project == null)
                {
                    throw new InvalidDataException("bussinessForm参数不能为空");
                }               
                project.BusinessId = requestFrom.bussinessId;


                var company = CompanyService.GetByCompanyId(project.CompanyId.HasValue?project.CompanyId.Value:0);
                if (company == null)
                {
                    throw new InvalidDataException("评估机构参数错误，找不到所属评估机构");
                }

                var resources = JsonConvert.DeserializeObject<IList<ProjectResourceRequest>>(requestFrom.files);

                var projectReturn = ProjectService.AcceptBusiness(project, resources);
                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = new { ProjectNo = projectReturn.ProjectNo };
             
            }
            catch (InvalidDataException ex)
            {
                responseResult.Code = 1;
                responseResult.Message =ex.Message;
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

                ProjectService.RevokeProject(project.Id);

                responseResult.Code = 0;
                responseResult.Message = "success";
                responseResult.Data = new { evalStatus = project.ProjectStatus};
                
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
                    company = result.Select(x => new {x.Id, x.CompanyName})
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
    }

}
