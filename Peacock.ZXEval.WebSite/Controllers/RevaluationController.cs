using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.WebSite.Help;

namespace Peacock.ZXEval.WebSite.Controllers
{
    public class RevaluationController : BaseController
    {
        /// <summary>
        /// 复估任务
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var revaluationStatus = new Dictionary<int, string>();
            foreach (var status in (RevaluationStatusEnum[])Enum.GetValues(typeof(RevaluationStatusEnum)))
            {
                revaluationStatus.Add((int)status, status.ToString());
            }
            ViewBag.Status = revaluationStatus;
            return View();
        }

        /// <summary>
        /// 开始复估
        /// </summary>
        /// <returns></returns>
        public ActionResult Revaluating(long revaluationId)
        {
            var revaluation = RevaluationService.GetRevaluationById(revaluationId, UserHelper.GetCurrentUser().Id);
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.未受理)
            {
                return Content("复估单未受理，无法进行复估");
            }
            //if (revaluation.RevaluationStatus == RevaluationStatusEnum.已撤单)
            //{
            //    return Content("复估单已撤单，无法进行复估");
            //}
            return View(revaluation);
        }

        /// <summary>
        /// 获取复估项
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GetRevaluationItemById(long id)
        {
            var item = RevaluationService.GetRevaluationItemById(id, UserHelper.GetCurrentUser().Id);
            return Json(new
            {
                item.ProtocolNumber,
                item.CustomerNumber,
                item.PledgeAddress,
                item.PropertyType,
                item.BorrowerName,
                InitialEstimateTime = item.InitialEstimateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                InitialEstimateValue = item.InitialEstimateValue.ToString("F2"),
                RevaluationTime =
                    item.RevaluationTime.HasValue ? item.RevaluationTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "",
                RevaluationValue = item.RevaluationValue.HasValue ? item.RevaluationValue.Value.ToString("F2") : "",
                item.OperationOrganization,
                item.CustomerAccountManager,
                item.ContactNumber,
                item.ChangeDescription,
                item.Remark,
                IsApprove = item.IsApprove.HasValue ? item.IsApprove.Value ? "是" : "否" : "",
                IsCancelConsult = item.IsCancelConsult.HasValue && item.IsCancelConsult.Value ? "是" : "否",
                item.ConsultReply
            });
        }

        /// <summary>
        /// 获取复估列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public JsonResult GetRevaluationList(RevaluationCondition condition, int index, int rows)
        {
            int total;
            var result = RevaluationService.GetRevaluationList(condition, index, rows, out total, UserHelper.GetCurrentUser().Id);
            return Json(new
            {
                rows = result.Select(x => new
                {
                    x.TId,
                    x.RevaluationNo,
                    x.RevaluationName,
                    x.EvalType,
                    RevaluationStatus = x.RevaluationStatus.ToString(),
                    CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", x.CreateTime),
                    x.canFinish
                }),
                total
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取复估列表
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="index"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public JsonResult GetRevaluationItemList(long revaluationId, int index, int rows)
        {
            int total;
            bool canFinish;
            var result = RevaluationService.GetRevaluationItemList(revaluationId, index, rows, out total, out canFinish,
                UserHelper.GetCurrentUser().Id);
            return Json(new
            {
                rows = result.Select(x => new
                {
                    x.TId,
                    x.ProjectNo,
                    x.PledgeAddress,
                    x.PropertyType,
                    x.BorrowerName,
                    InitialEstimateValue = x.InitialEstimateValue.ToString("F2"),
                    RevaluationTime =
                        x.RevaluationTime.HasValue ? string.Format("{0:yyyy-MM-dd HH:mm:ss}", x.RevaluationTime) : "",
                    RevaluationValue = x.RevaluationValue.HasValue ? x.RevaluationValue.Value.ToString("F2") : ""
                }),
                total,
                canFinish
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 复估单受理
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AcceptRevaluation(long id)
        {
            return ExceptionCatch.Invoke(() =>
            {
                RevaluationService.AcceptRevaluation(id, UserHelper.GetCurrentUser().Id);
            });
        }

        /// <summary>
        /// 单项复估
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RevaluatingRevaluationItem(RevaluationItemModel model)
        {
            return ExceptionCatch.Invoke(() =>
            {
                RevaluationService.RevaluatingRevaluationItem(model, UserHelper.GetCurrentUser().Id);
            });
        }

        /// <summary>
        /// 完成复估
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult FinishRevaluation(long id)
        {
            return ExceptionCatch.Invoke(() =>
            {
                RevaluationService.FinishRevaluation(id, UserHelper.GetCurrentUser().Id);
            });
        }

        /// <summary>
        /// 导出复估 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="revaluationNo"></param>
        /// <returns></returns>
        public ActionResult ExportRevaluation(long id, string revaluationNo)
        {
            var excel = RevaluationService.ExportRevaluation(id, UserHelper.GetCurrentUser().Id);
            return File(excel, "application/octet-stream", string.Format("导出{0}复估单_{1:yyyyMMddHHmmss}.xls", revaluationNo, DateTime.Now));
        }

        /// <summary>
        /// 导入复估结果
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UploadRevaluationResult(long id)
        {
            var files = Request.Files;
            HttpPostedFileBase file;
            //是否有文件
            if (files.Count > 0 && files[0] != null)
            {
                file = files[0];
            }
            else
            {
                return Json(new {msg = "请上传文件！"}, "text/html", JsonRequestBehavior.AllowGet);
            }
            var fileStream = file.InputStream;
            var fileName = file.FileName;
            var fileByte = new byte[file.ContentLength];
            fileStream.Read(fileByte, 0, file.ContentLength - 1);
            try
            {
                string result = RevaluationService.UploadRevaluationResult(id, fileName, fileByte,
                    UserHelper.GetCurrentUser().Id);
                //将返回设置为"text/html",解决ajaxSubmit 在IE8下不执行success，而是作为附件下载
                return Json(new {msg = result}, "text/html", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { msg = ex.Message }, "text/html", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取复估列表
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="index"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public JsonResult GetConsultList(ConsultCondition condition, int index, int rows)
        {
            int total;
            var result = RevaluationService.GetConsultList(condition, index, rows, out total, UserHelper.GetCurrentUser().Id);
            return Json(new
            {
                rows = result.Select(x => new
                {
                    x.TId,
                    x.ProjectNo,
                    x.PledgeAddress,
                    x.EvalType,
                    x.PropertyType,
                    InitialEstimateValue = x.InitialEstimateValue.ToString("F2"),
                    x.RevaluationValue,
                    x.RevaluationDifference,
                    x.RevaluationIncrease,
                    RevaluationTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", x.RevaluationTime),
                    IsApprove = x.IsApprove.HasValue ? x.IsApprove.Value ? "是" : "否" : "",
                    IsCancelConsult = x.IsCancelConsult.HasValue && x.IsCancelConsult.Value ? "是" : "否"
                }),
                total
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 复估认可
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult ApproveRevaluationItem(RevaluationItemModel model)
        {
            return ExceptionCatch.Invoke(() =>
            {
                RevaluationService.ApproveRevaluationItem(model, UserHelper.GetCurrentUser().Id);
            });
        }
    }
}
