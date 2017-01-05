using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using ICSharpCode.SharpZipLib.Zip;
using Peacock.Common.Exceptions;
using Peacock.Common.Helper;
using Peacock.ZXEval.Data.Entities;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Repository.Repositories;
using Peacock.ZXEval.Service.API;
using Peacock.ZXEval.Service.ApiModle;
using Peacock.ZXEval.Service.Base;
using RestSharp.Extensions;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace Peacock.ZXEval.Service
{
    public class ProjectService : SingModel<ProjectService>
    {
        private ProjectService()
        {
             
        }

        /// <summary>
        /// 获取项目
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Project GetProjectById(long projectId, long userId)
        {
            var project = ProjectRepository.Instance.Source.FirstOrDefault(x => x.Id == projectId);
            if (project == null)
            {
                throw new ServiceException("项目不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            return project;
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
        public IList<Project> GetProjectList(ProjectCondition condition, int index, int size, out int total, long userId)
        {
            var query = ProjectRepository.Instance.Source;
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
            if (!string.IsNullOrEmpty(condition.ProjectNo))
            {
                query = query.Where(x => x.ProjectNo.Contains(condition.ProjectNo));
            }
            if (!string.IsNullOrEmpty(condition.PledgeAddress))
            {
                query = query.Where(x => x.PledgeAddress.Contains(condition.PledgeAddress));
            }
            if (condition.ProjectStatus.HasValue)
            {
                var projectStatus =
                    (ProjectStatusEnum) Enum.Parse(typeof (ProjectStatusEnum), condition.ProjectStatus.ToString());
                query = query.Where(x => x.ProjectStatus == projectStatus);
            }
            if (!string.IsNullOrEmpty(condition.EvalType))
            {
                query = query.Where(x => x.EvalType == condition.EvalType);
            }
            if (!string.IsNullOrEmpty(condition.PropertyType))
            {
                query = query.Where(x => x.PropertyType == condition.PropertyType);
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
            query = query.OrderByDescending(x => x.Id);           
            return ProjectRepository.Instance.FindForPaging(size, index, query, out total).ToList();
        }

        /// <summary>
        /// 项目受理
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public bool AcceptProject(long projectId, long userId,string note)
        {
            var project = ProjectRepository.Instance.Source.FirstOrDefault(x => x.Id == projectId);
            if (project == null)
            {
                throw new ServiceException("项目不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (project.ProjectStatus != ProjectStatusEnum.未受理)
            {
                throw new ServiceException("项目已被受理，请勿重复受理!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该项目已撤单。");
            }
            if (project.ProjectStatus == ProjectStatusEnum.发送报告)
            {
                throw new ServiceException("您好，报告已发送，请勿再继续操作。");
            }
            project.ProjectStatus = ProjectStatusEnum.业务受理;
            var result = ProjectRepository.Instance.Save(project);
            if (result)
            {
                //推送状态
                Task.Factory.StartNew(() =>
                {
                    var request = new ApiModelSendStatusRequest()
                    {
                        yewuxitongbaogaoId = project.BusinessId,
                        status = ((int)ProjectStatusEnum.业务受理).ToString(),
                        feedback_description = note
                    };
                    LogHelper.Error(project.ProjectNo + "发送项目受理Json：" + request.ToJson(), null);
                    var response = new FangguguApiService().SendStatus(request);
                    if (!response.Success)
                    {
                        LogHelper.Error(project.ProjectNo + "发送项目受理状态失败，返回信息：" + response.Msg, null);
                    }
                }).ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        LogHelper.Error(project.ProjectNo + "发送项目受理状态失败", t.Exception);
                    }
                });
                //流程记录
                var projectState = new ProjectStateInfo()
                {
                    ProjectId = project.Id,
                    Content = ProjectStatusEnum.业务受理.ToString(),
                    OperationTime = DateTime.Now,
                    Operator = user.UserName,
                    Note = note
                };
                ProjectStateInfoRepository.Instance.Insert(projectState);
            }
            return result;
        }

        /// <summary>
        /// 外业勘察
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool OperateOuterTask(long projectId, OuterTask entity, long userId, string note)
        {
            var project = ProjectRepository.Instance.Source.FirstOrDefault(x => x.Id == projectId);
            if (project == null)
            {
                throw new ServiceException("项目不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.未受理)
            {
                throw new ServiceException("项目还未受理，请先受理!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该项目已撤单。");
            }
            if (project.ProjectStatus == ProjectStatusEnum.发送报告)
            {
                throw new ServiceException("您好，报告已发送，请勿再继续操作。");
            }
            project.ProjectStatus=ProjectStatusEnum.外业勘察;
            project.OuterTask.AppointmentDate = entity.AppointmentDate;
            project.OuterTask.FinishDate = entity.FinishDate;
            if (!project.OuterTask.CreateTime.HasValue)
            {
                project.OuterTask.CreateTime = DateTime.Now;
            }
            var result = ProjectRepository.Instance.Save(project);
            if (result)
            {
                Task.Factory.StartNew(() =>
                {
                    var request = new ApiModelSendStatusRequest()
                    {
                        yewuxitongbaogaoId = project.BusinessId,
                        status = ((int) ProjectStatusEnum.外业勘察).ToString(),
                        feedback_description = note
                        //    ProjectStatusEnum.外业勘察 +
                        //    (entity.AppointmentDate.HasValue
                        //        ? ("，预约时间：" + entity.AppointmentDate.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                        //        : string.Empty) +
                        //    (entity.FinishDate.HasValue
                        //        ? ("，完成时间：" + entity.FinishDate.Value.ToString("yyyy-MM-dd HH:mm:ss"))
                        //        : string.Empty)
                    };
                    LogHelper.Error(project.ProjectNo + "发送外业勘查Json：" + request.ToJson(), null);
                    var response = new FangguguApiService().SendStatus(request);
                    if (!response.Success)
                    {
                        LogHelper.Error(project.ProjectNo + "发送外业勘察状态失败，返回信息：" + response.Msg, null);
                    }
                }).ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        LogHelper.Error(project.ProjectNo + "发送外业勘察状态失败", t.Exception);
                    }
                });
                //流程记录
                var projectState = new ProjectStateInfo()
                {
                    ProjectId = project.Id,
                    Content = ProjectStatusEnum.外业勘察.ToString(),
                    OperationTime = DateTime.Now,
                    Operator = user.UserName,
                    Note = note
                };
                ProjectStateInfoRepository.Instance.Insert(projectState);
            }
            return result;
        }

        /// <summary>
        /// 报告预估
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool OperateReportEstimate(long projectId, SummaryData entity, long userId, string note)
        {
            var project = ProjectRepository.Instance.Source.FirstOrDefault(x => x.Id == projectId);
            if (project == null)
            {
                throw new ServiceException("项目不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.未受理)
            {
                throw new ServiceException("项目还未受理，请先受理!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该项目已撤单。");
            }
            if (project.ProjectStatus == ProjectStatusEnum.发送报告)
            {
                throw new ServiceException("您好，报告已发送，请勿再继续操作。");
            }
            var resource = GetLastUploadReportResource(projectId, ResourcesEnum.预估报告);
            if (resource == null)
            {
                throw new ServiceException("您还未上传预估报告，请先上传预估报告再保存");
            }
            if (!project.SummaryData.CreateTime.HasValue)
            {
                project.SummaryData.CreateTime = DateTime.Now;
            }
            #region 汇总字段
            project.SummaryData.CompanyName = entity.CompanyName;
            project.SummaryData.ChargeName = entity.ChargeName;
            project.SummaryData.ChargeType = entity.ChargeType;
            project.SummaryData.PledgeName = entity.PledgeName;
            project.SummaryData.EvaluationMethod = entity.EvaluationMethod;
            project.SummaryData.WarrantsType = entity.WarrantsType;
            project.SummaryData.WarrantNo = entity.WarrantNo;
            project.SummaryData.ValuationValue = entity.ValuationValue;
            project.SummaryData.AssessmentFees = entity.AssessmentFees;
            project.SummaryData.Country = entity.Country;
            project.SummaryData.Province = entity.Province;
            project.SummaryData.City = entity.City;
            project.SummaryData.District = entity.District;
            project.SummaryData.Address = entity.Address;
            project.SummaryData.Building = entity.Building;
            project.SummaryData.TotalFloor = entity.TotalFloor;
            project.SummaryData.Floor = entity.Floor;
            project.SummaryData.RoomNo = entity.RoomNo;
            project.SummaryData.FloorHeight = entity.FloorHeight;
            project.SummaryData.StructureArea = entity.StructureArea;
            project.SummaryData.UseLandArea = entity.UseLandArea;
            project.SummaryData.ApportionmentArae = entity.ApportionmentArae;
            project.SummaryData.BuildYear = entity.BuildYear;
            project.SummaryData.BuildingStructure = entity.BuildingStructure;
            project.SummaryData.DurableYears = entity.DurableYears;
            project.SummaryData.Toward = entity.Toward;
            project.SummaryData.MaintenanceCondition = entity.MaintenanceCondition;
            project.SummaryData.DecorationDegree = entity.DecorationDegree;
            project.SummaryData.PublicFacilities = entity.PublicFacilities;
            project.SummaryData.HaveElevator = entity.HaveElevator;
            project.SummaryData.Environment = entity.Environment;
            project.SummaryData.UseState = entity.UseState;
            project.SummaryData.LeasePeriod = entity.LeasePeriod;
            project.SummaryData.PeriodicUnit = entity.PeriodicUnit;
            project.SummaryData.IdlePeriod = entity.IdlePeriod;
            project.SummaryData.StreetCondition = entity.StreetCondition;
            project.SummaryData.TrafficCondition = entity.TrafficCondition;
            project.SummaryData.FlourishingDegree = entity.FlourishingDegree;
            project.SummaryData.EstimatedCompletionDate = entity.EstimatedCompletionDate;
            project.SummaryData.LandUse = entity.LandUse;
            project.SummaryData.LandOwnershipProperties = entity.LandOwnershipProperties;
            project.SummaryData.LandUseRightType = entity.LandUseRightType;
            project.SummaryData.LandUseCertificateNo = entity.LandUseCertificateNo;
            project.SummaryData.LandUseStartDate = entity.LandUseStartDate;
            project.SummaryData.LandUseEndDate = entity.LandUseEndDate;
            project.SummaryData.LandUseRightCompany = entity.LandUseRightCompany;
            project.SummaryData.LandUseYears = entity.LandUseYears;
            project.SummaryData.PurchasePrice = entity.PurchasePrice;
            project.SummaryData.BuyingTime = entity.BuyingTime;
            project.SummaryData.Remark = entity.Remark;
            project.SummaryData.ResidentialDistrict = entity.ResidentialDistrict;
            project.SummaryData.AccessoryArea = entity.AccessoryArea;
            project.SummaryData.PurchaseContractNo = entity.PurchaseContractNo;
            project.SummaryData.PropertyRightCertificateUseYears = entity.PropertyRightCertificateUseYears;
            project.SummaryData.AncestralLandArea = entity.AncestralLandArea;
            project.SummaryData.BasicFacilities = entity.BasicFacilities;
            project.SummaryData.HaveLandCertificate = entity.HaveLandCertificate;
            project.SummaryData.LandArea = entity.LandArea;
            project.SummaryData.LandExploitDegree = entity.LandExploitDegree;
            project.SummaryData.LandRightCertificateNo = entity.LandRightCertificateNo;
            project.SummaryData.LandWarrantPersonName = entity.LandWarrantPersonName;
            project.SummaryData.LandAcquisitionDate = entity.LandAcquisitionDate;
            project.SummaryData.UseYears = entity.UseYears;
            project.SummaryData.LocationRegion = entity.LocationRegion;
            project.SummaryData.HaveConstructionLandPlanningPermit = entity.HaveConstructionLandPlanningPermit;
            project.SummaryData.HaveAboveGroundBuilding = entity.HaveAboveGroundBuilding;
            project.SummaryData.AboveGroundBuildingConditionExplain = entity.AboveGroundBuildingConditionExplain;
            project.SummaryData.PeripheryEnvironmentCondition = entity.PeripheryEnvironmentCondition;
            project.SummaryData.LandPrice = entity.LandPrice;
            project.SummaryData.TransferPaymentCondition = entity.TransferPaymentCondition;
            project.SummaryData.MakeUpTransferFee = entity.MakeUpTransferFee;
            project.SummaryData.AncestralLandNo = entity.AncestralLandNo;
            project.SummaryData.AncestralLandFourDirectionLine = entity.AncestralLandFourDirectionLine;
            project.SummaryData.FloorAreaRatio = entity.FloorAreaRatio;
            project.SummaryData.BuildingDensity = entity.BuildingDensity;
            project.SummaryData.LandLevel = entity.LandLevel;
            project.SummaryData.LandUseMode = entity.LandUseMode;
            project.SummaryData.GroundAttachmentName = entity.GroundAttachmentName;
            project.SummaryData.GroundAttachmentAcreage = entity.GroundAttachmentAcreage;
            project.SummaryData.LandRemainingUseYears = entity.LandRemainingUseYears;
            project.SummaryData.EquityType = entity.EquityType;
            project.SummaryData.CertificateNo = entity.CertificateNo;
            project.SummaryData.IssuingUnit = entity.IssuingUnit;
            project.SummaryData.LatestNetAssetsPerShare = entity.LatestNetAssetsPerShare;
            project.SummaryData.Quantity = entity.Quantity;
            project.SummaryData.IssuePrice = entity.IssuePrice;
            project.SummaryData.PurchaseDate = entity.PurchaseDate;
            project.SummaryData.IsCompanyProfitable = entity.IsCompanyProfitable;
            project.SummaryData.CurrentManagementCondition = entity.CurrentManagementCondition;
            project.SummaryData.IsIndependentStore = entity.IsIndependentStore;
            project.SummaryData.FourDirectionRange = entity.FourDirectionRange;
            project.SummaryData.HaveInterlayer = entity.HaveInterlayer;
            project.SummaryData.StreetDepth = entity.StreetDepth;
            project.SummaryData.StreetWidth = entity.StreetWidth;
            project.SummaryData.HousePropertyRightPersonName = entity.HousePropertyRightPersonName;
            project.SummaryData.HouseType = entity.HouseType;
            project.SummaryData.BuildingType = entity.BuildingType;
            project.SummaryData.PlaneLayoutAdvantage = entity.PlaneLayoutAdvantage;
            project.SummaryData.Facilities = entity.Facilities;
            project.SummaryData.HavePrivateGarage = entity.HavePrivateGarage;
            project.SummaryData.IsUrbanArea = entity.IsUrbanArea;
            project.SummaryData.User = entity.User;
            project.SummaryData.OwnershipSituation = entity.OwnershipSituation;
            project.SummaryData.LandUseRightNature = entity.LandUseRightNature;
            project.SummaryData.LandAcquisitionMode = entity.LandAcquisitionMode;
            project.SummaryData.LandWarrantType = entity.LandWarrantType;
            project.SummaryData.LandCertificateNo = entity.LandCertificateNo;
            project.SummaryData.IssueDate = entity.IssueDate;
            project.SummaryData.ChargeEvaluationUnitPrice = entity.ChargeEvaluationUnitPrice;
            project.SummaryData.FirstOrSecondHand = entity.FirstOrSecondHand;
            project.SummaryData.EquipmentName = entity.EquipmentName;
            project.SummaryData.EquipmentNo = entity.EquipmentNo;
            project.SummaryData.MachineEquipmentType = entity.MachineEquipmentType;
            project.SummaryData.SpecificUse = entity.SpecificUse;
            project.SummaryData.EquipmentCondition = entity.EquipmentCondition;
            project.SummaryData.Brand = entity.Brand;
            project.SummaryData.SpecificationModel = entity.SpecificationModel;
            project.SummaryData.Manufacturer = entity.Manufacturer;
            project.SummaryData.ProductPlace = entity.ProductPlace;
            project.SummaryData.MeasurementUnit = entity.MeasurementUnit;
            project.SummaryData.HaveQualificationCertificate = entity.HaveQualificationCertificate;
            project.SummaryData.HaveSafetyCertificate = entity.HaveSafetyCertificate;
            project.SummaryData.HaveFireControlCertificate = entity.HaveFireControlCertificate;
            project.SummaryData.InvoiceNo = entity.InvoiceNo;
            project.SummaryData.Owner = entity.Owner;
            project.SummaryData.Place = entity.Place;
            project.SummaryData.EquipmentServiceLife = entity.EquipmentServiceLife;
            project.SummaryData.DepreciationCondition = entity.DepreciationCondition;
            project.SummaryData.OverhaulTimes = entity.OverhaulTimes;
            project.SummaryData.PowerFuel = entity.PowerFuel;
            project.SummaryData.LeaseCondition = entity.LeaseCondition;
            project.SummaryData.LeaseTerm = entity.LeaseTerm;
            project.SummaryData.AnnualRent = entity.AnnualRent;
            project.SummaryData.Power = entity.Power;
            project.SummaryData.PowerUnit = entity.PowerUnit;
            project.SummaryData.IsImportedEquipment = entity.IsImportedEquipment;
            project.SummaryData.IsCustomsControl = entity.IsCustomsControl;
            project.SummaryData.PurchaseCurrency = entity.PurchaseCurrency;
            project.SummaryData.ProductionDate = entity.ProductionDate;
            project.SummaryData.RegulatoryExpirationDate = entity.RegulatoryExpirationDate;
            project.SummaryData.EstimateTotalPrice = entity.EstimateTotalPrice;
            project.SummaryData.EstimateUnitPrice = entity.EstimateUnitPrice;
            #endregion
            project.ProjectStatus = ProjectStatusEnum.报告预估;
            var result = ProjectRepository.Instance.Save(project); 
            var domainName = ConfigurationManager.AppSettings["DomainName"];
            if (string.IsNullOrEmpty(domainName))
            {
                domainName = HttpContext.Current.Request.Url.Host +
                                     (HttpContext.Current.Request.Url.Port == 80
                                         ? ""
                                         : (":" + HttpContext.Current.Request.Url.Port));
            }
            if (result)
            {
                Task.Factory.StartNew(() =>
                {
                    var summaryData = new ApiModelSummaryDataRequest();
                    #region 汇总字段
                    summaryData.CompanyName = entity.CompanyName;
                    summaryData.ChargeName = entity.ChargeName;
                    summaryData.ChargeType = entity.ChargeType;
                    summaryData.PledgeName = entity.PledgeName;
                    summaryData.EvaluationMethod = entity.EvaluationMethod;
                    summaryData.WarrantsType = entity.WarrantsType;
                    summaryData.WarrantNo = entity.WarrantNo;
                    summaryData.ValuationValue = entity.ValuationValue;
                    summaryData.AssessmentFees = entity.AssessmentFees;
                    summaryData.Country = entity.Country;
                    summaryData.Province = entity.Province;
                    summaryData.City = entity.City;
                    summaryData.District = entity.District;
                    summaryData.Address = entity.Address;
                    summaryData.Building = entity.Building;
                    summaryData.TotalFloor = entity.TotalFloor;
                    summaryData.Floor = entity.Floor;
                    summaryData.RoomNo = entity.RoomNo;
                    summaryData.FloorHeight = entity.FloorHeight;
                    summaryData.StructureArea = entity.StructureArea;
                    summaryData.UseLandArea = entity.UseLandArea;
                    summaryData.ApportionmentArae = entity.ApportionmentArae;
                    summaryData.BuildYear = entity.BuildYear;
                    summaryData.BuildingStructure = entity.BuildingStructure;
                    summaryData.DurableYears = entity.DurableYears;
                    summaryData.Toward = entity.Toward;
                    summaryData.MaintenanceCondition = entity.MaintenanceCondition;
                    summaryData.DecorationDegree = entity.DecorationDegree;
                    summaryData.PublicFacilities = entity.PublicFacilities;
                    summaryData.HaveElevator = entity.HaveElevator;
                    summaryData.Environment = entity.Environment;
                    summaryData.UseState = entity.UseState;
                    summaryData.LeasePeriod = entity.LeasePeriod;
                    summaryData.PeriodicUnit = entity.PeriodicUnit;
                    summaryData.IdlePeriod = entity.IdlePeriod;
                    summaryData.StreetCondition = entity.StreetCondition;
                    summaryData.TrafficCondition = entity.TrafficCondition;
                    summaryData.FlourishingDegree = entity.FlourishingDegree;
                    summaryData.EstimatedCompletionDate = entity.EstimatedCompletionDate;
                    summaryData.LandUse = entity.LandUse;
                    summaryData.LandOwnershipProperties = entity.LandOwnershipProperties;
                    summaryData.LandUseRightType = entity.LandUseRightType;
                    summaryData.LandUseCertificateNo = entity.LandUseCertificateNo;
                    summaryData.LandUseStartDate = entity.LandUseStartDate;
                    summaryData.LandUseEndDate = entity.LandUseEndDate;
                    summaryData.LandUseRightCompany = entity.LandUseRightCompany;
                    summaryData.LandUseYears = entity.LandUseYears;
                    summaryData.PurchasePrice = entity.PurchasePrice;
                    summaryData.BuyingTime = entity.BuyingTime;
                    summaryData.Remark = entity.Remark;
                    summaryData.ResidentialDistrict = entity.ResidentialDistrict;
                    summaryData.AccessoryArea = entity.AccessoryArea;
                    summaryData.PurchaseContractNo = entity.PurchaseContractNo;
                    summaryData.PropertyRightCertificateUseYears = entity.PropertyRightCertificateUseYears;
                    summaryData.AncestralLandArea = entity.AncestralLandArea;
                    summaryData.BasicFacilities = entity.BasicFacilities;
                    summaryData.HaveLandCertificate = entity.HaveLandCertificate;
                    summaryData.LandArea = entity.LandArea;
                    summaryData.LandExploitDegree = entity.LandExploitDegree;
                    summaryData.LandRightCertificateNo = entity.LandRightCertificateNo;
                    summaryData.LandWarrantPersonName = entity.LandWarrantPersonName;
                    summaryData.LandAcquisitionDate = entity.LandAcquisitionDate;
                    summaryData.UseYears = entity.UseYears;
                    summaryData.LocationRegion = entity.LocationRegion;
                    summaryData.HaveConstructionLandPlanningPermit = entity.HaveConstructionLandPlanningPermit;
                    summaryData.HaveAboveGroundBuilding = entity.HaveAboveGroundBuilding;
                    summaryData.AboveGroundBuildingConditionExplain = entity.AboveGroundBuildingConditionExplain;
                    summaryData.PeripheryEnvironmentCondition = entity.PeripheryEnvironmentCondition;
                    summaryData.LandPrice = entity.LandPrice;
                    summaryData.TransferPaymentCondition = entity.TransferPaymentCondition;
                    summaryData.MakeUpTransferFee = entity.MakeUpTransferFee;
                    summaryData.AncestralLandNo = entity.AncestralLandNo;
                    summaryData.AncestralLandFourDirectionLine = entity.AncestralLandFourDirectionLine;
                    summaryData.FloorAreaRatio = entity.FloorAreaRatio;
                    summaryData.BuildingDensity = entity.BuildingDensity;
                    summaryData.LandLevel = entity.LandLevel;
                    summaryData.LandUseMode = entity.LandUseMode;
                    summaryData.GroundAttachmentName = entity.GroundAttachmentName;
                    summaryData.GroundAttachmentAcreage = entity.GroundAttachmentAcreage;
                    summaryData.LandRemainingUseYears = entity.LandRemainingUseYears;
                    summaryData.EquityType = entity.EquityType;
                    summaryData.CertificateNo = entity.CertificateNo;
                    summaryData.IssuingUnit = entity.IssuingUnit;
                    summaryData.LatestNetAssetsPerShare = entity.LatestNetAssetsPerShare;
                    summaryData.Quantity = entity.Quantity;
                    summaryData.IssuePrice = entity.IssuePrice;
                    summaryData.PurchaseDate = entity.PurchaseDate;
                    summaryData.IsCompanyProfitable = entity.IsCompanyProfitable;
                    summaryData.CurrentManagementCondition = entity.CurrentManagementCondition;
                    summaryData.IsIndependentStore = entity.IsIndependentStore;
                    summaryData.FourDirectionRange = entity.FourDirectionRange;
                    summaryData.HaveInterlayer = entity.HaveInterlayer;
                    summaryData.StreetDepth = entity.StreetDepth;
                    summaryData.StreetWidth = entity.StreetWidth;
                    summaryData.HousePropertyRightPersonName = entity.HousePropertyRightPersonName;
                    summaryData.HouseType = entity.HouseType;
                    summaryData.BuildingType = entity.BuildingType;
                    summaryData.PlaneLayoutAdvantage = entity.PlaneLayoutAdvantage;
                    summaryData.Facilities = entity.Facilities;
                    summaryData.HavePrivateGarage = entity.HavePrivateGarage;
                    summaryData.IsUrbanArea = entity.IsUrbanArea;
                    summaryData.User = entity.User;
                    summaryData.OwnershipSituation = entity.OwnershipSituation;
                    summaryData.LandUseRightNature = entity.LandUseRightNature;
                    summaryData.LandAcquisitionMode = entity.LandAcquisitionMode;
                    summaryData.LandWarrantType = entity.LandWarrantType;
                    summaryData.LandCertificateNo = entity.LandCertificateNo;
                    summaryData.IssueDate = entity.IssueDate;
                    summaryData.ChargeEvaluationUnitPrice = entity.ChargeEvaluationUnitPrice;
                    summaryData.FirstOrSecondHand = entity.FirstOrSecondHand;
                    summaryData.EquipmentName = entity.EquipmentName;
                    summaryData.EquipmentNo = entity.EquipmentNo;
                    summaryData.MachineEquipmentType = entity.MachineEquipmentType;
                    summaryData.SpecificUse = entity.SpecificUse;
                    summaryData.EquipmentCondition = entity.EquipmentCondition;
                    summaryData.Brand = entity.Brand;
                    summaryData.SpecificationModel = entity.SpecificationModel;
                    summaryData.Manufacturer = entity.Manufacturer;
                    summaryData.ProductPlace = entity.ProductPlace;
                    summaryData.MeasurementUnit = entity.MeasurementUnit;
                    summaryData.HaveQualificationCertificate = entity.HaveQualificationCertificate;
                    summaryData.HaveSafetyCertificate = entity.HaveSafetyCertificate;
                    summaryData.HaveFireControlCertificate = entity.HaveFireControlCertificate;
                    summaryData.InvoiceNo = entity.InvoiceNo;
                    summaryData.Owner = entity.Owner;
                    summaryData.Place = entity.Place;
                    summaryData.EquipmentServiceLife = entity.EquipmentServiceLife;
                    summaryData.DepreciationCondition = entity.DepreciationCondition;
                    summaryData.OverhaulTimes = entity.OverhaulTimes;
                    summaryData.PowerFuel = entity.PowerFuel;
                    summaryData.LeaseCondition = entity.LeaseCondition;
                    summaryData.LeaseTerm = entity.LeaseTerm;
                    summaryData.AnnualRent = entity.AnnualRent;
                    summaryData.Power = entity.Power;
                    summaryData.PowerUnit = entity.PowerUnit;
                    summaryData.IsImportedEquipment = entity.IsImportedEquipment;
                    summaryData.IsCustomsControl = entity.IsCustomsControl;
                    summaryData.PurchaseCurrency = entity.PurchaseCurrency;
                    summaryData.ProductionDate = entity.ProductionDate;
                    summaryData.RegulatoryExpirationDate = entity.RegulatoryExpirationDate;
                    #endregion
                    var sendReport = new ApiModelSendReportRequest()
                    {
                        yewuxitongbaogaoId = project.BusinessId,
                        huizongxinxi = summaryData,
                        baogaourl = domainName + resource.FilePath,
                        baogaoType = BaogaoTypeEnum.预估
                    };
                    LogHelper.Error(project.ProjectNo + "预估数据Json：" + sendReport.ToJson(), null);
                    var response = new FangguguApiService().SendReport(sendReport);
                    LogHelper.Error(project.ProjectNo + "预估数据返回结果：" + response.ToJson() + "，返回时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null);
                    if (!response.Success)
                    {
                        LogHelper.Error(project.ProjectNo + "发送报告预估失败，返回信息：" + response.Msg, null);
                    }
                    var sendStatus = new ApiModelSendStatusRequest()
                    {
                        yewuxitongbaogaoId = project.BusinessId,
                        status = ((int)ProjectStatusEnum.报告预估).ToString(),
                        feedback_description = note
                    };
                    LogHelper.Error(project.ProjectNo + "报告预估Json：" + sendReport.ToJson(), null);
                    response = new FangguguApiService().SendStatus(sendStatus);
                    LogHelper.Error(project.ProjectNo + "报告预估返回结果：" + response.ToJson() + "，返回时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null);
                    if (!response.Success)
                    {
                        LogHelper.Error(project.ProjectNo + "发送报告预估状态失败，返回信息：" + response.Msg, null);
                    }
                }).ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        LogHelper.Error(project.ProjectNo + "发送预估数据及报告预估状态失败", t.Exception);
                    }
                });
                //流程记录
                var projectState = new ProjectStateInfo()
                {
                    ProjectId = project.Id,
                    Content = ProjectStatusEnum.报告预估.ToString(),
                    OperationTime = DateTime.Now,
                    Operator = user.UserName,
                    Note = note
                };
                ProjectStateInfoRepository.Instance.Insert(projectState);
            }
            return result;
        }

        /// <summary>
        /// 报告准备
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool OperateReportPrepare(long projectId, long userId, string note)
        {
            var project = ProjectRepository.Instance.Source.FirstOrDefault(x => x.Id == projectId);
            if (project == null)
            {
                throw new ServiceException("项目不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.未受理)
            {
                throw new ServiceException("项目还未受理，请先受理!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该项目已撤单。");
            }
            if (project.ProjectStatus == ProjectStatusEnum.发送报告)
            {
                throw new ServiceException("您好，报告已发送，请勿再继续操作。");
            }
            project.ProjectStatus = ProjectStatusEnum.报告准备;
            var result = ProjectRepository.Instance.Save(project);
            if (result)
            {
                Task.Factory.StartNew(() =>
                {
                    var request = new ApiModelSendStatusRequest()
                    {
                        yewuxitongbaogaoId = project.BusinessId,
                        status = ((int)ProjectStatusEnum.报告准备).ToString(),
                        feedback_description = note
                    };
                    LogHelper.Error(project.ProjectNo + "发送报告准备Json：" + request.ToJson(), null);
                    var response = new FangguguApiService().SendStatus(request);
                    if (!response.Success)
                    {
                        LogHelper.Error(project.ProjectNo + "发送报告准备状态失败，返回信息：" + response.Msg, null);
                    }
                }).ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        LogHelper.Error(project.ProjectNo + "发送报告准备状态失败", t.Exception);
                    }
                });
                //流程记录
                var projectState = new ProjectStateInfo()
                {
                    ProjectId = project.Id,
                    Content = ProjectStatusEnum.报告准备.ToString(),
                    OperationTime = DateTime.Now,
                    Operator = user.UserName,
                    Note = note
                };
                ProjectStateInfoRepository.Instance.Insert(projectState);
            }
            return result;
        }

        ///// <summary>
        ///// 报告审核
        ///// </summary>
        ///// <param name="projectId"></param>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //public bool OperateReportAudit(long projectId, long userId)
        //{
        //    var project = ProjectRepository.Instance.Source.FirstOrDefault(x => x.Id == projectId);
        //    if (project == null)
        //    {
        //        throw new ServiceException("项目不存在!");
        //    }
        //    var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
        //    if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
        //    {
        //        throw new ServiceException("您无权操作此项目!");
        //    }
        //    if (project.ProjectStatus == ProjectStatusEnum.未受理)
        //    {
        //        throw new ServiceException("项目还未受理，请先受理!");
        //    }
        //    if (project.ProjectStatus == ProjectStatusEnum.已撤单)
        //    {
        //        throw new ServiceException("您好，该项目已撤单。");
        //    }
        //    if (project.ProjectStatus == ProjectStatusEnum.发送报告)
        //    {
        //        throw new ServiceException("您好，报告已发送，请勿再继续操作。");
        //    }
        //    project.ProjectStatus = ProjectStatusEnum.报告审核;
        //    var result = ProjectRepository.Instance.Save(project);
        //    if (result)
        //    {
        //        Task.Factory.StartNew(() =>
        //        {
        //            var request = new ApiModelSendStatusRequest()
        //            {
        //                yewuxitongbaogaoId = project.BusinessId,
        //                status = ((int)ProjectStatusEnum.报告审核).ToString(),
        //                note = ProjectStatusEnum.报告审核.ToString()
        //            };
        //            var response = new FangguguApiService().SendStatus(request);
        //            if (!response.Success)
        //            {
        //                LogHelper.Error(project.ProjectNo + "发送项目受理状态失败，返回信息：" + response.Msg, null);
        //            }
        //        }).ContinueWith((t) =>
        //        {
        //            if (t.Exception != null)
        //            {
        //                LogHelper.Error(project.ProjectNo + "发送报告审核状态失败", t.Exception);
        //            }
        //        });
        //        //流程记录
        //        var projectState = new ProjectStateInfo()
        //        {
        //            ProjectId = project.Id,
        //            Content = ProjectStatusEnum.报告审核.ToString(),
        //            OperationTime = DateTime.Now,
        //            Operator = user.UserName,
        //        };
        //        ProjectStateInfoRepository.Instance.Insert(projectState);
        //    }
        //    return result;
        //}

        /// <summary>
        /// 发送报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="entity"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool OperateReportSend(long projectId, SummaryData entity, long userId, string note)
        {
            var project = ProjectRepository.Instance.Source.FirstOrDefault(x => x.Id == projectId);
            if (project == null)
            {
                throw new ServiceException("项目不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.未受理)
            {
                throw new ServiceException("项目还未受理，请先受理!");
            }
            if (project.ProjectStatus == ProjectStatusEnum.已撤单)
            {
                throw new ServiceException("您好，该项目已撤单。");
            }
            var resource = GetLastUploadReportResource(projectId);
            if (resource == null)
            {
                throw new ServiceException("您还未上传报告，请先上传报告再保存汇总数据");
            }
            if (!project.SummaryData.CreateTime.HasValue)
            {
                project.SummaryData.CreateTime = DateTime.Now;
            }
            #region 汇总字段
            project.SummaryData.CompanyName = entity.CompanyName;
            project.SummaryData.ChargeName = entity.ChargeName;
            project.SummaryData.ChargeType = entity.ChargeType;
            project.SummaryData.PledgeName = entity.PledgeName;
            project.SummaryData.EvaluationMethod = entity.EvaluationMethod;
            project.SummaryData.WarrantsType = entity.WarrantsType;
            project.SummaryData.WarrantNo = entity.WarrantNo;
            project.SummaryData.ValuationValue = entity.ValuationValue;
            project.SummaryData.AssessmentFees = entity.AssessmentFees;
            project.SummaryData.Country = entity.Country;
            project.SummaryData.Province = entity.Province;
            project.SummaryData.City = entity.City;
            project.SummaryData.District = entity.District;
            project.SummaryData.Address = entity.Address;
            project.SummaryData.Building = entity.Building;
            project.SummaryData.TotalFloor = entity.TotalFloor;
            project.SummaryData.Floor = entity.Floor;
            project.SummaryData.RoomNo = entity.RoomNo;
            project.SummaryData.FloorHeight = entity.FloorHeight;
            project.SummaryData.StructureArea = entity.StructureArea;
            project.SummaryData.UseLandArea = entity.UseLandArea;
            project.SummaryData.ApportionmentArae = entity.ApportionmentArae;
            project.SummaryData.BuildYear = entity.BuildYear;
            project.SummaryData.BuildingStructure = entity.BuildingStructure;
            project.SummaryData.DurableYears = entity.DurableYears;
            project.SummaryData.Toward = entity.Toward;
            project.SummaryData.MaintenanceCondition = entity.MaintenanceCondition;
            project.SummaryData.DecorationDegree = entity.DecorationDegree;
            project.SummaryData.PublicFacilities = entity.PublicFacilities;
            project.SummaryData.HaveElevator = entity.HaveElevator;
            project.SummaryData.Environment = entity.Environment;
            project.SummaryData.UseState = entity.UseState;
            project.SummaryData.LeasePeriod = entity.LeasePeriod;
            project.SummaryData.PeriodicUnit = entity.PeriodicUnit;
            project.SummaryData.IdlePeriod = entity.IdlePeriod;
            project.SummaryData.StreetCondition = entity.StreetCondition;
            project.SummaryData.TrafficCondition = entity.TrafficCondition;
            project.SummaryData.FlourishingDegree = entity.FlourishingDegree;
            project.SummaryData.EstimatedCompletionDate = entity.EstimatedCompletionDate;
            project.SummaryData.LandUse = entity.LandUse;
            project.SummaryData.LandOwnershipProperties = entity.LandOwnershipProperties;
            project.SummaryData.LandUseRightType = entity.LandUseRightType;
            project.SummaryData.LandUseCertificateNo = entity.LandUseCertificateNo;
            project.SummaryData.LandUseStartDate = entity.LandUseStartDate;
            project.SummaryData.LandUseEndDate = entity.LandUseEndDate;
            project.SummaryData.LandUseRightCompany = entity.LandUseRightCompany;
            project.SummaryData.LandUseYears = entity.LandUseYears;
            project.SummaryData.PurchasePrice = entity.PurchasePrice;
            project.SummaryData.BuyingTime = entity.BuyingTime;
            project.SummaryData.Remark = entity.Remark;
            project.SummaryData.ResidentialDistrict = entity.ResidentialDistrict;
            project.SummaryData.AccessoryArea = entity.AccessoryArea;
            project.SummaryData.PurchaseContractNo = entity.PurchaseContractNo;
            project.SummaryData.PropertyRightCertificateUseYears = entity.PropertyRightCertificateUseYears;
            project.SummaryData.AncestralLandArea = entity.AncestralLandArea;
            project.SummaryData.BasicFacilities = entity.BasicFacilities;
            project.SummaryData.HaveLandCertificate = entity.HaveLandCertificate;
            project.SummaryData.LandArea = entity.LandArea;
            project.SummaryData.LandExploitDegree = entity.LandExploitDegree;
            project.SummaryData.LandRightCertificateNo = entity.LandRightCertificateNo;
            project.SummaryData.LandWarrantPersonName = entity.LandWarrantPersonName;
            project.SummaryData.LandAcquisitionDate = entity.LandAcquisitionDate;
            project.SummaryData.UseYears = entity.UseYears;
            project.SummaryData.LocationRegion = entity.LocationRegion;
            project.SummaryData.HaveConstructionLandPlanningPermit = entity.HaveConstructionLandPlanningPermit;
            project.SummaryData.HaveAboveGroundBuilding = entity.HaveAboveGroundBuilding;
            project.SummaryData.AboveGroundBuildingConditionExplain = entity.AboveGroundBuildingConditionExplain;
            project.SummaryData.PeripheryEnvironmentCondition = entity.PeripheryEnvironmentCondition;
            project.SummaryData.LandPrice = entity.LandPrice;
            project.SummaryData.TransferPaymentCondition = entity.TransferPaymentCondition;
            project.SummaryData.MakeUpTransferFee = entity.MakeUpTransferFee;
            project.SummaryData.AncestralLandNo = entity.AncestralLandNo;
            project.SummaryData.AncestralLandFourDirectionLine = entity.AncestralLandFourDirectionLine;
            project.SummaryData.FloorAreaRatio = entity.FloorAreaRatio;
            project.SummaryData.BuildingDensity = entity.BuildingDensity;
            project.SummaryData.LandLevel = entity.LandLevel;
            project.SummaryData.LandUseMode = entity.LandUseMode;
            project.SummaryData.GroundAttachmentName = entity.GroundAttachmentName;
            project.SummaryData.GroundAttachmentAcreage = entity.GroundAttachmentAcreage;
            project.SummaryData.LandRemainingUseYears = entity.LandRemainingUseYears;
            project.SummaryData.EquityType = entity.EquityType;
            project.SummaryData.CertificateNo = entity.CertificateNo;
            project.SummaryData.IssuingUnit = entity.IssuingUnit;
            project.SummaryData.LatestNetAssetsPerShare = entity.LatestNetAssetsPerShare;
            project.SummaryData.Quantity = entity.Quantity;
            project.SummaryData.IssuePrice = entity.IssuePrice;
            project.SummaryData.PurchaseDate = entity.PurchaseDate;
            project.SummaryData.IsCompanyProfitable = entity.IsCompanyProfitable;
            project.SummaryData.CurrentManagementCondition = entity.CurrentManagementCondition;
            project.SummaryData.IsIndependentStore = entity.IsIndependentStore;
            project.SummaryData.FourDirectionRange = entity.FourDirectionRange;
            project.SummaryData.HaveInterlayer = entity.HaveInterlayer;
            project.SummaryData.StreetDepth = entity.StreetDepth;
            project.SummaryData.StreetWidth = entity.StreetWidth;
            project.SummaryData.HousePropertyRightPersonName = entity.HousePropertyRightPersonName;
            project.SummaryData.HouseType = entity.HouseType;
            project.SummaryData.BuildingType = entity.BuildingType;
            project.SummaryData.PlaneLayoutAdvantage = entity.PlaneLayoutAdvantage;
            project.SummaryData.Facilities = entity.Facilities;
            project.SummaryData.HavePrivateGarage = entity.HavePrivateGarage;
            project.SummaryData.IsUrbanArea = entity.IsUrbanArea;
            project.SummaryData.User = entity.User;
            project.SummaryData.OwnershipSituation = entity.OwnershipSituation;
            project.SummaryData.LandUseRightNature = entity.LandUseRightNature;
            project.SummaryData.LandAcquisitionMode = entity.LandAcquisitionMode;
            project.SummaryData.LandWarrantType = entity.LandWarrantType;
            project.SummaryData.LandCertificateNo = entity.LandCertificateNo;
            project.SummaryData.IssueDate = entity.IssueDate;
            project.SummaryData.ChargeEvaluationUnitPrice = entity.ChargeEvaluationUnitPrice;
            project.SummaryData.FirstOrSecondHand = entity.FirstOrSecondHand;
            project.SummaryData.EquipmentName = entity.EquipmentName;
            project.SummaryData.EquipmentNo = entity.EquipmentNo;
            project.SummaryData.MachineEquipmentType = entity.MachineEquipmentType;
            project.SummaryData.SpecificUse = entity.SpecificUse;
            project.SummaryData.EquipmentCondition = entity.EquipmentCondition;
            project.SummaryData.Brand = entity.Brand;
            project.SummaryData.SpecificationModel = entity.SpecificationModel;
            project.SummaryData.Manufacturer = entity.Manufacturer;
            project.SummaryData.ProductPlace = entity.ProductPlace;
            project.SummaryData.MeasurementUnit = entity.MeasurementUnit;
            project.SummaryData.HaveQualificationCertificate = entity.HaveQualificationCertificate;
            project.SummaryData.HaveSafetyCertificate = entity.HaveSafetyCertificate;
            project.SummaryData.HaveFireControlCertificate = entity.HaveFireControlCertificate;
            project.SummaryData.InvoiceNo = entity.InvoiceNo;
            project.SummaryData.Owner = entity.Owner;
            project.SummaryData.Place = entity.Place;
            project.SummaryData.EquipmentServiceLife = entity.EquipmentServiceLife;
            project.SummaryData.DepreciationCondition = entity.DepreciationCondition;
            project.SummaryData.OverhaulTimes = entity.OverhaulTimes;
            project.SummaryData.PowerFuel = entity.PowerFuel;
            project.SummaryData.LeaseCondition = entity.LeaseCondition;
            project.SummaryData.LeaseTerm = entity.LeaseTerm;
            project.SummaryData.AnnualRent = entity.AnnualRent;
            project.SummaryData.Power = entity.Power;
            project.SummaryData.PowerUnit = entity.PowerUnit;
            project.SummaryData.IsImportedEquipment = entity.IsImportedEquipment;
            project.SummaryData.IsCustomsControl = entity.IsCustomsControl;
            project.SummaryData.PurchaseCurrency = entity.PurchaseCurrency;
            project.SummaryData.ProductionDate = entity.ProductionDate;
            project.SummaryData.RegulatoryExpirationDate = entity.RegulatoryExpirationDate;
            #endregion
            project.ProjectStatus = ProjectStatusEnum.发送报告;
            var result = ProjectRepository.Instance.Save(project);
            var domainName = ConfigurationManager.AppSettings["DomainName"];
            if (string.IsNullOrEmpty(domainName))
            {
                domainName = HttpContext.Current.Request.Url.Host +
                                     (HttpContext.Current.Request.Url.Port == 80
                                         ? ""
                                         : (":" + HttpContext.Current.Request.Url.Port));
            }
            if (result)
            {
                Task.Factory.StartNew(() =>
                {
                    var summaryData = new ApiModelSummaryDataRequest();
                    #region 汇总字段
                    summaryData.CompanyName = entity.CompanyName;
                    summaryData.ChargeName = entity.ChargeName;
                    summaryData.ChargeType = entity.ChargeType;
                    summaryData.PledgeName = entity.PledgeName;
                    summaryData.EvaluationMethod = entity.EvaluationMethod;
                    summaryData.WarrantsType = entity.WarrantsType;
                    summaryData.WarrantNo = entity.WarrantNo;
                    summaryData.ValuationValue = entity.ValuationValue;
                    summaryData.AssessmentFees = entity.AssessmentFees;
                    summaryData.Country = entity.Country;
                    summaryData.Province = entity.Province;
                    summaryData.City = entity.City;
                    summaryData.District = entity.District;
                    summaryData.Address = entity.Address;
                    summaryData.Building = entity.Building;
                    summaryData.TotalFloor = entity.TotalFloor;
                    summaryData.Floor = entity.Floor;
                    summaryData.RoomNo = entity.RoomNo;
                    summaryData.FloorHeight = entity.FloorHeight;
                    summaryData.StructureArea = entity.StructureArea;
                    summaryData.UseLandArea = entity.UseLandArea;
                    summaryData.ApportionmentArae = entity.ApportionmentArae;
                    summaryData.BuildYear = entity.BuildYear;
                    summaryData.BuildingStructure = entity.BuildingStructure;
                    summaryData.DurableYears = entity.DurableYears;
                    summaryData.Toward = entity.Toward;
                    summaryData.MaintenanceCondition = entity.MaintenanceCondition;
                    summaryData.DecorationDegree = entity.DecorationDegree;
                    summaryData.PublicFacilities = entity.PublicFacilities;
                    summaryData.HaveElevator = entity.HaveElevator;
                    summaryData.Environment = entity.Environment;
                    summaryData.UseState = entity.UseState;
                    summaryData.LeasePeriod = entity.LeasePeriod;
                    summaryData.PeriodicUnit = entity.PeriodicUnit;
                    summaryData.IdlePeriod = entity.IdlePeriod;
                    summaryData.StreetCondition = entity.StreetCondition;
                    summaryData.TrafficCondition = entity.TrafficCondition;
                    summaryData.FlourishingDegree = entity.FlourishingDegree;
                    summaryData.EstimatedCompletionDate = entity.EstimatedCompletionDate;
                    summaryData.LandUse = entity.LandUse;
                    summaryData.LandOwnershipProperties = entity.LandOwnershipProperties;
                    summaryData.LandUseRightType = entity.LandUseRightType;
                    summaryData.LandUseCertificateNo = entity.LandUseCertificateNo;
                    summaryData.LandUseStartDate = entity.LandUseStartDate;
                    summaryData.LandUseEndDate = entity.LandUseEndDate;
                    summaryData.LandUseRightCompany = entity.LandUseRightCompany;
                    summaryData.LandUseYears = entity.LandUseYears;
                    summaryData.PurchasePrice = entity.PurchasePrice;
                    summaryData.BuyingTime = entity.BuyingTime;
                    summaryData.Remark = entity.Remark;
                    summaryData.ResidentialDistrict = entity.ResidentialDistrict;
                    summaryData.AccessoryArea = entity.AccessoryArea;
                    summaryData.PurchaseContractNo = entity.PurchaseContractNo;
                    summaryData.PropertyRightCertificateUseYears = entity.PropertyRightCertificateUseYears;
                    summaryData.AncestralLandArea = entity.AncestralLandArea;
                    summaryData.BasicFacilities = entity.BasicFacilities;
                    summaryData.HaveLandCertificate = entity.HaveLandCertificate;
                    summaryData.LandArea = entity.LandArea;
                    summaryData.LandExploitDegree = entity.LandExploitDegree;
                    summaryData.LandRightCertificateNo = entity.LandRightCertificateNo;
                    summaryData.LandWarrantPersonName = entity.LandWarrantPersonName;
                    summaryData.LandAcquisitionDate = entity.LandAcquisitionDate;
                    summaryData.UseYears = entity.UseYears;
                    summaryData.LocationRegion = entity.LocationRegion;
                    summaryData.HaveConstructionLandPlanningPermit = entity.HaveConstructionLandPlanningPermit;
                    summaryData.HaveAboveGroundBuilding = entity.HaveAboveGroundBuilding;
                    summaryData.AboveGroundBuildingConditionExplain = entity.AboveGroundBuildingConditionExplain;
                    summaryData.PeripheryEnvironmentCondition = entity.PeripheryEnvironmentCondition;
                    summaryData.LandPrice = entity.LandPrice;
                    summaryData.TransferPaymentCondition = entity.TransferPaymentCondition;
                    summaryData.MakeUpTransferFee = entity.MakeUpTransferFee;
                    summaryData.AncestralLandNo = entity.AncestralLandNo;
                    summaryData.AncestralLandFourDirectionLine = entity.AncestralLandFourDirectionLine;
                    summaryData.FloorAreaRatio = entity.FloorAreaRatio;
                    summaryData.BuildingDensity = entity.BuildingDensity;
                    summaryData.LandLevel = entity.LandLevel;
                    summaryData.LandUseMode = entity.LandUseMode;
                    summaryData.GroundAttachmentName = entity.GroundAttachmentName;
                    summaryData.GroundAttachmentAcreage = entity.GroundAttachmentAcreage;
                    summaryData.LandRemainingUseYears = entity.LandRemainingUseYears;
                    summaryData.EquityType = entity.EquityType;
                    summaryData.CertificateNo = entity.CertificateNo;
                    summaryData.IssuingUnit = entity.IssuingUnit;
                    summaryData.LatestNetAssetsPerShare = entity.LatestNetAssetsPerShare;
                    summaryData.Quantity = entity.Quantity;
                    summaryData.IssuePrice = entity.IssuePrice;
                    summaryData.PurchaseDate = entity.PurchaseDate;
                    summaryData.IsCompanyProfitable = entity.IsCompanyProfitable;
                    summaryData.CurrentManagementCondition = entity.CurrentManagementCondition;
                    summaryData.IsIndependentStore = entity.IsIndependentStore;
                    summaryData.FourDirectionRange = entity.FourDirectionRange;
                    summaryData.HaveInterlayer = entity.HaveInterlayer;
                    summaryData.StreetDepth = entity.StreetDepth;
                    summaryData.StreetWidth = entity.StreetWidth;
                    summaryData.HousePropertyRightPersonName = entity.HousePropertyRightPersonName;
                    summaryData.HouseType = entity.HouseType;
                    summaryData.BuildingType = entity.BuildingType;
                    summaryData.PlaneLayoutAdvantage = entity.PlaneLayoutAdvantage;
                    summaryData.Facilities = entity.Facilities;
                    summaryData.HavePrivateGarage = entity.HavePrivateGarage;
                    summaryData.IsUrbanArea = entity.IsUrbanArea;
                    summaryData.User = entity.User;
                    summaryData.OwnershipSituation = entity.OwnershipSituation;
                    summaryData.LandUseRightNature = entity.LandUseRightNature;
                    summaryData.LandAcquisitionMode = entity.LandAcquisitionMode;
                    summaryData.LandWarrantType = entity.LandWarrantType;
                    summaryData.LandCertificateNo = entity.LandCertificateNo;
                    summaryData.IssueDate = entity.IssueDate;
                    summaryData.ChargeEvaluationUnitPrice = entity.ChargeEvaluationUnitPrice;
                    summaryData.FirstOrSecondHand = entity.FirstOrSecondHand;
                    summaryData.EquipmentName = entity.EquipmentName;
                    summaryData.EquipmentNo = entity.EquipmentNo;
                    summaryData.MachineEquipmentType = entity.MachineEquipmentType;
                    summaryData.SpecificUse = entity.SpecificUse;
                    summaryData.EquipmentCondition = entity.EquipmentCondition;
                    summaryData.Brand = entity.Brand;
                    summaryData.SpecificationModel = entity.SpecificationModel;
                    summaryData.Manufacturer = entity.Manufacturer;
                    summaryData.ProductPlace = entity.ProductPlace;
                    summaryData.MeasurementUnit = entity.MeasurementUnit;
                    summaryData.HaveQualificationCertificate = entity.HaveQualificationCertificate;
                    summaryData.HaveSafetyCertificate = entity.HaveSafetyCertificate;
                    summaryData.HaveFireControlCertificate = entity.HaveFireControlCertificate;
                    summaryData.InvoiceNo = entity.InvoiceNo;
                    summaryData.Owner = entity.Owner;
                    summaryData.Place = entity.Place;
                    summaryData.EquipmentServiceLife = entity.EquipmentServiceLife;
                    summaryData.DepreciationCondition = entity.DepreciationCondition;
                    summaryData.OverhaulTimes = entity.OverhaulTimes;
                    summaryData.PowerFuel = entity.PowerFuel;
                    summaryData.LeaseCondition = entity.LeaseCondition;
                    summaryData.LeaseTerm = entity.LeaseTerm;
                    summaryData.AnnualRent = entity.AnnualRent;
                    summaryData.Power = entity.Power;
                    summaryData.PowerUnit = entity.PowerUnit;
                    summaryData.IsImportedEquipment = entity.IsImportedEquipment;
                    summaryData.IsCustomsControl = entity.IsCustomsControl;
                    summaryData.PurchaseCurrency = entity.PurchaseCurrency;
                    summaryData.ProductionDate = entity.ProductionDate;
                    summaryData.RegulatoryExpirationDate = entity.RegulatoryExpirationDate;
                    #endregion
                    var sendReport = new ApiModelSendReportRequest()
                    {
                        yewuxitongbaogaoId = project.BusinessId,
                        huizongxinxi = summaryData,
                        baogaourl = domainName + resource.FilePath,
                        baogaoType = BaogaoTypeEnum.正式
                    };
                    LogHelper.Error(project.ProjectNo + "发送汇总数据Json：" + sendReport.ToJson(), null);
                    var response=new FangguguApiService().SendReport(sendReport);
                    LogHelper.Error(project.ProjectNo + "发送汇总数据返回结果：" + response.ToJson() + "，返回时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null);
                    if (!response.Success)
                    {
                        LogHelper.Error(project.ProjectNo + "发送汇总数据失败，返回信息：" + response.Msg, null);
                    }
                    var sendStatus = new ApiModelSendStatusRequest()
                    {
                        yewuxitongbaogaoId = project.BusinessId,
                        status = ((int)ProjectStatusEnum.发送报告).ToString(),
                        feedback_description = note
                    };
                    response = new FangguguApiService().SendStatus(sendStatus);
                    LogHelper.Error(project.ProjectNo + "发送报告Json：" + sendStatus.ToJson(), null);
                    LogHelper.Error(project.ProjectNo + "发送报告返回结果：" + response.ToJson() + "，返回时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), null);
                    if (!response.Success)
                    {
                        LogHelper.Error(project.ProjectNo + "发送报告发送状态失败，返回信息：" + response.Msg, null);
                    }
                }).ContinueWith((t) =>
                {
                    if (t.Exception != null)
                    {
                        LogHelper.Error(project.ProjectNo + "发送汇总数据及发送报告状态失败", t.Exception);
                    }
                });
                //流程记录
                var projectState = new ProjectStateInfo()
                {
                    ProjectId = project.Id,
                    Content = ProjectStatusEnum.发送报告.ToString(),
                    OperationTime = DateTime.Now,
                    Operator = user.UserName,
                    Note = note
                };
                ProjectStateInfoRepository.Instance.Insert(projectState);
            }
            return result;
        }


        /// <summary>
        /// 根据业务编号获取项目
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>
        public Project GetProject(string businessId)
        {
            var project = ProjectRepository.Instance.Find(x => x.BusinessId == businessId).FirstOrDefault();
            if (project == null)
            {
                project = ProjectRepository.Instance.Find(x => x.ProjectNo == businessId).FirstOrDefault();
            }
            return project;
        }

        /// <summary>
        /// 接单
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="resources"></param>
        /// <param name="companyName"></param>
        /// <returns></returns>
        public Project ReceiveProject(Project entity, IList<ProjectResource> resources, string companyName)
        {
            entity.ProjectNo = GetInstanceNo(1).FirstOrDefault();

            var company = CompanyRepository.Instance.Source.FirstOrDefault(x => x.CompanyName == companyName);
            if (company == null)
            {
                company = CompanyService.Instance.GetByCompanyId(entity.CompanyId);
                if (company == null)
                {
                    throw new ServiceException("评估机构参数错误，找不到所属评估公司");
                }
            }
            entity.Company = company;

            ProjectRepository.Instance.Transaction(() =>
            {
                entity.CreateTime = DateTime.Now;
                entity.ProjectStatus = ProjectStatusEnum.未受理;

                //外采
                var outTask = new OuterTask()
                {
                    //CreateTime = DateTime.Now
                };
                entity.OuterTask = outTask;

                //汇总数据
                var summaryData = new SummaryData()
                {
                    //CreateTime = DateTime.Now
                    CompanyName = entity.Company.CompanyName,
                    ChargeType = entity.PropertyType,
                    PledgeName = entity.PledgePerson
                };
                entity.SummaryData = summaryData;


                ProjectRepository.Instance.Insert(entity);

                ////流程记录
                //var projectState = new ProjectStateInfo();
                //projectState.ProjectId = entity.Id;
                //projectState.Content = "未受理";
                //projectState.OperationTime = DateTime.Now;
                //projectState.Operator = string.Format("{0}", ConfigurationManager.AppSettings["DefaultUser"]);
                //ProjectStateInfoRepository.Instance.Insert(projectState);

                //附件
                foreach (var resource in resources)
                {
                    #region 转换成本网站url

                    byte[] data;
                    using (var web = new WebClient())
                    {
                        try
                        {
                            data = web.DownloadData(resource.FilePath);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(
                                entity.ProjectNo + "项目资源《" + resource.FileName + "》下载失败，url：" + resource.FilePath, ex);
                            continue;
                        }
                    }
                    var path = FileStreamHelper.GetUploadFilePath();
                    string fullPath = FileStreamHelper.UrlConvertorLocal(path);
                    if (!Directory.Exists(fullPath))
                    {
                        Directory.CreateDirectory(fullPath);
                    }
                    var fileName = resource.FileName;
                    if (!fileName.Contains("."))
                    {
                        if (!string.IsNullOrEmpty(resource.FileFormat))
                        {
                            fileName = fileName + "." + resource.FileFormat;
                        }
                        else
                        {
                            fileName = fileName + ".jpg";
                        }
                    }
                    fileName = fileName.Replace("..", ".");
                    fullPath += fileName;
                    data.SaveAs(fullPath);
                    var url =
                        ("/" + path + fileName).Replace(@"\", @"/");
                    resource.FilePath = url;

                    #endregion

                    resource.ProjectId = entity.Id;
                    resource.CreateTime = DateTime.Now;
                    resource.ResourcesType = ResourcesEnum.附件;
                    ProjectResourceRepository.Instance.Insert(resource);
                }
            });
            if (entity.Id > 0)
            {
                Task.Factory.StartNew(() =>
                {
                    var user = UserRepository.Instance.Source.FirstOrDefault(x => x.CompanyId == company.Id && x.IsAdmin);
                    if (user == null)
                    {
                        LogHelper.Error("【" + companyName + "】没有默认的管理人员，无法发送短信", null);
                    }
                    else if (string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        LogHelper.Error("【" + companyName + "】管理人员" + "【" + user.UserName + "】没有配置手机号码，无法发送短信", null);
                    }
                    else
                    {
                        new EciticApiService().SendSms(new ApiModleSmsRequest()
                        {
                            teleno = user.PhoneNumber,
                            msg =
                                string.Format("【{0}】评估业务在{1}已发送至贵方系统，请尽快处理。", entity.PledgeAddress,
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        });
                    }
                });
            }
            return entity;
        }

        /// <summary>
        /// 生成流水号
        /// </summary>
        internal static string GetInstanceNo()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            var instanceNo = BitConverter.ToInt64(buffer, 0).ToString().Substring(0, 12);
            if (ProjectRepository.Instance.Find(x => x.ProjectNo == instanceNo).Any() || RevaluationItemRepository.Instance.Find(x => x.ProjectNo == instanceNo).Any())
            {
                instanceNo = GetInstanceNo();
            }
            return instanceNo;
        }

        /// <summary>
        /// 生成流水号
        /// </summary>
        internal static List<string> GetInstanceNo(int length)
        {
            if (length <= 0)
            {
                return new List<string>(0);
            }
            var result = new List<string>(length);
            var timeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000)/10000000;
            var max = (10000 - 1000)/length - 1;
            var rand = new Random().Next(1, max);
            for (int i = 0; i < length; i++)
            {
                var head = rand*length + 1000 + i;
                result.Add(string.Format("{0}{1}", head, timeStamp));
            }
            return result;
        }

        /// <summary>
        /// 撤单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reason"></param>
        public void RevokeProject(long id,string reason)
        {
            var project = ProjectRepository.Instance.Find(x => x.Id == id).FirstOrDefault();
            project.ProjectStatus = ProjectStatusEnum.已撤单;
            project.RevokeReason = reason;
            if (ProjectRepository.Instance.Save(project))
            {
                Task.Factory.StartNew(() =>
                {
                    var user = UserRepository.Instance.Source.FirstOrDefault(x => x.CompanyId == project.Company.Id && x.IsAdmin);
                    if (user == null)
                    {
                        LogHelper.Error("【" + project.Company.CompanyName + "】没有默认的管理人员，无法发送短信", null);
                    }
                    else if (string.IsNullOrEmpty(user.PhoneNumber))
                    {
                        LogHelper.Error("【" + project.Company.CompanyName + "】管理人员" + "【" + user.UserName + "】没有配置手机号码，无法发送短信", null);
                    }
                    else
                    {
                        new EciticApiService().SendSms(new ApiModleSmsRequest()
                        {
                            teleno = user.PhoneNumber,
                            msg =
                                string.Format("【{0}】评估业务在{1}已被撤销，请停止相关业务。", project.PledgeAddress,
                                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                        });
                    }
                });
            }
        }

        /// <summary>
        /// 下载资料
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public byte[] DownLoadFiles(long projectId, long userId)
        {
            var memoryStream = new MemoryStream();
            byte[] result = null;
            var project = ProjectRepository.Instance.Find(x => x.Id == projectId).FirstOrDefault();
            if (project == null)
            {
                return null;
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            using (var zipStream = new ZipOutputStream(memoryStream))
            {
                DownWebResource(zipStream, project);
                zipStream.Finish();
                result = memoryStream.ToArray();
                zipStream.Close();
            }
            return result;
        }

        /// <summary>
        /// 导出汇总数据
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public byte[] DownLoadSummaryData(long projectId, long userId)
        {
            byte[] result = null;
            var project = ProjectRepository.Instance.Find(x => x.Id == projectId).FirstOrDefault();
            if (project == null)
            {
                return null;
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            if (!project.SummaryData.CreateTime.HasValue)
            {
                throw new ServiceException("汇总数据未填写，无法导出!");
            }
            var fieldList = new Dictionary<string, string>();
            switch (project.PropertyType)
            {
                #region 根据项目类型添加字段

                case "办公":
                    fieldList.Add("Country", "国家(*)");
                    fieldList.Add("Province", "省/直辖市/自治区(*)");
                    fieldList.Add("City", "城市/自治州(*)");
                    fieldList.Add("District", "区（县）(*)");
                    fieldList.Add("Address", "座落地址(*)");
                    fieldList.Add("Building", "所在楼座");
                    fieldList.Add("TotalFloor", "楼层总层数(*)");
                    fieldList.Add("Floor", "所在楼层(*)");
                    fieldList.Add("RoomNo", "具体房号");
                    fieldList.Add("FloorHeight", "层高（米）");
                    fieldList.Add("StructureArea", "建筑面积（平方米）(*)");
                    fieldList.Add("UseLandArea", "用地面积（平方米）");
                    fieldList.Add("ApportionmentArae", "分摊土地面积（平方米）");
                    fieldList.Add("BuildYear", "建成年代(*)");
                    fieldList.Add("BuildingStructure", "建筑结构(*)");
                    fieldList.Add("DurableYears", "耐用年限(*)");
                    fieldList.Add("Toward", "朝向(*)");
                    fieldList.Add("MaintenanceCondition", "维护状况(*)");
                    fieldList.Add("DecorationDegree", "装修程度(*)");
                    fieldList.Add("PublicFacilities", "公共配套设施");
                    fieldList.Add("HaveElevator", "是否带电梯(*)");
                    fieldList.Add("Environment", "环境（质量、景观）");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("LeasePeriod", "租赁周期");
                    fieldList.Add("PeriodicUnit", "周期单位");
                    fieldList.Add("IdlePeriod", "闲置期（月）");
                    fieldList.Add("StreetCondition", "临街状况(*)");
                    fieldList.Add("TrafficCondition", "交通条件");
                    fieldList.Add("FlourishingDegree", "繁华度");
                    fieldList.Add("EstimatedCompletionDate", "预计竣工日期");
                    fieldList.Add("LandUse", "土地用途");
                    fieldList.Add("LandOwnershipProperties", "土地所有制性质");
                    fieldList.Add("LandUseRightType", "土地使用权类型");
                    fieldList.Add("LandUseCertificateNo", "土地使用权证书号");
                    fieldList.Add("LandUseStartDate", "土地使用起始日期");
                    fieldList.Add("LandUseEndDate", "土地使用终止日期");
                    fieldList.Add("LandUseRightCompany", "土地使用权单位");
                    fieldList.Add("LandUseYears", "土地使用年限（年）");
                    fieldList.Add("PurchasePrice", "购置价（元）");
                    fieldList.Add("BuyingTime", "购买时间");
                    fieldList.Add("Remark", "备注");
                    fieldList.Add("ResidentialDistrict", "所处小区");
                    fieldList.Add("AccessoryArea", "附属面积（平方米）");
                    fieldList.Add("PurchaseContractNo", "购房合同号");
                    fieldList.Add("PropertyRightCertificateUseYears", "产权证使用年限(*)");
                    break;
                case "工业厂房":
                    fieldList.Add("Country", "国家(*)");
                    fieldList.Add("Province", "省/直辖市/自治区(*)");
                    fieldList.Add("City", "城市/自治州(*)");
                    fieldList.Add("District", "区（县）(*)");
                    fieldList.Add("Address", "座落地址(*)");
                    fieldList.Add("Building", "所在楼座");
                    fieldList.Add("TotalFloor", "楼层总层数(*)");
                    fieldList.Add("Floor", "所在楼层(*)");
                    fieldList.Add("FloorHeight", "层高（米）");
                    fieldList.Add("StructureArea", "建筑面积（平方米）(*)");
                    fieldList.Add("AncestralLandArea", "宗地面积（平方米）");
                    fieldList.Add("ApportionmentArae", "分摊土地面积（平方米）");
                    fieldList.Add("BuildYear", "建成年代(*)");
                    fieldList.Add("BuildingStructure", "建筑结构(*)");
                    fieldList.Add("DurableYears", "耐用年限(*)");
                    fieldList.Add("MaintenanceCondition", "维护状况(*)");
                    fieldList.Add("DecorationDegree", "装修程度(*)");
                    fieldList.Add("BasicFacilities", "基础配套设施");
                    fieldList.Add("HaveElevator", "是否带电梯(*)");
                    fieldList.Add("Environment", "环境（质量、景观）");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("LeasePeriod", "租赁周期");
                    fieldList.Add("PeriodicUnit", "周期单位");
                    fieldList.Add("IdlePeriod", "闲置期（月）");
                    fieldList.Add("StreetCondition", "临街状况(*)");
                    fieldList.Add("TrafficCondition", "交通条件");
                    fieldList.Add("FlourishingDegree", "繁华度");
                    fieldList.Add("HaveLandCertificate", "有无土地证(*)");
                    fieldList.Add("LandUse", "土地用途");
                    fieldList.Add("LandOwnershipProperties", "土地所有制性质");
                    fieldList.Add("LandUseRightType", "土地使用权类型");
                    fieldList.Add("LandUseCertificateNo", "土地使用权证书号");
                    fieldList.Add("LandUseRightCompany", "土地使用权单位");
                    fieldList.Add("LandUseStartDate", "土地使用起始日期");
                    fieldList.Add("LandUseEndDate", "土地使用终止日期");
                    fieldList.Add("PurchasePrice", "购置价（元）");
                    fieldList.Add("BuyingTime", "购买时间");
                    fieldList.Add("Remark", "备注");
                    break;
                case "工业用地":
                    fieldList.Add("Country", "国家(*)");
                    fieldList.Add("Province", "省(*)");
                    fieldList.Add("City", "市(*)");
                    fieldList.Add("District", "区(县)(*)");
                    fieldList.Add("Address", "座落地址(*)");
                    fieldList.Add("LandArea", "土地面积（平方米）(*)");
                    fieldList.Add("LandExploitDegree", "土地开发程度(*)");
                    fieldList.Add("LandRightCertificateNo", "土地权证号(*)");
                    fieldList.Add("LandUseRightType", "土地使用权类型(*)");
                    fieldList.Add("LandWarrantPersonName", "土地权证人名称(*)");
                    fieldList.Add("LandUse", "土地用途(*)");
                    fieldList.Add("LandOwnershipProperties", "土地所有制性质(*)");
                    fieldList.Add("LandUseStartDate", "土地使用起始日期(*)");
                    fieldList.Add("LandUseEndDate", "土地使用终止日期(*)");
                    fieldList.Add("LandAcquisitionDate", "土地取得日期");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("IdlePeriod", "闲置期（月）");
                    fieldList.Add("UseYears", "使用年限(*)");
                    fieldList.Add("LocationRegion", "所处区域(*)");
                    fieldList.Add("HaveConstructionLandPlanningPermit", "是否取得建设用地规划许可证");
                    fieldList.Add("HaveAboveGroundBuilding", "有无地上建筑物(*)");
                    fieldList.Add("AboveGroundBuildingConditionExplain", "地上建筑物情况说明");
                    fieldList.Add("PeripheryEnvironmentCondition", "周边环境状况(*)");
                    fieldList.Add("LandPrice", "土地价款（元）");
                    fieldList.Add("TransferPaymentCondition", "出让金交付情况");
                    fieldList.Add("MakeUpTransferFee", "应补出让金金额（元）");
                    fieldList.Add("AncestralLandNo", "宗地号");
                    fieldList.Add("AncestralLandFourDirectionLine", "宗地四至");
                    fieldList.Add("FloorAreaRatio", "容积率(*)");
                    fieldList.Add("BuildingDensity", "建筑密度");
                    fieldList.Add("LandLevel", "土地级别");
                    fieldList.Add("LandUseMode", "土地取用方式");
                    fieldList.Add("GroundAttachmentName", "地上附着物名称");
                    fieldList.Add("GroundAttachmentAcreage", "地上附着物面积（平方米）");
                    fieldList.Add("LandRemainingUseYears", "土地剩余使用年限（年）");
                    fieldList.Add("Remark", "备注");
                    break;
                case "股权":
                    fieldList.Add("EquityType", "股权类型(*)");
                    fieldList.Add("CertificateNo", "权证号");
                    fieldList.Add("IssuingUnit", "发行单位(*)");
                    fieldList.Add("LatestNetAssetsPerShare", "最新每股净资产(元)");
                    fieldList.Add("Quantity", "数量（股）(*)");
                    fieldList.Add("IssuePrice", "发行价格(元)(*)");
                    fieldList.Add("PurchasePrice", "购入价格(元)(*)");
                    fieldList.Add("PurchaseDate", "购入日期(*)");
                    fieldList.Add("IsCompanyProfitable", "公司是否盈利");
                    fieldList.Add("Remark", "备注");
                    break;
                case "其他商业用房":
                    fieldList.Add("Country", "国家(*)");
                    fieldList.Add("Province", "省/直辖市/自治区(*)");
                    fieldList.Add("City", "城市/自治州(*)");
                    fieldList.Add("District", "区（县）(*)");
                    fieldList.Add("Address", "座落地址(*)");
                    fieldList.Add("Building", "所在楼座");
                    fieldList.Add("TotalFloor", "楼层总层数(*)");
                    fieldList.Add("Floor", "所在楼层(*)");
                    fieldList.Add("FloorHeight", "层高（米）");
                    fieldList.Add("StructureArea", "建筑面积（平方米）(*)");
                    fieldList.Add("AncestralLandArea", "宗地面积（平方米）");
                    fieldList.Add("ApportionmentArae", "分摊土地面积（平方米）");
                    fieldList.Add("Toward", "朝向(*)");
                    fieldList.Add("CurrentManagementCondition", "目前管理状况(*)");
                    fieldList.Add("MaintenanceCondition", "维护状况(*)");
                    fieldList.Add("BuildingStructure", "建筑结构(*)");
                    fieldList.Add("DurableYears", "耐用年限(*)");
                    fieldList.Add("DecorationDegree", "装修程度(*)");
                    fieldList.Add("PublicFacilities", "公共配套设施");
                    fieldList.Add("IsIndependentStore", "是否独立店面(*)");
                    fieldList.Add("FourDirectionRange", "四至范围(*)");
                    fieldList.Add("HaveInterlayer", "有无夹层(*)");
                    fieldList.Add("HaveElevator", "是否带电梯(*)");
                    fieldList.Add("Environment", "环境（质量、景观）");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("LeasePeriod", "租赁周期");
                    fieldList.Add("PeriodicUnit", "周期单位");
                    fieldList.Add("IdlePeriod", "闲置期（月）");
                    fieldList.Add("StreetCondition", "临街状况(*)");
                    fieldList.Add("StreetDepth", "临街深度");
                    fieldList.Add("StreetWidth", "临街宽度");
                    fieldList.Add("TrafficCondition", "交通条件");
                    fieldList.Add("FlourishingDegree", "繁华度");
                    fieldList.Add("BuildYear", "建成年代(*)");
                    fieldList.Add("HousePropertyRightPersonName", "房产权利人名称(*)");
                    fieldList.Add("LandUseRightType", "土地使用权类型(*)");
                    fieldList.Add("HaveLandCertificate", "有无土地证(*)");
                    fieldList.Add("LandUseCertificateNo", "土地使用权证书号");
                    fieldList.Add("LandOwnershipProperties", "土地所有制性质");
                    fieldList.Add("LandUseStartDate", "土地使用起始日期");
                    fieldList.Add("LandUseEndDate", "土地使用终止日期");
                    fieldList.Add("LandUseRightCompany", "土地使用权单位");
                    fieldList.Add("LandUseYears", "土地使用年限");
                    fieldList.Add("PurchasePrice", "购置价（元）");
                    fieldList.Add("BuyingTime", "购买时间");
                    fieldList.Add("Remark", "备注");
                    break;
                case "其他用地":
                    fieldList.Add("Country", "国家(*)");
                    fieldList.Add("Province", "省(*)");
                    fieldList.Add("City", "市(*)");
                    fieldList.Add("District", "区(县)(*)");
                    fieldList.Add("Address", "座落地址(*)");
                    fieldList.Add("LandArea", "土地面积（平方米）(*)");
                    fieldList.Add("LandExploitDegree", "土地开发程度(*)");
                    fieldList.Add("LandRightCertificateNo", "土地权证号(*)");
                    fieldList.Add("LandUseRightType", "土地使用权类型(*)");
                    fieldList.Add("LandWarrantPersonName", "土地权证人名称(*)");
                    fieldList.Add("LandUse", "土地用途(*)");
                    fieldList.Add("LandOwnershipProperties", "土地所有制性质(*)");
                    fieldList.Add("LandUseStartDate", "土地使用起始日期(*)");
                    fieldList.Add("LandUseEndDate", "土地使用终止日期(*)");
                    fieldList.Add("LandAcquisitionDate", "土地取得日期");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("IdlePeriod", "闲置期（月）");
                    fieldList.Add("UseYears", "使用年限(*)");
                    fieldList.Add("LocationRegion", "所处区域(*)");
                    fieldList.Add("HaveConstructionLandPlanningPermit", "是否取得建设用地规划许可证");
                    fieldList.Add("HaveAboveGroundBuilding", "有无地上建筑物(*)");
                    fieldList.Add("AboveGroundBuildingConditionExplain", "地上建筑物情况说明");
                    fieldList.Add("PeripheryEnvironmentCondition", "周边环境状况(*)");
                    fieldList.Add("LandPrice", "土地价款（元）");
                    fieldList.Add("TransferPaymentCondition", "出让金交付情况");
                    fieldList.Add("MakeUpTransferFee", "应补出让金金额（元）");
                    fieldList.Add("AncestralLandNo", "宗地号");
                    fieldList.Add("AncestralLandFourDirectionLine", "宗地四至");
                    fieldList.Add("FloorAreaRatio", "容积率(*)");
                    fieldList.Add("BuildingDensity", "建筑密度");
                    fieldList.Add("LandLevel", "土地级别");
                    fieldList.Add("LandUseMode", "土地取用方式");
                    fieldList.Add("GroundAttachmentName", "地上附着物名称");
                    fieldList.Add("GroundAttachmentAcreage", "地上附着物面积（平方米）");
                    fieldList.Add("LandRemainingUseYears", "土地剩余使用年限（年）");
                    fieldList.Add("Remark", "备注");
                    break;
                case "商品房":
                    fieldList.Add("Country", "国家(*)");
                    fieldList.Add("Province", "省/直辖市/自治区(*)");
                    fieldList.Add("City", "城市/自治州(*)");
                    fieldList.Add("District", "区（县）(*)");
                    fieldList.Add("ResidentialDistrict", "所处小区(*)");
                    fieldList.Add("Address", "座落地址(*)");
                    fieldList.Add("CurrentManagementCondition", "管理状态(*)");
                    fieldList.Add("MaintenanceCondition", "维护状态(*)");
                    fieldList.Add("Building", "所在楼座");
                    fieldList.Add("Floor", "所在楼层(*)");
                    fieldList.Add("TotalFloor", "楼层总层数(*)");
                    fieldList.Add("RoomNo", "具体房号(*)");
                    fieldList.Add("FloorHeight", "层高（米）");
                    fieldList.Add("StructureArea", "建筑面积（平方米）(*)");
                    fieldList.Add("Toward", "朝向(*)");
                    fieldList.Add("HouseType", "户型");
                    fieldList.Add("BuildingStructure", "建筑结构(*)");
                    fieldList.Add("DurableYears", "耐用年限(*)");
                    fieldList.Add("BuildingType", "建筑形式");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("LeasePeriod", "租赁周期");
                    fieldList.Add("PeriodicUnit", "周期单位");
                    fieldList.Add("IdlePeriod", "闲置期（月）");
                    fieldList.Add("DecorationDegree", "装修程度(*)");
                    fieldList.Add("PlaneLayoutAdvantage", "平面布置优劣度(*)");
                    fieldList.Add("Facilities", "配套设施(*)");
                    fieldList.Add("PublicFacilities", "公共配套设施");
                    fieldList.Add("HavePrivateGarage", "有无私家车库");
                    fieldList.Add("HaveElevator", "是否带电梯(*)");
                    fieldList.Add("IsUrbanArea", "是否位于城市地带(*)");
                    fieldList.Add("PeripheryEnvironmentCondition", "周边环境状况");
                    fieldList.Add("Environment", "环境（质量、景观）");
                    fieldList.Add("StreetCondition", "临街状况(*)");
                    fieldList.Add("StreetDepth", "临街深度");
                    fieldList.Add("StreetWidth", "临街宽度");
                    fieldList.Add("TrafficCondition", "交通条件");
                    fieldList.Add("FlourishingDegree", "繁华度");
                    fieldList.Add("BuildYear", "建成年代(*)");
                    fieldList.Add("UseYears", "使用年限");
                    fieldList.Add("User", "使用者");
                    fieldList.Add("OwnershipSituation", "权属情况");
                    fieldList.Add("LandUse", "土地用途");
                    fieldList.Add("LandOwnershipProperties", "土地所有制性质");
                    fieldList.Add("LandUseRightNature", "土地使用权性质");
                    fieldList.Add("LandUseRightType", "土地使用权类型");
                    fieldList.Add("LandUseCertificateNo", "土地使用权证书号");
                    fieldList.Add("LandUseStartDate", "土地使用起始日期");
                    fieldList.Add("LandUseEndDate", "土地使用终止日期");
                    fieldList.Add("LandAcquisitionMode", "土地取得方式");
                    fieldList.Add("LandAcquisitionDate", "土地取得日期");
                    fieldList.Add("HaveLandCertificate", "有无土地证(*)");
                    fieldList.Add("LandWarrantType", "土地权证类型");
                    fieldList.Add("LandCertificateNo", "土地证号");
                    fieldList.Add("LandUseRightCompany", "土地使用权单位");
                    fieldList.Add("IssueDate", "核发日期");
                    fieldList.Add("PurchasePrice", "购置价（元）");
                    fieldList.Add("BuyingTime", "购买时间");
                    fieldList.Add("ChargeEvaluationUnitPrice", "押品评估单价（元）");
                    fieldList.Add("FirstOrSecondHand", "一手/二手");
                    fieldList.Add("Remark", "备注");
                    break;
                case "商铺":
                    fieldList.Add("Country", "国家(*)");
                    fieldList.Add("Province", "省/直辖市/自治区(*)");
                    fieldList.Add("City", "城市/自治州(*)");
                    fieldList.Add("District", "区（县）(*)");
                    fieldList.Add("Address", "座落地址(*)");
                    fieldList.Add("Building", "所在楼座");
                    fieldList.Add("TotalFloor", "楼层总层数(*)");
                    fieldList.Add("Floor", "所在楼层(*)");
                    fieldList.Add("FloorHeight", "层高（米）");
                    fieldList.Add("StructureArea", "建筑面积（平方米）(*)");
                    fieldList.Add("AncestralLandArea", "宗地面积（平方米）");
                    fieldList.Add("ApportionmentArae", "分摊土地面积（平方米）");
                    fieldList.Add("Toward", "朝向(*)");
                    fieldList.Add("CurrentManagementCondition", "目前管理状况(*)");
                    fieldList.Add("MaintenanceCondition", "维护状况(*)");
                    fieldList.Add("BuildingStructure", "建筑结构(*)");
                    fieldList.Add("DurableYears", "耐用年限(*)");
                    fieldList.Add("DecorationDegree", "装修程度(*)");
                    fieldList.Add("PublicFacilities", "公共配套设施");
                    fieldList.Add("IsIndependentStore", "是否独立店面(*)");
                    fieldList.Add("FourDirectionRange", "四至范围(*)");
                    fieldList.Add("HaveInterlayer", "有无夹层(*)");
                    fieldList.Add("HaveElevator", "是否带电梯(*)");
                    fieldList.Add("Environment", "环境（质量、景观）");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("LeasePeriod", "租赁周期");
                    fieldList.Add("PeriodicUnit", "周期单位");
                    fieldList.Add("IdlePeriod", "闲置期（月）");
                    fieldList.Add("StreetCondition", "临街状况(*)");
                    fieldList.Add("StreetDepth", "临街深度");
                    fieldList.Add("StreetWidth", "临街宽度");
                    fieldList.Add("TrafficCondition", "交通条件");
                    fieldList.Add("FlourishingDegree", "繁华度");
                    fieldList.Add("BuildYear", "建成年代(*)");
                    fieldList.Add("HousePropertyRightPersonName", "房产权利人名称(*)");
                    fieldList.Add("LandUseRightType", "土地使用权类型(*)");
                    fieldList.Add("HaveLandCertificate", "有无土地证(*)");
                    fieldList.Add("LandUseCertificateNo", "土地使用权证书号");
                    fieldList.Add("LandOwnershipProperties", "土地所有制性质");
                    fieldList.Add("LandUseStartDate", "土地使用起始日期");
                    fieldList.Add("LandUseEndDate", "土地使用终止日期");
                    fieldList.Add("LandUseRightCompany", "土地使用权单位");
                    fieldList.Add("LandUseYears", "土地使用年限");
                    fieldList.Add("PurchasePrice", "购置价（元）");
                    fieldList.Add("BuyingTime", "购买时间");
                    fieldList.Add("Remark", "备注");
                    break;
                case "商业用地":
                    fieldList.Add("Country", "国家(*)");
                    fieldList.Add("Province", "省(*)");
                    fieldList.Add("City", "市(*)");
                    fieldList.Add("District", "区(县)(*)");
                    fieldList.Add("Address", "座落地址(*)");
                    fieldList.Add("LandArea", "土地面积（平方米）(*)");
                    fieldList.Add("LandExploitDegree", "土地开发程度(*)");
                    fieldList.Add("LandRightCertificateNo", "土地权证号(*)");
                    fieldList.Add("LandUseRightType", "土地使用权类型(*)");
                    fieldList.Add("LandWarrantPersonName", "土地权证人名称(*)");
                    fieldList.Add("LandUse", "土地用途(*)");
                    fieldList.Add("LandOwnershipProperties", "土地所有制性质(*)");
                    fieldList.Add("LandUseStartDate", "土地使用起始日期(*)");
                    fieldList.Add("LandUseEndDate", "土地使用终止日期(*)");
                    fieldList.Add("LandAcquisitionDate", "土地取得日期");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("IdlePeriod", "闲置期（月）");
                    fieldList.Add("UseYears", "使用年限(*)");
                    fieldList.Add("LocationRegion", "所处区域(*)");
                    fieldList.Add("HaveConstructionLandPlanningPermit", "是否取得建设用地规划许可证");
                    fieldList.Add("HaveAboveGroundBuilding", "有无地上建筑物(*)");
                    fieldList.Add("AboveGroundBuildingConditionExplain", "地上建筑物情况说明");
                    fieldList.Add("PeripheryEnvironmentCondition", "周边环境状况(*)");
                    fieldList.Add("LandPrice", "土地价款（元）");
                    fieldList.Add("TransferPaymentCondition", "出让金交付情况");
                    fieldList.Add("MakeUpTransferFee", "应补出让金金额（元）");
                    fieldList.Add("AncestralLandNo", "宗地号");
                    fieldList.Add("AncestralLandFourDirectionLine", "宗地四至");
                    fieldList.Add("FloorAreaRatio", "容积率(*)");
                    fieldList.Add("BuildingDensity", "建筑密度");
                    fieldList.Add("LandLevel", "土地级别");
                    fieldList.Add("LandUseMode", "土地取用方式");
                    fieldList.Add("GroundAttachmentName", "地上附着物名称");
                    fieldList.Add("GroundAttachmentAcreage", "地上附着物面积（平方米）");
                    fieldList.Add("LandRemainingUseYears", "土地剩余使用年限（年）");
                    fieldList.Add("Remark", "备注");
                    break;
                case "设备":
                    fieldList.Add("EquipmentName", "设备名称");
                    fieldList.Add("EquipmentNo", "设备编号");
                    fieldList.Add("MachineEquipmentType", "机器设备类型");
                    fieldList.Add("SpecificUse", "具体用途");
                    fieldList.Add("EquipmentCondition", "设备状况（是否正常生产）(*)");
                    fieldList.Add("Brand", "品牌(*)");
                    fieldList.Add("SpecificationModel", "规格型号(*)");
                    fieldList.Add("Manufacturer", "制造厂家");
                    fieldList.Add("ProductPlace", "产地(*)");
                    fieldList.Add("Quantity", "数量");
                    fieldList.Add("MeasurementUnit", "计量单位");
                    fieldList.Add("HaveQualificationCertificate", "有无产品合格证");
                    fieldList.Add("HaveSafetyCertificate", "有无安全检查证明");
                    fieldList.Add("HaveFireControlCertificate", "有无消防检查证明");
                    fieldList.Add("InvoiceNo", "发票号码");
                    fieldList.Add("User", "使用人");
                    fieldList.Add("Owner", "所有人");
                    fieldList.Add("FirstOrSecondHand", "一手/二手(*)");
                    fieldList.Add("UseYears", "已使用年限");
                    fieldList.Add("Place", "处所");
                    fieldList.Add("UseState", "使用状态(*)");
                    fieldList.Add("EquipmentServiceLife", "设备使用寿命");
                    fieldList.Add("DepreciationCondition", "新旧情况");
                    fieldList.Add("OverhaulTimes", "大修次数");
                    fieldList.Add("PowerFuel", "动力燃料");
                    fieldList.Add("LeaseCondition", "租赁情况");
                    fieldList.Add("LeaseTerm", "租赁年限");
                    fieldList.Add("AnnualRent", "年租金");
                    fieldList.Add("Power", "功率");
                    fieldList.Add("PowerUnit", "功率单位");
                    fieldList.Add("IsImportedEquipment", "是否进口设备(*)");
                    fieldList.Add("IsCustomsControl", "是否为海关监管物(*)");
                    fieldList.Add("PurchaseCurrency", "购置币种(*)");
                    fieldList.Add("PurchasePrice", "购置价(*)");
                    fieldList.Add("ProductionDate", "出厂日期(*)");
                    fieldList.Add("PurchaseDate", "购入日期");
                    fieldList.Add("RegulatoryExpirationDate", "监管到期日");
                    fieldList.Add("Remark", "备注");
                    break;

                #endregion
            }
            var book = new HSSFWorkbook();
            var sheet = book.CreateSheet("sheet1");
            var index = 0;
            foreach (var field in fieldList)
            {
                System.Reflection.PropertyInfo propertyInfo = project.SummaryData.GetType().GetProperty(field.Key);
                if (propertyInfo != null)
                {
                    var row = sheet.CreateRow(index);
                    ICell cell = null;
                    cell = row.CreateCell(0);
                    cell.SetCellValue(field.Value);
                    row.Cells.Add(cell);
                    cell = row.CreateCell(1);
                    cell.SetCellValue(propertyInfo.GetValue(project.SummaryData, null) == null
                        ? ""
                        : propertyInfo.GetValue(project.SummaryData, null).ToString());
                    row.Cells.Add(cell);
                }
                index++;
            }
            var stream = new MemoryStream();
            book.Write(stream);
            return stream.ToArray();
        }

        /// <summary>
        /// 上传报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="fileName"></param>
        /// <param name="fileByte"></param>
        /// <param name="userId"></param>
        /// <param name="reportType"></param>
        public string UploadProjectResource(long projectId, string fileName, byte[] fileByte, long userId, ResourcesEnum reportType = ResourcesEnum.正式报告)
        {
            if (projectId == 0)
            {
                return "上传失败!";
            }
            var project = ProjectRepository.Instance.Source.FirstOrDefault(x => x.Id == projectId);
            if (project == null)
            {
                return "上传失败!";
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                return "您无权操作此项目!";
            }
            //var extensions = new string[] { "doc", "docx", "xls", "xlsx", "pdf" };
            var extensions = new string[] { "doc", "docx" };
            if (!extensions.Contains(fileName.Substring(fileName.LastIndexOf('.') + 1).ToLower()))
            {
                //return "请上传word、excel或pdf文件!";
                return "请上传word文件!";
            }
            var newName = project.ProjectNo + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            //文件后缀
            if (fileName.Contains("."))
            {
                newName += "." + fileName.Substring(fileName.LastIndexOf('.') + 1);
            }
            else
            {
                newName += ".doc";
            }
            var path = FileStreamHelper.GetUploadFilePath();
            string fullPath = FileStreamHelper.UrlConvertorLocal(path);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            fullPath += newName;
            fileByte.SaveAs(fullPath);
            var url = ("/" + path + newName).Replace(@"\", @"/");
            var resource = new ProjectResource()
            {
                FileName = fileName,
                FileFormat = fileName.Substring(fileName.LastIndexOf('.') + 1),
                FilePath = url,
                ResourcesType = reportType,
                ProjectId = projectId,
                CreateTime = DateTime.Now
            };
            ProjectResourceRepository.Instance.Insert(resource);

            //流程记录
            var projectState = new ProjectStateInfo();
            projectState.ProjectId = projectId;
            if (reportType == ResourcesEnum.预估报告)
            {
                projectState.Content = "上传预估报告《" + fileName + "》";
            }
            else
            {
                projectState.Content = "上传正式报告《" + fileName + "》";
            }
            projectState.OperationTime = DateTime.Now;
            projectState.Operator = string.Format("{0}", user.UserName);
            ProjectStateInfoRepository.Instance.Insert(projectState);
            return "上传成功!";
        }

        /// <summary>
        /// 获取最新的上传报告
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="reportType"></param>
        public ProjectResource GetLastUploadReportResource(long projectId, ResourcesEnum reportType = ResourcesEnum.正式报告)
        {
            return
                ProjectResourceRepository.Instance.Source.Where(
                    x => x.ProjectId == projectId && x.ResourcesType == reportType)
                    .OrderByDescending(y => y.Id)
                    .FirstOrDefault();
        }

        /// <summary>
        /// 获取项目资源
        /// </summary>
        /// <param name="resourceId"></param>
        public ProjectResource GetResourceById(long resourceId)
        {
            return
                ProjectResourceRepository.Instance.Source.FirstOrDefault(
                    x => x.Id == resourceId);
        }

        /// <summary>
        /// 下载线上资料
        /// </summary>
        /// <param name="zipStream"></param>
        /// <param name="project"></param>
        private void DownWebResource(ZipOutputStream zipStream, Project project)
        { 
            foreach (var item in project.ProjectResources.Where(x=>x.ResourcesType==ResourcesEnum.附件))
            {
                var sourceUrl = FileStreamHelper.GetPathAtApplication(item.FilePath);
                if (string.IsNullOrEmpty(sourceUrl))
                    continue;
                string fileName = string.Format("{0}_{1}", "线上资料", item.FileName);
                byte[] data;
                string extension = !string.IsNullOrEmpty(item.FileFormat) ? item.FileFormat : Path.GetExtension(fileName);
                if (!fileName.Contains("." + extension))
                {
                    fileName += "." + extension;
                }
                try
                {
                    var fs = new FileStream(sourceUrl, FileMode.Open, FileAccess.Read);
                    data = new byte[(int)fs.Length];
                    fs.Read(data, 0, data.Length);
                    fs.Close();
                }
                catch
                {
                    fileName = "（下载失败）" + fileName;
                    data = new byte[0];
                }
                ZipEntry entry = new ZipEntry(fileName);
                entry.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry);
                zipStream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// 保存扫描记录
        /// </summary>
        /// <param name="idList"></param>
        public void SaveScanResult(IList<long> idList)
        {
            var result = ProjectRepository.Instance.Source.Where(x => idList.Contains(x.Id)).ToList();
            foreach (var project in result)
            {
                project.IsScan = true;
                project.ScanTime = DateTime.Now;
                ProjectRepository.Instance.Save(project);
            }
        }

        /// <summary>
        /// 根据流水号获取项目
        /// </summary>
        /// <param name="projectNo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Project GetByProjectNo(string projectNo, long userId)
        {
            var project = ProjectRepository.Instance.Find(x => x.ProjectNo == projectNo).FirstOrDefault();
            if (project == null)
            {
                throw new ServiceException("项目不存在!");
            }
            var user = UserRepository.Instance.Source.FirstOrDefault(x => x.Id == userId);
            if (user == null || !(user.CompanyId == project.CompanyId || user.IsAdmin))
            {
                throw new ServiceException("您无权操作此项目!");
            }
            return project;
        }
    }
}
