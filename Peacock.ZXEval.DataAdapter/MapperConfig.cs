using System.Data.Entity.Core.Metadata.Edm;
﻿using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peacock.ZXEval.Data;
using System.Linq.Expressions;
using Peacock.ZXEval.Model.ApiModel;
using Peacock.ZXEval.Model.DTO;
using Peacock.ZXEval.Service;
using Peacock.ZXEval.Data.Entities;


namespace Peacock.ZXEval.DataAdapter
{
    /// <summary>
    /// Model和Entity之间的转换
    /// ConditionQuery和Condition之间的转换
    /// </summary>
    public static class MapperConfig
    {
        static MapperConfig()
        {
            Init();
        }

        public static void Init()
        {
            Mapper.CreateMap<User, UserModel>();
            Mapper.CreateMap<UserModel, User>();
            Mapper.CreateMap<CompanyModel, Company>();
            Mapper.CreateMap<Company, CompanyModel>();
            Mapper.CreateMap<CompanyBaseData, CompanyBaseDataModel>();
            Mapper.CreateMap<CompanyBaseDataModel, CompanyBaseData>();
            Mapper.CreateMap<Project, ProjectModel>();
            Mapper.CreateMap<ProjectModel, Project>();
            Mapper.CreateMap<SummaryData, SummaryDataModel>();
            Mapper.CreateMap<SummaryDataModel, SummaryData>();
            Mapper.CreateMap<OuterTask, OuterTaskModel>();
            Mapper.CreateMap<OuterTaskModel, OuterTask>();
            Mapper.CreateMap<ProjectStateInfo, ProjectStateInfoModel>();
            Mapper.CreateMap<ProjectStateInfoModel, ProjectStateInfo>();
            Mapper.CreateMap<ProjectRequest, Project>();
            Mapper.CreateMap<ProjectResourceRequest, ProjectResource>();
            Mapper.CreateMap<ProjectResourceModel, ProjectResource>();
            Mapper.CreateMap<ProjectResource, ProjectResourceModel>();
            Mapper.CreateMap<Parameter, ParameterModel>();
            Mapper.CreateMap<Revaluation, RevaluationModel>();
            Mapper.CreateMap<Revaluation, RevaluationDto>()
                .ForMember(x => x.canFinish,
                    x =>
                        x.MapFrom(
                            y =>
                                !y.RevaluationItems.Any(
                                    z => z.RevaluationValue.HasValue == false || z.RevaluationTime.HasValue == false)));
            Mapper.CreateMap<RevaluationModel, Revaluation>();
            Mapper.CreateMap<RevaluationItem, RevaluationItemModel>()
                .ForMember(x => x.EvalType, x => x.MapFrom(y => y.Revaluation.EvalType));
            Mapper.CreateMap<RevaluationItemModel, RevaluationItem>();
            Mapper.CreateMap<RevaluationItemRequest, RevaluationItem>()
                .ForMember(x=>x.CreditBalance,x=>x.MapFrom(y=>decimal.Parse(y.CreditBalance)))
                .ForMember(x => x.ContractExpirationDate,x=>x.MapFrom(y=>DateTime.Parse(y.ContractExpirationDate)))
                .ForMember(x => x.InitialEstimateValue, x => x.MapFrom(y => decimal.Parse(y.InitialEstimateValue)))
                .ForMember(x => x.InitialEstimateTime, x => x.MapFrom(y => DateTime.Parse(y.InitialEstimateTime)))
                .ForMember(x => x.BusinessId, x => x.MapFrom(y => y.BussinessId));
        }


        public static TResult ToModel<TResult>(this object entity)
        {
            return Mapper.Map<TResult>(entity);
        }

        public static List<TResult> ToListModel<TResult, TInput>(this IEnumerable<TInput> list)
        {
            return list.Select(x => x.ToModel<TResult>()).ToList();
        }
    }
}
