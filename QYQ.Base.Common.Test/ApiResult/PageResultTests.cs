using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QYQ.Base.Common.ApiResult;
using Xunit;

namespace QYQ.Base.Common.Test.ApiResult
{
    /// <summary>
    /// 验证旧分页结构与新 API 分页结构的兼容行为。
    /// </summary>
    public class PageResultTests
    {
        /// <summary>
        /// 旧 PageResult 应继续输出 list 字段，避免旧前端升级后报错。
        /// </summary>
        [Fact]
        public void PageResult_ShouldKeepOldListContract()
        {
            var result = new PageResult<int>().SetPageResult([1, 2], pageIndex: 1, pageSize: 20, total: 2);

            var json = JObject.Parse(JsonConvert.SerializeObject(result));

            Assert.Equal(new[] { 1, 2 }, json["list"]!.ToObject<int[]>());
            Assert.Equal(2, json.Value<int>("total"));
            Assert.Equal(20, json.Value<int>("pageSize"));
            Assert.Equal(1, json.Value<int>("pageIndex"));
            Assert.Null(json["code"]);
            Assert.Null(json["message"]);
            Assert.Null(json["data"]);
            Assert.Null(json["lastId"]);
        }

        /// <summary>
        /// ApiPageResult 设置分页结果时应默认返回成功状态。
        /// </summary>
        [Fact]
        public void ApiPageResult_ShouldUseSuccessCodeAndMessage_ByDefault()
        {
            var result = new ApiPageResult<int>().SetPageResult([1, 2], pageIndex: 1, pageSize: 20, total: 2, lastId: 2);

            Assert.Equal((int)ApiResultCode.Success, result.Code);
            Assert.Equal("Success", result.Message);
            Assert.Equal(new List<int> { 1, 2 }, result.Data);
        }

        /// <summary>
        /// ApiPageResult 序列化时应输出统一字段并且不输出旧 list 字段。
        /// </summary>
        [Fact]
        public void ApiPageResult_ShouldUseDataAndPaginationFields_WithoutListField()
        {
            var result = new ApiPageResult<int>().SetPageResult([1, 2], pageIndex: 1, pageSize: 20, total: 2, lastId: 2);

            var json = JObject.Parse(JsonConvert.SerializeObject(result));

            Assert.Equal(0, json.Value<int>("code"));
            Assert.Equal("Success", json.Value<string>("message"));
            Assert.Equal(new[] { 1, 2 }, json["data"]!.ToObject<int[]>());
            Assert.Equal(2, json.Value<int>("total"));
            Assert.Equal(20, json.Value<int>("pageSize"));
            Assert.Equal(1, json.Value<int>("pageIndex"));
            Assert.Equal(2, json.Value<int>("lastId"));
            Assert.Null(json["list"]);
        }

        /// <summary>
        /// ApiPageResult 应支持字符串 lastId 游标。
        /// </summary>
        [Fact]
        public void ApiPageResult_ShouldSupportStringLastId()
        {
            var result = new ApiPageResult<int, string>().SetPageResult([1], lastId: "cursor-001");

            Assert.Equal("cursor-001", result.LastId);
        }

        /// <summary>
        /// ApiPageResult 的数据列表为空时应返回空数组而不是 null。
        /// </summary>
        [Fact]
        public void ApiPageResult_ShouldUseEmptyList_WhenDataIsNull()
        {
            var result = new ApiPageResult<int>().SetPageResult(null);

            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }
    }
}
