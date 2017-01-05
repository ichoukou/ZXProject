using System;

namespace Peacock.ZXEval.Data.Entities
{
    public class ProjectStateInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 项目
        /// </summary>
        public long ProjectId { get; set; }

        public virtual Project Project { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 反馈内容
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// 操作时间
        /// </summary>
        public DateTime OperationTime { get; set; }

    }

    internal class ProjectStateInfoConfig : EntityConfig<ProjectStateInfo>
    {
        internal ProjectStateInfoConfig()
        {
            ToTable("ProjectStateInfo");
            HasKey(x => x.Id);
            HasRequired(x => x.Project).WithMany(x => x.ProjectStateInfos).HasForeignKey(x => x.ProjectId);
            //HasRequired(x => x.Operator).WithMany().HasForeignKey(x => x.OperatorID);
        }
    }
}
