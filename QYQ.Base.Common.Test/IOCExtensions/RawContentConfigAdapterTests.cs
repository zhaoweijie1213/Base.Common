using System;
using System.Linq;
using System.Xml.Linq;
using Com.Ctrip.Framework.Apollo;
using Com.Ctrip.Framework.Apollo.Core;
using QYQ.Base.Common.IOCExtensions;
using Xunit;

namespace QYQ.Base.Common.Test.IOCExtensions
{
    /// <summary>
    /// 验证「只取原文、不摊平」的 Apollo 适配器及其注册入口。
    /// </summary>
    public class RawContentConfigAdapterTests
    {
        /// <summary>
        /// 策划表格导出的原始 XML 形态：根节点与子节点同名、属性名为中文，
        /// 这正是内置 XmlConfigAdapter 会撞键报错的内容。
        /// </summary>
        private const string ExportedGameListXml = """
            <item>
            	<item ID="1" 游戏ID="塔城麻将" GameID="2800" GameType="2" DistrictID="654200"/>
            	<item ID="2" 游戏ID="营口麻将" GameID="2809" GameType="2" DistrictID="210800"/>
            </item>
            """;

        /// <summary>
        /// 任意文本都应原样落在 content 键上，适配器不做任何解析。
        /// </summary>
        [Fact]
        public void GetProperties_ShouldKeepRawContent_OnContentKey()
        {
            const string content = "任意文本 with English & <not-xml>";

            var properties = RawContentConfigAdapter.Instance.GetProperties(content);

            var names = properties.GetPropertyNames();
            Assert.Single(names);
            Assert.Contains(ConfigConsts.ConfigFileContentKey, names);
            Assert.Equal(content, properties.GetProperty(ConfigConsts.ConfigFileContentKey));
        }

        /// <summary>
        /// 含同名兄弟节点的 XML 不得抛出，且内容一个字符都不能被改写——
        /// 这是游戏列表配置迁移到 Apollo 的核心前提。
        /// </summary>
        [Fact]
        public void GetProperties_ShouldNotThrowOrRewrite_WhenXmlHasDuplicateSiblings()
        {
            var properties = RawContentConfigAdapter.Instance.GetProperties(ExportedGameListXml);

            var raw = properties.GetProperty(ConfigConsts.ConfigFileContentKey);
            Assert.Equal(ExportedGameListXml, raw);

            // 原文仍是合法 XML，业务侧可以自行解析出全部同名节点
            var entries = XDocument.Parse(raw).Root!.Elements().ToList();
            Assert.Equal(2, entries.Count);
            Assert.Equal("塔城麻将", entries[0].Attribute("游戏ID")?.Value);
        }

        /// <summary>
        /// 服务端未下发内容时应降级为空字符串，而不是抛空引用。
        /// </summary>
        [Fact]
        public void GetProperties_ShouldReturnEmptyContent_WhenContentIsNull()
        {
            var properties = RawContentConfigAdapter.Instance.GetProperties((string)null!);

            Assert.Equal(string.Empty, properties.GetProperty(ConfigConsts.ConfigFileContentKey));
        }

        /// <summary>
        /// sectionKey 为空会让原文落到配置根级并与其他 Namespace 互相覆盖，
        /// 因此必须在触碰构建器之前就直接拒绝。
        /// </summary>
        /// <param name="sectionKey">待校验的配置键前缀。</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddQYQRawNamespace_ShouldThrowArgumentException_WhenSectionKeyIsBlank(string sectionKey)
        {
            // 传入 null 构建器：校验一旦通过就会立即 NullReferenceException，
            // 因此本用例同时守住了「参数校验先于任何构建器操作」这一时序。
            var exception = Assert.Throws<ArgumentException>(
                () => ApolloConfigExtension.AddQYQRawNamespace(null!, "GameList", sectionKey));

            Assert.Equal("sectionKey", exception.ParamName);
        }
    }
}
