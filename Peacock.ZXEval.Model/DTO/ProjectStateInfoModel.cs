using System;

namespace Peacock.ZXEval.Model.DTO
{
    public class ProjectStateInfoModel
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

        //public virtual ProjectModel Project { get; set; }

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
}
