using System.Collections.Generic;

namespace Niusys.Extensions.ComponentModels
{
    /// <summary>
    /// 分页模型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Page<T>
    {
        /// <summary>
        /// 分页信息
        /// </summary>
        public Paging Paging { get; set; }

        /// <summary>
        /// 分页数据
        /// </summary>
        public List<T> Records { get; set; }
    }
}
