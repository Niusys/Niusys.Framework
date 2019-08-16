using System;

namespace Niusys.Extensions.ComponentModels
{
    /// <summary>
    /// 分页信息
    /// </summary>
    public class Paging
    {
        private int _pageIndex;
        /// <summary>
        /// 第几页(从1开始)
        /// </summary>
        public int PageIndex
        {
            get => _pageIndex;
            set => _pageIndex = value <= 0 ? 1 : value;
        }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        ///  每页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 是否存在下一页
        /// </summary>
        public bool HasNextPage => Total > PageIndex * PageSize;
    }
}
