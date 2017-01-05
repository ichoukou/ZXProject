using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.ZXEval.Data.Entities
{
    public class ProjectResource
    {

        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 资源格式
        /// </summary>
        public string FileFormat { get; set; }

        /// <summary>
        /// 存储路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 资料类型
        /// </summary>
        public ResourcesEnum ResourcesType { get; set; }

        /// <summary>
        /// 项目ID
        /// </summary>
        public long ProjectId { get; set; }


        public DateTime CreateTime { get; set; }
    }

    /// <summary>
    /// 资料枚举
    /// </summary>
    public enum ResourcesEnum
    {
        /// <summary>
        /// 报告
        /// </summary>
        正式报告 = 0,

        /// <summary>
        /// 附件
        /// </summary>
        附件,

        /// <summary>
        /// 预估报告
        /// </summary>
        预估报告,
    }


    internal class ProjectResourceConfig : EntityConfig<ProjectResource>
    {
        internal ProjectResourceConfig()
        {
            ToTable("ProjectResource");
            HasKey(p => p.Id);
        }
    }
}
