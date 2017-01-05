using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Peacock.Common.Helper;
using Peacock.ZXEval.Model.Condition;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.WebSite.Help;

namespace Peacock.ZXEval.WebSite.Controllers
{
    public class ProjectController : BaseController
    {
        /// <summary>
        /// 评估任务
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var projectStatus = new Dictionary<int, string>();
            foreach (var status in (ProjectStatusEnum[]) Enum.GetValues(typeof (ProjectStatusEnum)))
            {
                projectStatus.Add((int) status, status.ToString());
            }
            ViewBag.Status = projectStatus;
            return View();
        }

        /// <summary>
        /// 获取评估列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetProjectList(ProjectCondition condition, int index, int rows)
        {
            int total;
            var result = ProjectService.GetProjectList(condition, index, rows, out total, UserHelper.GetCurrentUser().Id);
            return Json(new
            {
                rows = result.Select(x => new
                {
                    x.Id,
                    x.ProjectNo,
                    x.PledgeAddress,
                    x.EvalType,
                    x.PropertyType,
                    ProjectStatus = x.ProjectStatus.ToString(),
                    CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", x.CreateTime),
                    IsFirstCredit = x.IsFirstCredit ? "是" : "否",
                    CanDownLoadSummaryData = x.SummaryData.CreateTime.HasValue,
                    x.RevokeReason
                }),
                total
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 项目受理
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AcceptProject(long id, string note)
        {
            return ExceptionCatch.Invoke(() =>
            {
                ProjectService.AcceptProject(id, UserHelper.GetCurrentUser().Id, note);
            });
        }

        /// <summary>
        /// 外业勘察
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OperateOuterTask(long projectId, OuterTaskModel model, string note)
        {
            return ExceptionCatch.Invoke(() =>
            {
                ProjectService.OperateOuterTask(projectId, model, UserHelper.GetCurrentUser().Id, note);
            });
        }

        /// <summary>
        /// 报告预估
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OperateReportEstimate(long projectId, SummaryDataModel model, string note)
        {
            string data = JsonConvert.SerializeObject(model);
            return ExceptionCatch.Invoke(() =>
            {
                ProjectService.OperateReportEstimate(projectId, model, UserHelper.GetCurrentUser().Id, note);
            });
        }

        /// <summary>
        /// 报告准备
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OperateReportPrepare(long id, string note)
        {
            return ExceptionCatch.Invoke(() =>
            {
                ProjectService.OperateReportPrepare(id, UserHelper.GetCurrentUser().Id, note);
            });
        }

        ///// <summary>
        ///// 报告审核
        ///// </summary>
        ///// <returns></returns>
        //[HttpPost]
        //public JsonResult OperateReportAudit(long id)
        //{
        //    return ExceptionCatch.Invoke(() =>
        //    {
        //        ProjectService.OperateReportAudit(id, UserHelper.GetCurrentUser().Id);
        //    });
        //}

        /// <summary>
        /// 发送报告
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult OperateReportSend(long projectId, SummaryDataModel model, string note)
        {
            return ExceptionCatch.Invoke(() =>
            {
                ProjectService.OperateReportSend(projectId, model, UserHelper.GetCurrentUser().Id, note);
            });
        }

        /// <summary>
        /// 汇总数据
        /// </summary>
        /// <returns></returns>
        public ActionResult SummaryData(long projectId, string operateType)
        {
            var project = ProjectService.GetProjectById(projectId, UserHelper.GetCurrentUser().Id);
            var fieldList = new Dictionary<string, string>();
            var haveRegion = true;
            switch (project.PropertyType)
            {
                    #region 根据项目类型添加字段

                case "办公":
                    //fieldList.Add("Country", "国家(*)");
                    //fieldList.Add("Province", "省/直辖市/自治区(*)");
                    //fieldList.Add("City", "城市/自治州(*)");
                    //fieldList.Add("District", "区（县）(*)");
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
                    //fieldList.Add("Country", "国家(*)");
                    //fieldList.Add("Province", "省/直辖市/自治区(*)");
                    //fieldList.Add("City", "城市/自治州(*)");
                    //fieldList.Add("District", "区（县）(*)");
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
                    //fieldList.Add("Country", "国家(*)");
                    //fieldList.Add("Province", "省(*)");
                    //fieldList.Add("City", "市(*)");
                    //fieldList.Add("District", "区(县)(*)");
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
                    haveRegion = false;
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
                    //fieldList.Add("Country", "国家(*)");
                    //fieldList.Add("Province", "省/直辖市/自治区(*)");
                    //fieldList.Add("City", "城市/自治州(*)");
                    //fieldList.Add("District", "区（县）(*)");
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
                    //fieldList.Add("Country", "国家(*)");
                    //fieldList.Add("Province", "省(*)");
                    //fieldList.Add("City", "市(*)");
                    //fieldList.Add("District", "区(县)(*)");
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
                    //fieldList.Add("Country", "国家(*)");
                    //fieldList.Add("Province", "省/直辖市/自治区(*)");
                    //fieldList.Add("City", "城市/自治州(*)");
                    //fieldList.Add("District", "区（县）(*)");
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
                    //fieldList.Add("Country", "国家(*)");
                    //fieldList.Add("Province", "省/直辖市/自治区(*)");
                    //fieldList.Add("City", "城市/自治州(*)");
                    //fieldList.Add("District", "区（县）(*)");
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
                    //fieldList.Add("Country", "国家(*)");
                    //fieldList.Add("Province", "省(*)");
                    //fieldList.Add("City", "市(*)");
                    //fieldList.Add("District", "区(县)(*)");
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
                    haveRegion = false;
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
            ViewBag.HaveRegion = haveRegion;
            ViewBag.FieldList = fieldList;
            string haveUpload;
            if (operateType == "报告预估")
            {
                haveUpload = ProjectService.GetLastUploadReportResource(projectId, ResourcesEnum.预估报告) != null
                    ? "true"
                    : "false";
            }
            else
            {
                haveUpload = ProjectService.GetLastUploadReportResource(projectId) != null
                    ? "true"
                    : "false";
            }
            ViewBag.HaveUpload = haveUpload;
            ViewBag.OperateType = operateType;
            return View(project.SummaryData);
        }

        /// <summary>
        /// 项目信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Detail(long id)
        {
            var project = ProjectService.GetProjectById(id, UserHelper.GetCurrentUser().Id);
            return View(project);
        }

        /// <summary>
        /// 下载附件 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        public ActionResult DownLoadFile(long projectId, string projectNo)
        {
            byte[] source = ProjectService.DownLoadFiles(projectId, UserHelper.GetCurrentUser().Id);
            return File(source, "application/octet-stream",
                string.Format("下载附件{0}_{1:yyyyMMddHHmmss}.zip", projectNo, DateTime.Now));
        }

        /// <summary>
        /// 导出汇总数据 
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="projectNo"></param>
        /// <returns></returns>
        public ActionResult DownLoadSummaryData(long projectId, string projectNo)
        {
            byte[] source = ProjectService.DownLoadSummaryData(projectId, UserHelper.GetCurrentUser().Id);
            return File(source, "application/octet-stream",
                string.Format("导出{0}汇总数据_{1:yyyyMMddHHmmss}.xls", projectNo, DateTime.Now));
        }

        /// <summary>
        /// 下载项目资源 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="projectId"></param>
        /// <param name="operateType"></param>
        /// <returns></returns>
        public ActionResult DownLoadResource(long id, long projectId, string operateType)
        {
            var resource = ProjectService.GetResourceById(id);
            if (resource == null)
            {
                return
                    Content(
                        "<script >alert('对不起，该文件已被删除，无法下载！');self.location='/Project/SummaryData?projectId=" + projectId +
                        "&operateType=" + operateType + "'</script >", "text/html");
            }
            var sourceUrl = FileStreamHelper.GetPathAtApplication(resource.FilePath);
            byte[] data;
            try
            {
                var fs = new FileStream(sourceUrl, FileMode.Open, FileAccess.Read);
                data = new byte[(int)fs.Length];
                fs.Read(data, 0, data.Length);
                fs.Close();
            }
            catch
            {
                return
                    Content(
                        "<script >alert('对不起，该文件已被删除，无法下载！');self.location='/Project/SummaryData?projectId=" + projectId +
                        "&operateType=" + operateType + "'</script >", "text/html");
            }
            return File(data, "application/octet-stream", resource.FileName);
        }

        /// <summary>
        /// 上传报告
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadReport()
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = null;
            //是否有文件
            if (files.Count > 0)
            {
                file = files[0];
            }
            else
            {
                return Json(new {msg = "请上传文件！"}, "text/html", JsonRequestBehavior.AllowGet);
            }
            long projectId;
            long.TryParse(Request["projectId"] + "", out projectId);

            Stream fileStream = file.InputStream;
            string fileName = file.FileName;
            byte[] fileByte = new byte[file.ContentLength];
            fileStream.Read(fileByte, 0, file.ContentLength - 1);
            string result = ProjectService.UploadProjectResource(projectId, fileName, fileByte,
                UserHelper.GetCurrentUser().Id);
            //将返回设置为"text/html",解决ajaxSubmit 在IE8下不执行success，而是作为附件下载
            return Json(new {msg = result}, "text/html", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 上传预估报告
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadEstimateReport()
        {
            HttpFileCollectionBase files = Request.Files;
            HttpPostedFileBase file = null;
            //是否有文件
            if (files.Count > 0)
            {
                file = files[0];
            }
            else
            {
                return Json(new {msg = "请上传文件！"}, "text/html", JsonRequestBehavior.AllowGet);
            }
            long projectId;
            long.TryParse(Request["projectId"] + "", out projectId);

            Stream fileStream = file.InputStream;
            string fileName = file.FileName;
            byte[] fileByte = new byte[file.ContentLength];
            fileStream.Read(fileByte, 0, file.ContentLength - 1);
            string result = ProjectService.UploadProjectResource(projectId, fileName, fileByte,
                UserHelper.GetCurrentUser().Id, ResourcesEnum.预估报告);
            //将返回设置为"text/html",解决ajaxSubmit 在IE8下不执行success，而是作为附件下载
            return Json(new {msg = result}, "text/html", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 项目信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetReportList(long id)
        {
            var project = ProjectService.GetProjectById(id, UserHelper.GetCurrentUser().Id);
            return
                Json(
                    project.ProjectResources.Where(
                        y => y.ResourcesType == ResourcesEnum.预估报告 || y.ResourcesType == ResourcesEnum.正式报告)
                        .OrderBy(z => z.Id)
                        .Select(x => new
                        {
                            x.Id,
                            x.FileName,
                            x.FilePath,
                            ResourcesType = x.ResourcesType.ToString(),
                            CreateTime = string.Format("{0:yyyy-MM-dd HH:mm:ss}", x.CreateTime),
                            IsLast =
                                (x ==
                                 project.ProjectResources.Where(p => p.ResourcesType == x.ResourcesType)
                                     .OrderByDescending(y => y.Id)
                                     .FirstOrDefault())
                                    ? "是"
                                    : "否"
                        }));
        }
    }
}
