using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.ApiResult
{
    /// <summary>
    /// 分页数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResult<T>
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 页面大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<T> List { get; set; } = [];


        /// <summary>
        /// 设置分页
        /// 返回结果
        /// </summary>
        /// <param name="data"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public PageResult<T> SetPageRsult(List<T> data, int pageIndex = 1, int pageSize = 20, int total = 0)
        {
            PageIndex = pageIndex;
            List = data;
            PageSize = pageSize;
            Total = total;
            return this;
        }

    }
}
