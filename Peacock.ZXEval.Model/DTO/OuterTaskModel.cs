using System;

namespace Peacock.ZXEval.Model.DTO
{
    public class OuterTaskModel
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
        public virtual ProjectModel Project { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
    }
}
