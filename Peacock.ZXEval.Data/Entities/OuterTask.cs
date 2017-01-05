using System;

namespace Peacock.ZXEval.Data.Entities
{
    public class OuterTask
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 预约时间
        /// </summary>
        public DateTime? AppointmentDate { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? FinishDate { get; set; }

        /// <summary>
        /// 项目导航属性
        /// </summary>
        public virtual Project Project { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
    }

    internal class OuterTaskConfig : EntityConfig<OuterTask>
    {
        internal OuterTaskConfig()
        {
            ToTable("OuterTask");
            HasKey(x => x.Id);
            HasRequired(x => x.Project).WithRequiredDependent(x => x.OuterTask).Map(x => x.MapKey("ProjectID"));
        }
    }
}
