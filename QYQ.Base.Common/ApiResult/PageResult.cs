using Newtonsoft.Json;
using System.Collections.Generic;

namespace QYQ.Base.Common.ApiResult
{
    /// <summary>
    /// 分页数据，用于保持旧前端依赖的 list 分页结构。
    /// </summary>
    /// <typeparam name="T">分页列表中的数据类型。</typeparam>
    public class PageResult<T>
    {
        /// <summary>
        /// 符合当前查询条件的数据总数。
        /// </summary>
        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public int? Total { get; set; }

        /// <summary>
        /// 当前分页请求的页面大小。
        /// </summary>
        [JsonProperty("pageSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? PageSize { get; set; }

        /// <summary>
        /// 当前分页请求的页码。
        /// </summary>
        [JsonProperty("pageIndex", NullValueHandling = NullValueHandling.Ignore)]
        public int? PageIndex { get; set; }

        /// <summary>
        /// 当前页数据列表，保留旧接口使用的 list 字段。
        /// </summary>
        [JsonProperty("list", NullValueHandling = NullValueHandling.Ignore)]
        public List<T> List { get; set; } = [];

        /// <summary>
        /// 设置旧结构分页返回结果。
        /// </summary>
        /// <param name="data">当前页数据列表。</param>
        /// <param name="pageIndex">当前页码，未传入时不输出该字段。</param>
        /// <param name="pageSize">当前页面大小，未传入时不输出该字段。</param>
        /// <param name="total">符合查询条件的数据总数，未传入时不输出该字段。</param>
        /// <returns>包含 list 与分页信息的旧结构分页结果。</returns>
        public PageResult<T> SetPageResult(List<T> data, int? pageIndex = null, int? pageSize = null, int? total = null)
        {
            PageIndex = pageIndex;
            List = data;
            PageSize = pageSize;
            Total = total;
            return this;
        }
    }
}
