using Newtonsoft.Json;
using QYQ.Base.Common.Extension;
using System.Collections.Generic;

namespace QYQ.Base.Common.ApiResult
{
    /// <summary>
    /// API 分页数据，默认使用 long? 作为 lastId 游标类型。
    /// </summary>
    /// <typeparam name="T">分页列表中的数据类型。</typeparam>
    public class ApiPageResult<T> : ApiPageResult<T, long?>
    {
    }

    /// <summary>
    /// 带游标的 API 分页数据，统一承载 API 返回状态与分页列表。
    /// </summary>
    /// <typeparam name="T">分页列表中的数据类型。</typeparam>
    /// <typeparam name="TLastId">下一页游标的数据类型。</typeparam>
    public class ApiPageResult<T, TLastId> : ApiResult<List<T>>
    {
        /// <summary>
        /// API 返回码。
        /// </summary>
        [JsonProperty("code")]
        public new int Code
        {
            get => base.Code;
            set => base.Code = value;
        }

        /// <summary>
        /// API 提示消息。
        /// </summary>
        [JsonProperty("message")]
        public new string Message
        {
            get => base.Message;
            set => base.Message = value;
        }

        /// <summary>
        /// 当前页数据列表。
        /// </summary>
        [JsonProperty("data")]
        public new List<T>? Data
        {
            get => base.Data;
            set => base.Data = value;
        }

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
        /// 当前页最后一条数据的游标值，用于继续请求下一页。
        /// </summary>
        [JsonProperty("lastId", NullValueHandling = NullValueHandling.Ignore)]
        public TLastId? LastId { get; set; }

        /// <summary>
        /// 设置 API 分页返回结果，并默认标记为成功状态。
        /// </summary>
        /// <param name="data">当前页数据列表，传入 null 时返回空列表。</param>
        /// <param name="pageIndex">当前页码，未传入时不输出该字段。</param>
        /// <param name="pageSize">当前页面大小，未传入时不输出该字段。</param>
        /// <param name="total">符合查询条件的数据总数，未传入时不输出该字段。</param>
        /// <param name="lastId">当前页最后一条数据的游标值，未传入时不输出该字段。</param>
        /// <param name="code">API 返回码，默认表示成功。</param>
        /// <param name="message">API 提示消息，未传入时使用返回码描述。</param>
        /// <returns>包含 API 状态、分页列表与分页游标的返回结果。</returns>
        public ApiPageResult<T, TLastId> SetPageResult(
            List<T>? data,
            int? pageIndex = null,
            int? pageSize = null,
            int? total = null,
            TLastId? lastId = default,
            ApiResultCode code = ApiResultCode.Success,
            string? message = null)
        {
            Code = (int)code;
            Message = message ?? code.GetDescription();
            Data = data ?? [];
            PageIndex = pageIndex;
            PageSize = pageSize;
            Total = total;
            LastId = lastId;
            return this;
        }
    }
}
