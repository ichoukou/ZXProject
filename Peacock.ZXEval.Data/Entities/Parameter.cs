﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peacock.ZXEval.Data.Entities
{
    /// <summary>
    /// 参数表
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// 系统唯一标识
        /// </summary>
        public long Id { set; get; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// 父级对象ID
        /// </summary>
        public long ParentId { get; set; }

    }
    internal class ParameterConfig : EntityConfig<Parameter>
    {
        internal ParameterConfig()
            : base(200)
        {
            base.HasKey(x => x.Id);
            base.ToTable("SystemParameter");
        }
    }
}
