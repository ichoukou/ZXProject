using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Peacock.Common.Exceptions;
using Peacock.Common.Helper;
using Peacock.ZXEval.Data.Entities;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Repository.Repositories;
using Peacock.ZXEval.Service.API;
using Peacock.ZXEval.Service.ApiModle;
using Peacock.ZXEval.Service.Base;
using RestSharp.Extensions;

namespace Peacock.ZXEval.Service
{
    public class RevaluationService : SingModel<RevaluationService>
    {
        private RevaluationService()
        {
             
        }
        /// <summary>
        /// 接复估单
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="items"></param>
        /// <param name="companyName"></param>
        /// <param name="isComplete"></param>
        /// <param name="revaluationNo"></param>
        /// <returns></returns>
        public Dictionary<string, string> ReceiveRevaluation(Revaluation entity, List<RevaluationItem> items, string companyName, bool isComplete, out string revaluationNo)
        {
            var result = new Dictionary<string, string>();
            var isFirst = false;
            var businessIds = items.Select(y => y.BusinessId);
            if (RevaluationItemRepository.Instance.Source.Any(x => businessIds.Contains(x.BusinessId)))
            {
                throw new ServiceException("已存在重复BusinessId的复估项，请勿重复推送");
            }
            var company = CompanyRepository.Instance.Source.FirstOrDefault(x => x.CompanyName == companyName);
            if (company == null)
            {
                company = CompanyService.Instance.GetByCompanyId(entity.CompanyId);
                if (company == null)
                {
                    throw new ServiceException("评估机构参数错误，找不到所属评估机构");
                }
            }
            var revaluation =
                RevaluationRepository.Instance.Source.FirstOrDefault(x => x.BusinessId == entity.BusinessId);
            if (revaluation == null)
            {
                isFirst = true;
                revaluation = entity;
                revaluation.Company = company;
                revaluation.CreateTime = DateTime.Now;
                revaluation.RevaluationStatus = RevaluationStatusEnum.未受理;
                revaluation.RevaluationNo = GetInstanceNo();
            }
            else if (revaluation.RevaluationStatus > RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("对不起，该复估单已接收完并可能已经开始作业!");
            }
            if (isFirst)
            {
                revaluation=RevaluationRepository.Instance.InsertReturnEntity(revaluation);
            }
            revaluationNo = revaluation.RevaluationNo;
            var projectNos = ProjectService.GetInstanceNo(items.Count(x => string.IsNullOrEmpty(x.ProjectNo)));
            var count = 0;
            foreach (var item in items)
            {
                item.RevaluationId = revaluation.TId;
                if (string.IsNullOrWhiteSpace(item.ProjectNo))
                {
                    item.ProjectNo = projectNos[count];
                    count++;
                }
                item.IsConsult = false;
                item.CreateTime = DateTime.Now;
                result.Add(item.BusinessId, item.ProjectNo);
            }
            if (!BatchInsertRevaluationItem(items))
            {
                throw new ServiceException("对不起，复估单接收失败");
            }
            revaluation.RevaluationStatus = isComplete ? RevaluationStatusEnum.未受理 : RevaluationStatusEnum.未接收完;
            RevaluationRepository.Instance.Save(revaluation);
            if (revaluation.TId > 0 && isComplete)
            {
                Task.Factory.StartNew(() =>
                {
                    var user = UserRepository.Instance.Source.FirstOrDefault(x => x.CompanyId == company.Id);
                    if (user == null)
                    {
                        LogHelper.Error("【" + companyName + "】没有默认的用户，无法发送短信", null);
                    }
                    else if (string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        LogHelper.Error("【" + companyName + "】用户" + "【" + user.UserName + "】没有配置手机号码，无法发送短信", null);
                    }
                    else
                    {
                        new EciticApiService().SendSms(new ApiModleSmsRequest()
                        {
                            teleno = user.PhoneNumber,
                            msg =
                                string.Format("【{0}】复估业务在{1}已发送至贵方系统，请尽快处理。", revaluation.RevaluationName,
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        });
                    }
                });
            }
            return result;
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
        public IList<Revaluation> GetRevaluationList(RevaluationCondition condition, int index, int size, out int total,
            long userId)
        {
            var query = RevaluationRepository.Instance.Find(x => x.RevaluationStatus > RevaluationStatusEnum.未接收完); 
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                total = 0;
                return null;
            }
            if (!user.IsAdmin)
            {
                query = query.Where(x => x.CompanyId == user.CompanyId);
            }
            if (!string.IsNullOrEmpty(condition.RevaluationNo))
            {
                query = query.Where(x => x.RevaluationNo.Contains(condition.RevaluationNo));
            }
            if (!string.IsNullOrEmpty(condition.RevaluationName))
            {
                query = query.Where(x => x.RevaluationName.Contains(condition.RevaluationName));
            }
            if (!string.IsNullOrEmpty(condition.EvalType))
            {
                query = query.Where(x => x.EvalType == condition.EvalType);
            }
            if (condition.RevaluationStatus.HasValue)
            {
                var revaluationStatus =
                    (RevaluationStatusEnum)
                        Enum.Parse(typeof (RevaluationStatusEnum), condition.RevaluationStatus.Value.ToString());
                query = query.Where(x => x.RevaluationStatus == revaluationStatus);
            }
            if (condition.CreateTimeFrom.HasValue)
                query = query.Where(x => x.CreateTime >= condition.CreateTimeFrom);
            if (condition.CreateTimeTo.HasValue)
            {
                var endDate = condition.CreateTimeTo.Value.AddDays(1);
                query = query.Where(x => x.CreateTime < endDate);
            }
            if (condition.ScanType == 1)
            {
                query = query.Where(x => !x.IsScan.HasValue && !x.ScanTime.HasValue);
            }
            query = query.OrderByDescending(x => x.TId);
            return RevaluationRepository.Instance.FindForPaging(size, index, query, out total).ToList();
        }

        /// <summary>
        /// 获取复估单
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Revaluation GetRevaluationById(long revaluationId, long userId)
        {
            var revaluation = RevaluationRepository.Instance.Source.FirstOrDefault(x => x.TId == revaluationId);
            if (revaluation == null)
            {
                throw new ServiceException("复估单不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == revaluation.CompanyId || user.IsAdmin) || revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("您无权操作此复估单!");
            }
            return revaluation;
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
        public IList<RevaluationItem> GetRevaluationItemList(long revaluationId, int index, int size, out int total,
            out bool canFinish, long userId)
        {
            canFinish = false;
            var revaluation = RevaluationRepository.Instance.Source.FirstOrDefault(x => x.TId == revaluationId);
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (revaluation == null || user == null || (revaluation.CompanyId != user.CompanyId && !user.IsAdmin) ||
                revaluation.RevaluationStatus == RevaluationStatusEnum.未受理 ||
                revaluation.RevaluationStatus == RevaluationStatusEnum.已撤单 ||
                revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                total = 0;
                return null;
            }
            var query = RevaluationItemRepository.Instance.Find(x => x.RevaluationId == revaluationId);
            canFinish = !query.Any(x => x.RevaluationValue.HasValue == false || x.RevaluationTime.HasValue == false);
            query = query.OrderBy(x => x.TId);
            return RevaluationItemRepository.Instance.FindForPaging(size, index, query, out total).ToList();
        }

        /// <summary>
        /// 获取复估项
        /// </summary>
        /// <param name="revaluationItemId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RevaluationItem GetRevaluationItemById(long revaluationItemId, long userId)
        {
            var revaluationItem = RevaluationItemRepository.Instance.Source.FirstOrDefault(x => x.TId == revaluationItemId);
            if (revaluationItem == null)
            {
                throw new ServiceException("复估项不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null ||
                !(user.CompanyId == revaluationItem.Revaluation.CompanyId || user.IsAdmin ||
                  user.Company == revaluationItem.InitialEstimateCompany) ||
                revaluationItem.Revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("您无权操作此复估项!");
            }
            return revaluationItem;
        }

        /// <summary>
        /// 复估受理
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool AcceptRevaluation(long revaluationId, long userId)
        {
            var revaluation = RevaluationRepository.Instance.Source.FirstOrDefault(x => x.TId == revaluationId);
            if (revaluation == null)
            {
                throw new ServiceException("复估单不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == revaluation.CompanyId || user.IsAdmin) || revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (revaluation.RevaluationStatus != RevaluationStatusEnum.未受理)
            {
                throw new ServiceException("复估单已被受理，请勿重复受理!");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该复估单已撤单。");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.复估完成)
            {
                throw new ServiceException("您好，复估已完成，请勿再继续操作。");
            }
            revaluation.RevaluationStatus = RevaluationStatusEnum.复估受理;
            var result = RevaluationRepository.Instance.Save(revaluation);
            return result;
        }

        /// <summary>
        /// 单项复估
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool RevaluatingRevaluationItem(RevaluationItem entity, long userId)
        {
            var revaluationItem = RevaluationItemRepository.Instance.Source.FirstOrDefault(x => x.TId == entity.TId);
            if (revaluationItem == null)
            {
                throw new ServiceException("复估项不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == revaluationItem.Revaluation.CompanyId || user.IsAdmin) || revaluationItem.Revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("您无权操作此复估项!");
            }
            if (revaluationItem.Revaluation.RevaluationStatus == RevaluationStatusEnum.未受理)
            {
                throw new ServiceException("复估单还未受理，请勿先受理。");
            }
            if (revaluationItem.Revaluation.RevaluationStatus == RevaluationStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该复估单已撤单。");
            }
            if (revaluationItem.Revaluation.RevaluationStatus == RevaluationStatusEnum.复估完成)
            {
                throw new ServiceException("您好，复估已完成，请勿再继续操作。");
            }
            if (!entity.RevaluationValue.HasValue)
            {
                throw new ServiceException("请输入复估结果!");
            }
            revaluationItem.RevaluationValue = entity.RevaluationValue.Value;
            revaluationItem.RevaluationDifference = entity.RevaluationValue.Value - revaluationItem.InitialEstimateValue;
            revaluationItem.RevaluationIncrease = (entity.RevaluationValue.Value - revaluationItem.InitialEstimateValue) /
                                                  revaluationItem.InitialEstimateValue;
            revaluationItem.ChangeDescription = entity.ChangeDescription;
            revaluationItem.Remark = entity.Remark;
            revaluationItem.RevaluationTime = DateTime.Now;
            return RevaluationItemRepository.Instance.Save(revaluationItem);
        }

        /// <summary>
        /// 导出复估
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public byte[] ExportRevaluation(long revaluationId, long userId)
        {
            var revaluation = RevaluationRepository.Instance.Source.FirstOrDefault(x => x.TId == revaluationId);
            if (revaluation == null)
            {
                throw new ServiceException("复估单不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == revaluation.CompanyId || user.IsAdmin) || revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.未受理)
            {
                throw new ServiceException("复估单还未受理，请勿先受理。");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该复估单已撤单。");
            }
            var titles = new Dictionary<string, string>()
            {
                {"流水号", "ProjectNo"},
                {"经营机构", "OperationOrganization"},
                {"管户客户经理", "CustomerAccountManager"},
                {"联系电话", "ContactNumber"},
                {"协议号", "ProtocolNumber"},
                {"客户号", "CustomerNumber"},
                {"押品地址", "PledgeAddress"},
                {"押品类型", "PropertyType"},
                {"借款人名称", "BorrowerName"},
                {"初估时间", "InitialEstimateTime"},
                {"初估价值(元)", "InitialEstimateValue"},
                {"复估价值(元)", "RevaluationValue"},
                //{"复估时点", "RevaluationTime"},
                {"变动说明", "ChangeDescription"},
                {"备注", "Remark"}
            };
            var book = new HSSFWorkbook();
            #region 加锁
            var lockCell = book.CreateCellStyle();
            lockCell.IsLocked = true;
            var unlockCell = book.CreateCellStyle();
            unlockCell.IsLocked = revaluation.RevaluationStatus != RevaluationStatusEnum.复估受理;
            unlockCell.BottomBorderColor = HSSFColor.Green.Index;
            unlockCell.TopBorderColor = HSSFColor.Green.Index;
            #endregion
            var sheet = book.CreateSheet("sheet1");
            var row = sheet.CreateRow(0);
            ICell cell = null;
            var index = 0;
            foreach (var title in titles)
            {
                if (index > 11)
                {
                    sheet.SetColumnWidth(index, 40 * 256);
                }
                cell = row.CreateCell(index);
                cell.SetCellValue(title.Key);
                cell.CellStyle = lockCell;
                index++;
            }
            index = 1;
            foreach (var revaluationItem in revaluation.RevaluationItems)
            {
                row = sheet.CreateRow(index);
                cell = row.CreateCell(0);
                cell.SetCellValue(revaluationItem.ProjectNo);
                cell.CellStyle = lockCell;
                cell = row.CreateCell(1);
                cell.SetCellValue(revaluationItem.OperationOrganization);
                cell = row.CreateCell(2);
                cell.SetCellValue(revaluationItem.CustomerAccountManager);
                cell = row.CreateCell(3);
                cell.SetCellValue(revaluationItem.ContactNumber);
                cell = row.CreateCell(4);
                cell.SetCellValue(revaluationItem.ProtocolNumber);
                cell = row.CreateCell(5);
                cell.SetCellValue(revaluationItem.CustomerNumber);
                cell = row.CreateCell(6);
                cell.SetCellValue(revaluationItem.PledgeAddress);
                cell = row.CreateCell(7);
                cell.SetCellValue(revaluationItem.PropertyType);
                cell = row.CreateCell(8);
                cell.SetCellValue(revaluationItem.BorrowerName);
                cell = row.CreateCell(9);
                cell.SetCellValue(revaluationItem.InitialEstimateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                cell = row.CreateCell(10);
                cell.SetCellValue(revaluationItem.InitialEstimateValue.ToString("F2"));
                cell = row.CreateCell(11);
                cell.SetCellValue(revaluationItem.RevaluationValue.HasValue
                    ? revaluationItem.RevaluationValue.Value.ToString("F2")
                    : "");
                cell.CellStyle = unlockCell;
                //cell = row.CreateCell(12);
                //cell.SetCellValue(revaluationItem.RevaluationTime.HasValue ? revaluationItem.RevaluationTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : "");
                //cell.CellStyle = unlockCell;
                cell = row.CreateCell(12);
                cell.SetCellValue(revaluationItem.ChangeDescription);
                cell.CellStyle = unlockCell;
                cell = row.CreateCell(13);
                cell.SetCellValue(revaluationItem.Remark);
                cell.CellStyle = unlockCell;
                index++;
            }
            sheet.ProtectSheet("ZXEval");//设置密码保护
            var stream = new MemoryStream();
            book.Write(stream);
            return stream.ToArray();
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
            if (revaluationId == 0)
            {
                return "上传失败!";
            }
            var revaluation = RevaluationRepository.Instance.Source.FirstOrDefault(x => x.TId == revaluationId);
            if (revaluation == null)
            {
                return "上传失败!";
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == revaluation.CompanyId || user.IsAdmin) || revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                return "您无权操作此项目!";
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.未受理)
            {
                return "复估单还未受理，请勿先受理。";
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.已撤单)
            {
                return "您好，该复估单已撤单。";
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.复估完成)
            {
                return "您好，复估已完成，请勿再继续操作。";
            }
            var extensions = new string[] {"xls", "xlsx"};
            if (!extensions.Contains(fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower()))
            {
                return "文件类型不对，只能导入xls和xlsx格式的文件!";
            }
            //保存至本地
            string filePath = FileStreamHelper.UrlConvertorLocal(FileStreamHelper.GetUploadFilePath());
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
            filePath += revaluation.RevaluationNo + "-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." +
                        fileName.Substring(fileName.LastIndexOf('.') + 1);
            fileByte.SaveAs(filePath);
            var revaluationItems = new List<RevaluationItem>();
            var errorMsg = new List<string>();

            #region Excel获取数据

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var extension = Path.GetExtension(filePath);
                if (extension == ".xlsx")
                {
                    var workbook = new XSSFWorkbook(fs);
                    var sheet = workbook.GetSheetAt(0) as XSSFSheet;
                    if (sheet == null)
                    {
                        return "读取excel数据出错，请检查sheet表格是否有数据!";
                    }
                    if (sheet.LastRowNum <= 0)
                    {
                        return "读取excel数据出错，请检查sheet表格中是否有数据!";
                    }
                    var rowCount = sheet.LastRowNum + 1;
                    for (var i = (sheet.FirstRowNum + 1); i < rowCount; i++)
                    {
                        var row = sheet.GetRow(i) as XSSFRow;
                        if (row == null)
                        {
                            return "读取excel数据出错，请检查表格是否有行数据不存在!";
                        }
                        if (row.GetCell(0) == null || row.GetCell(11) == null || row.GetCell(12) == null)
                        {
                            errorMsg.Add(string.Format("读取excel数据出错，请检查行数据是否正确,所在行号[{0}]", i + 1));
                            continue;
                        }
                        decimal value;
                        var projectNo = row.GetCell(0).ToString().Trim();
                        var revaluationValue = row.GetCell(11).ToString().Trim();
                        var changeDescription = row.GetCell(12).ToString().Trim();
                        var remark = row.GetCell(13).ToString().Trim();
                        var hasError = false;
                        if (string.IsNullOrEmpty(projectNo) && string.IsNullOrEmpty(revaluationValue))
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(projectNo))
                        {
                            errorMsg.Add(string.Format("流水号不能为空,所在行号[{0}]", i + 1));
                            hasError = true;
                        }
                        if (!(revaluation.RevaluationItems.Any(x => x.ProjectNo == projectNo)))
                        {
                            errorMsg.Add(string.Format("[{0}]复估单不存在项目[{1}],所在行号[{2}]", revaluation.RevaluationNo,
                                projectNo, i + 1));
                            hasError = true;
                        }
                        if (string.IsNullOrEmpty(revaluationValue) || !decimal.TryParse(revaluationValue, out value))
                        {
                            errorMsg.Add(string.Format("复估价值不能为空且数据必须正确,流水号为[{0}],所在行号[{1}]",
                                projectNo, i + 1));
                            continue;
                        }
                        if (value <= 0)
                        {
                            errorMsg.Add(string.Format("流水号为[{0}],复估价值必须大于0,所在行号[{1}]", projectNo, i + 1));
                            hasError = true;
                        }
                        if (changeDescription.Length > 300)
                        {
                            errorMsg.Add(string.Format("《变动说明》字数不能超过300,流水号为[{0}],所在行号[{1}]",
                                projectNo, i + 1));
                            hasError = true;
                        }
                        if (remark.Length > 500)
                        {
                            errorMsg.Add(string.Format("《备注》字数不能超过500,流水号为[{0}],所在行号[{1}]",
                                projectNo, i + 1));
                            hasError = true;
                        }
                        if (!hasError)
                        {
                            revaluationItems.Add(new RevaluationItem()
                            {
                                ProjectNo = projectNo,
                                RevaluationValue = value,
                                ChangeDescription = changeDescription,
                                Remark = remark
                            });
                        }
                    }
                }
                else
                {
                    var workbook = new HSSFWorkbook(fs);
                    var sheet = workbook.GetSheetAt(0) as HSSFSheet;
                    if (sheet == null)
                    {
                        return "读取excel数据出错，请检查sheet表格是否有数据!";
                    }
                    var rowCount = sheet.LastRowNum + 1;
                    for (var i = (sheet.FirstRowNum + 1); i < rowCount; i++)
                    {
                        if (errorMsg.Count > 6)
                        {
                            errorMsg.Add("......");
                            break;
                        }
                        var row = sheet.GetRow(i) as HSSFRow;
                        if (row == null)
                        {
                            return "读取excel数据出错，请检查表格是否有行数据不存在!";
                        }
                        if (row.GetCell(0) == null || row.GetCell(11) == null || row.GetCell(12) == null)
                        {
                            errorMsg.Add(string.Format("读取excel数据出错，请检查行数据是否正确,所在行号[{0}]", i + 1));
                            continue;
                        }
                        decimal value;
                        var projectNo = row.GetCell(0).ToString().Trim();
                        var revaluationValue = row.GetCell(11).ToString().Trim();
                        var hasError = false;
                        if (string.IsNullOrEmpty(projectNo) && string.IsNullOrEmpty(revaluationValue))
                        {
                            continue;
                        }
                        if (string.IsNullOrEmpty(projectNo))
                        {
                            errorMsg.Add(string.Format("流水号不能为空,所在行号[{0}]", i + 1));
                            hasError = true;
                        }
                        if (!(revaluation.RevaluationItems.Any(x => x.ProjectNo == projectNo)))
                        {
                            errorMsg.Add(string.Format("[{0}]复估单不存在项目[{1}],所在行号[{2}]", revaluation.RevaluationNo,
                                projectNo, i + 1));
                            hasError = true;
                        }
                        if (string.IsNullOrEmpty(revaluationValue) || !decimal.TryParse(revaluationValue, out value))
                        {
                            errorMsg.Add(string.Format("复估价值不能为空且数据必须正确,流水号为[{0}],所在行号[{1}]",
                                projectNo, i + 1));
                            continue;
                        }
                        if (value <= 0)
                        {
                            errorMsg.Add(string.Format("流水号为[{0}],复估价值必须大于0,所在行号[{1}]", projectNo, i + 1));
                            hasError = true;
                        }
                        if (!hasError)
                        {
                            revaluationItems.Add(new RevaluationItem()
                            {
                                ProjectNo = projectNo,
                                RevaluationValue = value,
                                ChangeDescription = row.GetCell(12).ToString().Trim(),
                                Remark = row.GetCell(13).ToString().Trim()
                            });
                        }
                    }
                }
            }

            #endregion

            //var noContain =
            //    revaluation.RevaluationItems.Where(x => !revaluationItems.Select(y => y.ProjectNo).Contains(x.ProjectNo))
            //        .Select(z => z.ProjectNo)
            //        .ToList();
            //if (noContain.Any())
            //{
            //    errorMsg.Add(string.Format("Excel中未包含复估单[{0}]所包含的项目[{1}]", revaluation.RevaluationNo,
            //        string.Join(",", noContain)));
            //}
            if (errorMsg.Any())
            {
                return string.Join("<br/>", errorMsg);
            }
            foreach (var entity in revaluationItems)
            {
                var revaluationItem =
                    revaluation.RevaluationItems.FirstOrDefault(x => x.ProjectNo == entity.ProjectNo);
                entity.TId = revaluationItem.TId;
                entity.RevaluationDifference = entity.RevaluationValue.Value -
                                                        revaluationItem.InitialEstimateValue;
                entity.RevaluationIncrease = (entity.RevaluationValue.Value -
                                                       revaluationItem.InitialEstimateValue) /
                                                      revaluationItem.InitialEstimateValue;
                entity.RevaluationTime = DateTime.Now;
            }
            var result = BatchUpdateRevaluationItem(revaluationItems);
            return result ? "上传成功" : "上传失败";
        }

        /// <summary>
        /// sql语句批量更新
        /// </summary>
        /// <param name="revaluationItems"></param>
        /// <returns></returns>
        private bool BatchUpdateRevaluationItem(IEnumerable<RevaluationItem> revaluationItems)
        {
            var sql = new StringBuilder();
            sql.Append("BEGIN TRY\n");
            sql.Append("BEGIN TRANSACTION\n");
            foreach (var revaluationItem in revaluationItems)
            {
                sql.Append(string.Format(
                    "UPDATE RevaluationItem SET RevaluationValue={0},RevaluationDifference={1},RevaluationIncrease={2},ChangeDescription='{3}',Remark='{4}',RevaluationTime='{5}' WHERE TID={6};",
                    revaluationItem.RevaluationValue,
                    revaluationItem.RevaluationDifference,
                    revaluationItem.RevaluationIncrease, revaluationItem.ChangeDescription, revaluationItem.Remark,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), revaluationItem.TId
                    ));
            }
            sql.Append("COMMIT TRANSACTION\n");
            sql.Append("END TRY\n");
            sql.Append("BEGIN CATCH\n");
            sql.Append("SELECT ERROR_NUMBER() AS ERRORNUMBER\n");
            sql.Append("ROLLBACK TRANSACTION\n");
            sql.Append("END CATCH\n");
            return RevaluationItemRepository.Instance.ExecuteSqlCommand(sql.ToString()) > 0;
        }

        /// <summary>
        /// sql语句批量插入
        /// </summary>
        /// <param name="revaluationItems"></param>
        /// <returns></returns>
        private bool BatchInsertRevaluationItem(IEnumerable<RevaluationItem> revaluationItems)
        {
            var sql = new StringBuilder();
            sql.Append("BEGIN TRY\n");
            sql.Append("BEGIN TRANSACTION\n");
            sql.Append(@"INSERT INTO RevaluationItem
                   (RevaluationId,BusinessId,OperationOrganization,BorrowerName,CreditBalance,FiveCategories,ContractExpirationDate,
                   PropertyType,PledgeAddress,InitialEstimateValue,InitialEstimateOrganization,InitialEstimateTime,CustomerAccountManager,
                   ContactNumber,ProtocolNumber,CustomerNumber,ProjectNo,IsConsult,CreateTime)
                   VALUES");
            foreach (var item in revaluationItems)
            {
                sql.Append(string.Format(
                    "('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}'),",
                    item.RevaluationId, item.BusinessId, item.OperationOrganization, item.BorrowerName,
                    item.CreditBalance, item.FiveCategories, item.ContractExpirationDate, item.PropertyType,
                    item.PledgeAddress, item.InitialEstimateValue, item.InitialEstimateOrganization,
                    item.InitialEstimateTime.ToString("yyyy-MM-dd HH:mm:ss"), item.CustomerAccountManager, item.ContactNumber, item.ProtocolNumber,
                    item.CustomerNumber, item.ProjectNo, item.IsConsult, item.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")));
            }
            sql = sql.Remove(sql.ToString().LastIndexOf(','), 1);
            sql.Append("COMMIT TRANSACTION\n");
            sql.Append("END TRY\n");
            sql.Append("BEGIN CATCH\n");
            sql.Append("SELECT ERROR_NUMBER() AS ERRORNUMBER\n");
            sql.Append("ROLLBACK TRANSACTION\n");
            sql.Append("END CATCH\n");
            return RevaluationItemRepository.Instance.ExecuteSqlCommand(sql.ToString()) > 0;
        }

        /// <summary>
        /// 完成复估
        /// </summary>
        /// <param name="revaluationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool FinishRevaluation(long revaluationId, long userId)
        {
            var revaluation = RevaluationRepository.Instance.Source.FirstOrDefault(x => x.TId == revaluationId);
            if (revaluation == null)
            {
                throw new ServiceException("复估单不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == revaluation.CompanyId || user.IsAdmin) ||
                revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.未受理)
            {
                throw new ServiceException("复估单还未受理，请勿先受理!");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该复估单已撤单。");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.复估完成)
            {
                throw new ServiceException("您好，复估已完成，请勿再继续操作。");
            }
            var unfinishItem = revaluation.RevaluationItems.Where(x => !x.RevaluationValue.HasValue).ToList();
            if (unfinishItem.Any())
            {
                throw new ServiceException("存在未完成复估的复估项：(" + string.Join(",", unfinishItem.Select(y => y.ProjectNo)) +
                                           ")!");
            }
            revaluation.RevaluationStatus = RevaluationStatusEnum.复估完成;
            var result = RevaluationRepository.Instance.Save(revaluation);
            if (result)
            {
                Task.Factory.StartNew(() => BatchSendRevaluationResult(revaluation)
                    //{
                    //    var request = new ApiModelRevaluationResultRequest()
                    //    {
                    //        bussiness_ID = revaluation.BusinessId,
                    //        result_list =
                    //            new List<ApiModelRevaluationItemResultRequest>(
                    //                revaluation.RevaluationItems.Select(x => new ApiModelRevaluationItemResultRequest()
                    //                {
                    //                    record_ID = x.BusinessId,
                    //                    re_valuation_value = x.RevaluationValue.Value.ToString("F2"),
                    //                    re_valuation_time = x.RevaluationTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    //                    difference = x.RevaluationDifference.Value.ToString("F2"),
                    //                    change_rate = x.RevaluationIncrease.Value.ToString("F2"),
                    //                    description_of_change = x.ChangeDescription,
                    //                    notes = x.Remark
                    //                }))
                    //    };
                    //    var response = new FangguguApiService().SendRevaluationResult(request);
                    //    LogHelper.Error(revaluation.RevaluationNo + "复估完成Json：" + request.ToJson(), null);
                    //    LogHelper.Error(
                    //        revaluation.RevaluationNo + "复估完成返回结果：" + response.ToJson() + "，返回时间：" +
                    //        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null);
                    //    if (!response.Success)
                    //    {
                    //        LogHelper.Error(revaluation.RevaluationNo + "发送复估完成失败，返回信息：" + response.Msg, null);
                    //    }
                    //}
                    ).
                    ContinueWith((t) =>
                    {
                        if (t.Exception != null)
                        {
                            LogHelper.Error(revaluation.RevaluationNo + "发送复估完成失败", t.Exception);
                        }
                    });
            }
            return result;
        }

        /// <summary>
        /// 发送复估结果
        /// </summary>
        /// <param name="revaluation"></param>
        /// <returns></returns>
        private void BatchSendRevaluationResult(Revaluation revaluation)
        {
            var isComplete = true;
            var success = new List<long>();
            var unsuccess = new List<long>();
            var tryTime = 0;
            while (revaluation.RevaluationItems.Any(x => !success.Contains(x.TId)))
            {
                var list = revaluation.RevaluationItems.Where(x => unsuccess.Contains(x.TId));
                if (!list.Any())
                {
                    list = revaluation.RevaluationItems.Where(x => !success.Contains(x.TId));
                    if (list.Count() > 1000)
                    {
                        list = list.Take(1000);
                        isComplete = false;
                    }
                    else
                    {
                        isComplete = true;
                    }
                }
                var items = list.Select(item => new ApiModelRevaluationItemResultRequest
                {
                    record_ID = item.BusinessId,
                    re_valuation_value = item.RevaluationValue.Value.ToString("F2"),
                    re_valuation_time = item.RevaluationTime.Value.ToString("yyyy-MM-dd HH:mm:ss"),
                    difference = item.RevaluationDifference.Value.ToString("F2"),
                    change_rate = item.RevaluationIncrease.Value.ToString("F2"),
                    description_of_change = item.ChangeDescription,
                    notes = item.Remark
                }).ToList();
                var request = new ApiModelRevaluationResultRequest()
                {
                    bussiness_ID = revaluation.BusinessId,
                    result_list = items,
                    is_complete = isComplete
                };
                LogHelper.Error(revaluation.RevaluationNo + "复估完成Json：" + request.ToJson(), null);
                var response = new FangguguApiService().SendRevaluationResult(request);
                LogHelper.Error(
                    revaluation.RevaluationNo + "复估完成返回结果：" + response.ToJson() + "，返回时间：" +
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null);
                if (!response.Success && tryTime < 3)
                {
                    tryTime++;
                    unsuccess.AddRange(list.Where(x => !unsuccess.Contains(x.TId)).Select(x => x.TId));
                    LogHelper.Error(revaluation.RevaluationNo + "发送复估完成失败，返回信息：" + response.Msg, null);
                }
                else
                {
                    tryTime = 0;
                    success.AddRange(list.Where(x => !success.Contains(x.TId)).Select(x => x.TId));
                }
            }
        }

        /// <summary>
        /// 复估价格异议
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        public bool ConsultRevaluation(string businessId)
        {
            var item = RevaluationItemRepository.Instance.Source.FirstOrDefault(x => x.BusinessId == businessId);
            if (item == null)
            {
                throw new ServiceException("评估系统不存在该BusinessId：" + businessId);
            }
            if (item.Revaluation.RevaluationStatus != RevaluationStatusEnum.复估完成)
            {
                throw new ServiceException("该复估单还未提交复估完成，无法提出异议!");
            }
            if (item.IsConsult && !(item.IsCancelConsult.HasValue && item.IsCancelConsult.Value))
            {
                throw new ServiceException("该BusinessId已在评估系统提出价格异议并且还未撤销,请勿重复提出异议!");
            }
            var company = CompanyRepository.Instance.Source.FirstOrDefault(x => x.CompanyName == item.InitialEstimateOrganization);
            if (company == null)
            {
                throw new ServiceException("评估系统不存在该初估公司，无法提出异议");
            }
            item.InitialEstimateCompany = company;
            item.IsConsult = true;
            item.IsCancelConsult = false;
            var result = RevaluationItemRepository.Instance.Save(item);
            return result;
        }

        /// <summary>
        /// 撤销复估价格异议
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        public bool CancelConsultRevaluation(string businessId)
        {
            var item = RevaluationItemRepository.Instance.Source.FirstOrDefault(x => x.BusinessId == businessId);
            if (item==null)
            {
                throw new ServiceException("评估系统不存在该复估项目，无法撤销异议!");
            }
            if (!item.IsConsult)
            {
                throw new ServiceException("该复估项目未发起异议，无需撤销异议!");
            }
            if (item.IsCancelConsult.HasValue && item.IsCancelConsult.Value)
            {
                throw new ServiceException("该复估项目已撤销异议，无需重复撤销!");
            }
            item.IsCancelConsult = true;
            return RevaluationItemRepository.Instance.Save(item);
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
        public IList<RevaluationItem> GetConsultList(ConsultCondition condition, int index, int size, out int total, long userId)
        {
            var query = RevaluationItemRepository.Instance.Find(x => x.IsConsult);
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                total = 0;
                return null;
            }
            if (!user.IsAdmin)
            {
                query = query.Where(x => x.InitialEstimateCompany == user.Company);
            }
            if (!string.IsNullOrEmpty(condition.ProjectNo))
            {
                query = query.Where(x => x.ProjectNo.Contains(condition.ProjectNo));
            }
            if (!string.IsNullOrEmpty(condition.PledgeAddress))
            {
                query = query.Where(x => x.PledgeAddress.Contains(condition.PledgeAddress));
            }
            if (!string.IsNullOrEmpty(condition.EvalType))
            {
                query = query.Where(x => x.Revaluation.EvalType == condition.EvalType);
            }
            if (!string.IsNullOrEmpty(condition.PropertyType))
            {
                query = query.Where(x => x.PropertyType == condition.PropertyType);
            }
            if (condition.RevaluationTimeFrom.HasValue)
                query = query.Where(x => x.RevaluationTime >= condition.RevaluationTimeFrom.Value);
            if (condition.RevaluationTimeTo.HasValue)
            {
                var endDate = condition.RevaluationTimeTo.Value.AddDays(1);
                query = query.Where(x => x.RevaluationTime < endDate);
            }
            if (condition.ScanType == 1)
            {
                query = query.Where(x => !x.IsScan.HasValue && !x.ScanTime.HasValue);
            }
            query = query.OrderByDescending(x => x.TId);
            return RevaluationItemRepository.Instance.FindForPaging(size, index, query, out total).ToList();
        }

        /// <summary>
        /// 复估认可
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool ApproveRevaluationItem(RevaluationItem entity, long userId)
        {
            var revaluationItem = RevaluationItemRepository.Instance.Source.FirstOrDefault(x => x.TId == entity.TId);
            if (revaluationItem == null)
            {
                throw new ServiceException("复估项不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.Company == revaluationItem.InitialEstimateCompany || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此复估项!");
            }
            if (revaluationItem.Revaluation.RevaluationStatus == RevaluationStatusEnum.未受理)
            {
                throw new ServiceException("复估单还未受理，请勿先受理。");
            }
            if (!revaluationItem.IsConsult)
            {
                throw new ServiceException("项目未提交价格异议，无需进行复估认可。");
            }
            if (revaluationItem.IsCancelConsult.HasValue && revaluationItem.IsCancelConsult.Value)
            {
                throw new ServiceException("项目已撤销价格异议，无需再进行复估认可。");
            }
            if (revaluationItem.IsApprove.HasValue)
            {
                throw new ServiceException("项目已进行复估认可，请勿重复操作");
            }
            if (!entity.IsApprove.HasValue)
            {

            }
            revaluationItem.IsApprove = entity.IsApprove;
            revaluationItem.ConsultReply = entity.ConsultReply;
            var result = RevaluationItemRepository.Instance.Save(revaluationItem);
            if (result)
            {
                Task.Factory.StartNew(() =>
                {
                    var request = new ApiModelConsultResultRequest()
                    {
                        record_ID = revaluationItem.BusinessId,
                        //reply_content =
                        //    string.Format("对复估价格表示【{0}】。回复信息：{1}",
                        //        entity.IsApprove.HasValue && entity.IsApprove.Value ? "认可" : "不认可",
                        //        string.IsNullOrEmpty(entity.ConsultReply) ? "无" : entity.ConsultReply)
                        reply_content = entity.ConsultReply
                    };
                    var response = new FangguguApiService().SendConsultResult(request);
                    LogHelper.Error(revaluationItem.ProjectNo + "复估认可Json：" + request.ToJson(), null);
                    LogHelper.Error(
                        revaluationItem.ProjectNo + "复估认可返回结果：" + response.ToJson() + "，返回时间：" +
                        DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null);
                    if (!response.Success)
                    {
                        LogHelper.Error(revaluationItem.ProjectNo + "发送复估认可失败，返回信息：" + response.Msg, null);
                    }
                }).ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        LogHelper.Error(revaluationItem.ProjectNo + "发送复估认可失败", t.Exception);
                    }
                });
            }
            return result;
        }

        /// <summary>
        /// 生成复估单号
        /// </summary>
        private static string GetInstanceNo()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var instanceNo = BitConverter.ToInt64(buffer, 0).ToString().Substring(0, 9);
            if (RevaluationRepository.Instance.Find(x => x.RevaluationNo == instanceNo).Any())
            {
                instanceNo = GetInstanceNo();
            }
            return instanceNo;
        }

        /// <summary>
        /// 保存扫描评估单记录
        /// </summary>
        /// <param name="idList"></param>
        public void SaveScanResult(IList<long> idList)
        {
            var result = RevaluationRepository.Instance.Source.Where(x => idList.Contains(x.TId) && x.RevaluationStatus > RevaluationStatusEnum.未接收完).ToList();
            foreach (var revaluation in result)
            {
                revaluation.IsScan = true;
                revaluation.ScanTime = DateTime.Now;
                RevaluationRepository.Instance.Save(revaluation);
            }
        }

        /// <summary>
        /// 获取复估单
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Revaluation GetByRevaluationNo(string revaluationNo, long userId)
        {
            var revaluation = RevaluationRepository.Instance.Source.FirstOrDefault(x => x.RevaluationNo == revaluationNo);
            if (revaluation == null)
            {
                throw new ServiceException("复估单不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == revaluation.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此复估单!");
            }
            return revaluation;
        }

        /// <summary>
        /// 获取复估项
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="projectNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public RevaluationItem GetRevaluationItem(string revaluationNo, string projectNo, long userId)
        {
            var revaluationItem =
                RevaluationItemRepository.Instance.Source.FirstOrDefault(
                    x => x.Revaluation.RevaluationNo == revaluationNo && x.ProjectNo == projectNo);
            if (revaluationItem == null)
            {
                throw new ServiceException("复估项不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null ||
                !(user.CompanyId == revaluationItem.Revaluation.CompanyId || user.IsAdmin ||
                  user.Company == revaluationItem.InitialEstimateCompany) ||
                revaluationItem.Revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("您无权操作此复估项!");
            }
            return revaluationItem;
        }

        /// <summary>
        /// 保存扫描复估认可记录
        /// </summary>
        /// <param name="idList"></param>
        public void SaveScanProjectResult(IList<long> idList)
        {
            var result = RevaluationItemRepository.Instance.Source.Where(x => idList.Contains(x.TId) && x.Revaluation.RevaluationStatus > RevaluationStatusEnum.未接收完).ToList();
            foreach (var revalItem in result)
            {
                revalItem.IsScan = true;
                revalItem.ScanTime = DateTime.Now;
                RevaluationItemRepository.Instance.Save(revalItem);
            }
        }

        /// <summary>
        /// 批量复估保存
        /// </summary>
        /// <param name="revaluationNo"></param>
        /// <param name="revalItems"></param>
        /// <param name="userId"></param>
        public void SaveBatchRevalProject(string revaluationNo, IList<RevaluationItem> revalItems, long userId)
        {
            var revaluation = RevaluationRepository.Instance.Source.FirstOrDefault(x => x.RevaluationNo == revaluationNo);
            if (revaluation == null)
            {
               throw new ServiceException("复估单不存在");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == revaluation.CompanyId || user.IsAdmin) || revaluation.RevaluationStatus == RevaluationStatusEnum.未接收完)
            {
                throw new ServiceException("您无权操作此项目");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.未受理)
            {
                throw new ServiceException("复估单还未受理，请勿先受理。");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该复估单已撤单。");
            }
            if (revaluation.RevaluationStatus == RevaluationStatusEnum.复估完成)
            {
                throw new ServiceException("您好，复估已完成，请勿再继续操作。");
            }
            var projectNos = revalItems.Select(x => x.ProjectNo).ToList();
            if (projectNos.Any(x => !revaluation.RevaluationItems.Select(y => y.ProjectNo).Contains(x)))
            {
                throw new ServiceException(string.Format("您好，存在该复估单不包含的复估项：【{0}】",
                    string.Join("、",
                        projectNos.Where(x => !revaluation.RevaluationItems.Select(y => y.ProjectNo).Contains(x)))));
            }
            if (revalItems.Any(x => x.RevaluationValue <= 0))
            {
                throw new ServiceException(string.Format("您好，订单号【{0}】复估价值必须大于0。",
                    string.Join("、", revalItems.Where(x => x.RevaluationValue <= 0).Select(y => y.ProjectNo))));
            }
            if (revalItems.Any(x => x.ChangeDescription.Length > 300))
            {
                throw new ServiceException(string.Format("您好，订单号【{0}】《变动说明》字数不能超过300。",
                    string.Join("、", revalItems.Where(x => x.ChangeDescription.Length > 300).Select(y => y.ProjectNo))));
            }
            if (revalItems.Any(x => x.Remark.Length > 500))
            {
                throw new ServiceException(string.Format("您好，订单号【{0}】《备注》字数不能超过500。",
                    string.Join("、", revalItems.Where(x => x.Remark.Length > 500).Select(y => y.ProjectNo))));
            }
            var uplProjectList = revalItems.Select(x => x.ProjectNo).ToList();
            RevaluationRepository.Instance.Transaction(() =>
            {
                foreach (var entity in revaluation.RevaluationItems)
                {
                    var project = revalItems.FirstOrDefault(x => uplProjectList.Contains(entity.ProjectNo));
                    if (project == null)
                    {
                        continue;
                    }                  
                    entity.RevaluationValue = project.RevaluationValue;
                    entity.RevaluationDifference = project.RevaluationValue -
                                                            entity.InitialEstimateValue;
                    entity.RevaluationIncrease = (project.RevaluationValue -
                                                           entity.InitialEstimateValue) /
                                                          entity.InitialEstimateValue;
                    entity.ChangeDescription = project.ChangeDescription;
                    entity.Remark = project.Remark;
                    entity.RevaluationTime = DateTime.Now;
                    RevaluationItemRepository.Instance.Save(entity);
                }
            });            
        }
    }
}
