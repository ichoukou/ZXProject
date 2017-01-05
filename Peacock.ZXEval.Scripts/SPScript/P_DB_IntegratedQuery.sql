DROP PROCEDURE IF EXISTS P_DB_IntegratedQuery;
CREATE PROCEDURE P_DB_IntegratedQuery (
  IN exceType VARCHAR(50),
	IN iIndex INT,
  IN iSize   INT,
  IN columnSel VARCHAR(2000),
  IN currentUser INT,
	IN departIds   VARCHAR(500),
	IN deptId INT,
	IN projectNo VARCHAR(50),
	IN reportNo VARCHAR(50),
	IN address VARCHAR(100),
	IN areaName VARCHAR(100),
	IN bank VARCHAR(50),
	IN subBank VARCHAR(100),
	IN customerName VARCHAR(50),
	IN customerTel VARCHAR(20),
	IN customerPhone VARCHAR(20),
	IN contactName VARCHAR(50),
	IN contactPhone VARCHAR(20),
	IN propertyType VARCHAR(50),
	IN reportType VARCHAR(50),
	IN remark VARCHAR(100),
	IN createTimeBegin VARCHAR(50),
	IN createTimeEnd VARCHAR(50),
	IN IsCancle SMALLINT,        
  INOUT itotal BIGINT
)
BEGIN
	SET @strSql ='';
	SET @strWhere ='';
	SET @strFrom ='';
SET @strWhere =  ' where 1 = 1 ';
if currentUser!=0 OR departIds !='' THEN
	SET @strWhere = CONCAT(@strWhere,' and( 1 != 1 ');
	 if currentUser!=0  THEN
		SET @strWhere =concat(@strWhere,' OR p.ProjectCreatorId = ',currentUser);
	 END IF;
	 if departIds !='' THEN
		SET @strWhere =concat(@strWhere,' OR p.DepartmentId in (',departIds,')');
		END IF;
	SET @strWhere=concat(@strWhere,' ) ');
	END IF;
if deptId !=0 THEN
	SET @strWhere =concat(@strWhere,' AND p.DepartmentId = ',deptId);
END IF;
IF projectNo !='' THEN
	SET @strWhere =concat(@strWhere,' AND p.ProjectNo  like "%',projectNo,'%"');
END IF;
if propertyType !='' THEN
	SET @strWhere =concat(@strWhere,' AND p.PropertyType  like "%',propertyType,'%"');
END IF;
IF reportNo !='' THEN
	SET @strWhere =concat(@strWhere,' AND p.ReportNo like "%',reportNo,'%"');
END IF;
if reportType !='' THEN
	SET @strWhere =concat(@strWhere,' AND p.ReportType  like "%',reportType,'%"');
END IF;
if address !='' THEN
	SET @strWhere = concat(@strWhere,' AND p.ProjectAddress like "%',address,'%"');
END IF;
if areaName !='' THEN
	SET @strWhere = concat(@strWhere,' AND p.ResidentialAreaName like "%',areaName,'%"');
END IF;
if bank !=''  THEN
	SET @strWhere = concat(@strWhere,' AND cust.bank like "%',bank,'%"');
END IF;
if subBank !='' THEN
	SET @strWhere = concat(@strWhere,' AND cust.Subbranch like "%',subBank,'%"');
END IF;
if customerName !='' THEN
	SET @strWhere = concat(@strWhere,' AND cust.CustomerName like "%',customerName,'%"');
END IF;
if customerTel !='' THEN
	SET @strWhere = concat(@strWhere,' AND cust.Tel like "%',customerTel,'%"');
END IF;
if customerPhone !='' THEN
	SET @strWhere = concat(@strWhere,' AND cust.Phone like "%',customerPhone,'%"');
END IF;
if contactName !='' THEN
	SET @strWhere = concat(@strWhere,' AND lec.ContactsName like "%',contactName,'%"');
END IF;
if contactPhone !='' THEN
	SET @strWhere = concat(@strWhere,' AND lec.ContactsPhone like "%',contactPhone,'%"');
END IF;
IF remark !='' THEN
	SET @strWhere =concat(@strWhere,' AND p.remark like "%',remark,'%"');
END IF;
IF createTimeBegin!='' THEN
        SET @strWhere=CONCAT(@strWhere,' AND p.CreateTime >= "',createTimeBegin,'"');
END IF;
IF createTimeEnd!='' THEN
        SET @strWhere=CONCAT(@strWhere,' AND p.CreateTime <= "',createTimeEnd,'"');
END IF;
IF isCancle =1 THEN
	SET @strWhere = concat(@strWhere,' AND p.IsCancle = ',isCancle,'');
END IF;
SET @strWhere = concat(@strWhere,' Order by p.Id desc ');
SET @strFrom =' SELECT p.Id as ProjectId,p.DepartmentId,p.ProjectNo,p.ReportNo,p.PropertyType,p.BusinessType,
p.ProjectSource,p.ProjectAddress,p.ResidentialAreaName,
p.ResidentialAddress,ROUND(p.BuildingArea,2) as BuildingArea,p.City,p.District,p.BuildedYear,ROUND(p.InquiryResult,2) as InquiryResult ,
ROUND(p.InquiryPrice,2) as  InquiryPrice,p.EmergencyLevel,p.Principal,p.CreditBank,p.CreditSubbranch,p.CreditInfo,
if(p.IsProspecting=1,"是","否") as IsProspecting ,p.Investigator,p.Remark ,if(p.IsSubmitted=1,"是","否") as IsSubmitted ,
IsApprove =case p.IsApprove when 0 then "未审核" when 1 then "已审核" when 2 then "审核中" when 3 then "已退回" else "未知" end,
if(p.IsApprove=1,"是","否") as IsApprove ,if(p.IsSent=1,"是","否") as IsSent,
date_format( p.SubmitTime,"%Y-%m-%d %H:%i:%s") as SubmitTime ,
date_format( p.ApproveTime,"%Y-%m-%d %H:%i:%s") as ApproveTime ,
if(p.IsUploadProject=1,"是","否") as IsUploadProject ,
date_format( p.CreateTime,"%Y-%m-%d %H:%i:%s") as CreateTime,if(p.Deleted=1,"是","否") as Deleted ,
p.ReportType,cust.CustomerName,cust.Tel AS CustomerTel ,cust.Phone AS CustomerPhone,
cust.Bank as CustomerBank,cust.Subbranch as CustomerSubbranch,
cust.QQ as CustomerQQ,lec.ContactsName ,lec.ContactsPhone ,
rep.SendReportCount,sum.Id as sumId,sum.Company as sumCompany ,sum.Department as  sumDepartment ,
sum.ProjectName as sumProjectName, sum.ReportNo as sumReportNo   ,
sum.ReportYear as sumReportYear ,sum.ReportMonth as sumReportMonth ,
sum.EvalGoal as sumEvalGoal ,sum.GoalDescription as sumGoalDescription, 
sum.EvalEntrust as sumEvalEntrust ,sum.ReportUse as sumReportUse ,sum.Subbranch as sumSubbranch 
,sum.SurveyPeople as sumSurveyPeople ,sum.ReportWriting as  sumReportWriting ,
sum.ProjectPopularizeName as sumProjectPopularizeName ,sum.ProjectProvince as sumProjectProvince ,
sum.ProjectCity as sumProjectCity ,sum.ProjectDistrict as sumProjectDistrict ,
sum.OtherAddress as sumOtherAddress ,sum.ProjectAddress as sumProjectAddress ,
 date_format( sum.SurveyTime,"%Y-%m-%d") as sumSurveyTime ,
	date_format( sum.WorthTime,"%Y-%m-%d")as sumWorthTime ,
	date_format( sum.JobStartTime,"%Y-%m-%d") as sumJobStartTime,
date_format( sum.JobEndTime,"%Y-%m-%d") as sumJobEndTime ,
sum.ReportValidityTerm  as sumReportValidityTerm,
sum.QuantitySurveyor1 as sumQuantitySurveyor1 , sum.QuantitySurveyor2 as sumQuantitySurveyor2 ,
sum.QuantitySurveyorJoin  as sumQuantitySurveyorJoin,
sum.EvalMethod1 as sumEvalMethod1 ,sum.EvalMethod2 as sumEvalMethod2 ,sum.EvalMethodJoin as sumEvalMethodJoin ,
sum.EvaluateTotal  as sumEvaluateTotal,sum.EvaluatePrice as sumEvaluatePrice ,
sum.HouserOwnerNo as sumHouserOwnerNo,sum.HouserOwner as sumHouserOwner,sum.PropertyNature as sumPropertyNature ,
sum.ArchitectureArea as sumArchitectureArea , sum.BuildingStructure as sumBuildingStructure ,
sum.BuiltYear as sumBuiltYear ,sum.RegisterUse  as sumRegisterUse, sum.Floor as sumFloor,
sum.MaxFloor as sumMaxFloor ,sum.FloorJoin as sumFloorJoin ,sum.HouseType as sumHouseType,
sum.Decoration as sumDecoration ,sum.LandUserNo as sumLandUserNo ,
sum.LandAddress as sumLandAddress ,sum.LandUsePeople as sumLandUsePeople ,sum.LandUseType as sumLandUseType ,
sum.LandUseArea as sumLandUseArea, sum.LandSpareYear as sumLandSpareYear,
sum.SpareEconomicsYear as sumSpareEconomicsYear ,sum.MaxUseYear as sumMaxUseYear,sum.LandUse as  sumLandUse,
date_format( sum.LandEndTime,"%Y-%m-%d") as sumLandEndTime ,
sum.OtherInfluenceFactor as sumOtherInfluenceFactor ,
date_format( sum.BusinessTime,"%Y-%m-%d")  as sumBusinessTime ,
sum.LoopLine as sumLoopLine ,sum.DealTotal  as sumDealTotal,
sum.DealPrice  as sumDealPrice,"" as CancleTime,"" as CancleReason ,p.ApproverName,p.ProjectCreatorName,
"" as DepartmentName
FROM project as p
			LEFT OUTER JOIN customer as cust ON p.CustomerId = cust.Id
			LEFT OUTER JOIN summarydata sum on p.Id = sum.ProjectId
			LEFT OUTER JOIN (SELECT rs.ProjectId,COUNT(1) as SendReportCount
													from ReportSend   	rs
											GROUP BY rs.ProjectId
											) as rep  on p.Id = rep.ProjectId 
			LEFT OUTER JOIN (
				SELECT  ec.ProjectId,group_concat(ec.Contacts) as ContactsName,
				group_concat(ec.Phone) as ContactsPhone FROM ExplorationContacts as ec 
					group by ec.ProjectId
					  ) as lec on p.Id = lec.ProjectId
			
			';
	if columnSel='' THEN 
		set columnSel = ' * ';
	END IF;
	IF exceType='导出' THEN 
		
		SET @strSql =CONCAT(' Select ', columnSel ,' from ( ' ,@strFrom,@strWhere, ' ) as t '  );
		prepare stmt from @strSql; 
		EXECUTE stmt;
	ELSE 
		SET @strSql =CONCAT(' Select SQL_CALC_FOUND_ROWS ', columnSel ,' from ( ' ,@strFrom, @strWhere,' ) as t limit ',(iIndex-1) * iSize,',',iSize  );
		prepare stmt from @strSql;  
		EXECUTE stmt;
		select FOUND_ROWS() into itotal;
	END IF;
	
END