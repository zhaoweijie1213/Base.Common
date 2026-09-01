using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using QYQ.Base.Common.Extension;
using QYQ.Base.Common.Xml;
using Xunit;

namespace QYQ.Base.Common.Test.Extension
{
    /// <summary>
    /// 验证 XML 原文按特性映射为对象集合的解析行为。
    /// </summary>
    public class XmlContentExtensionTests
    {
        /// <summary>
        /// 策划表格导出的原始形态：根节点与子节点同名、属性名含中文。
        /// 第 1 行是根节点，第 2、3 行各是一条记录，行号断言依赖这个布局。
        /// </summary>
        private const string ExportedGameListXml = """
            <item>
            	<item ID="1" 游戏ID="塔城麻将" GameID="2800" GameType="2" DistrictID="654200"/>
            	<item ID="2" 游戏ID="营口麻将" GameID="2809" GameType="2" DistrictID="210800"/>
            </item>
            """;

        /// <summary>
        /// 原始导出形态应能完整解析，中文属性名与同名兄弟节点都不成问题。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldMapExportedGameList_WithChineseAttributeName()
        {
            var entries = ExportedGameListXml.ParseXmlItems<GameListEntry>();

            Assert.Equal(2, entries.Count);

            Assert.Equal(1, entries[0].Id);
            Assert.Equal("塔城麻将", entries[0].Title);
            Assert.Equal(2800, entries[0].GameId);
            Assert.Equal(2, entries[0].GameType);
            Assert.Equal(654200, entries[0].DistrictId);

            Assert.Equal("营口麻将", entries[1].Title);
            Assert.Equal(2809, entries[1].GameId);
        }

        /// <summary>
        /// 根节点叫什么都不影响解析——导出宏可以随时改根节点名，不该把它写死进代码。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldIgnoreRootElementName()
        {
            const string xml = """
                <GameList>
                	<row ID="7" 游戏ID="斗地主" GameID="1001" GameType="1"/>
                </GameList>
                """;

            var entries = xml.ParseXmlItems<GameListEntry>();

            Assert.Single(entries);
            Assert.Equal(1001, entries[0].GameId);
        }

        /// <summary>
        /// 指定 elementName 时只取同名子节点，混排的其他节点应被忽略。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldOnlyTakeMatchingElements_WhenElementNameGiven()
        {
            const string xml = """
                <root>
                	<item ID="1" 游戏ID="塔城麻将" GameID="2800" GameType="2"/>
                	<remark ID="9" 游戏ID="备注" GameID="0" GameType="0"/>
                	<item ID="2" 游戏ID="营口麻将" GameID="2809" GameType="2"/>
                </root>
                """;

            var entries = xml.ParseXmlItems<GameListEntry>("item");

            Assert.Equal(2, entries.Count);
            Assert.Equal([2800, 2809], [entries[0].GameId, entries[1].GameId]);
        }

        /// <summary>
        /// Apollo 未下发内容时 content 就是空串，此时返回空集合而不是抛异常。
        /// </summary>
        /// <param name="content">待解析的原文。</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ParseXmlItems_ShouldReturnEmpty_WhenContentIsBlank(string content)
        {
            var entries = XmlContentExtension.ParseXmlItems<GameListEntry>(content);

            Assert.Empty(entries);
        }

        /// <summary>
        /// 未标注映射特性的属性完全不参与映射，也不因 XML 上没有同名属性而报错。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldIgnoreProperty_WhenAttributeNotAnnotated()
        {
            var entries = ExportedGameListXml.ParseXmlItems<GameListEntry>();

            Assert.Equal("未参与映射", entries[0].Remark);
        }

        /// <summary>
        /// Required = false 的属性缺失或留空时保留默认值，可空类型为 null。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldKeepDefault_WhenOptionalAttributeMissingOrEmpty()
        {
            const string xml = """
                <root>
                	<item ID="1" 游戏ID="塔城麻将" GameID="2800" GameType="2"/>
                	<item ID="2" 游戏ID="营口麻将" GameID="2809" GameType="2" DistrictID=""/>
                </root>
                """;

            var entries = xml.ParseXmlItems<GameListEntry>();

            Assert.Null(entries[0].DistrictId);
            Assert.Null(entries[1].DistrictId);
        }

        /// <summary>
        /// 必填属性缺失时抛出，异常要能直接指出是哪一行的哪个属性。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldThrow_WhenRequiredAttributeMissing()
        {
            const string xml = """
                <item>
                	<item ID="1" 游戏ID="塔城麻将" GameType="2"/>
                </item>
                """;

            var exception = Assert.Throws<XmlContentParseException>(() => xml.ParseXmlItems<GameListEntry>());

            Assert.Equal("GameID", exception.AttributeName);
            Assert.Equal(2, exception.LineNumber);
            Assert.Equal(0, exception.ItemIndex);
            Assert.Contains("GameID", exception.Message);
        }

        /// <summary>
        /// 属性值转换失败时抛出，异常要带上行号、属性名和写错的原始值。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldThrow_WhenValueCannotConvert()
        {
            const string xml = """
                <item>
                	<item ID="1" 游戏ID="塔城麻将" GameID="2800" GameType="2"/>
                	<item ID="2" 游戏ID="营口麻将" GameID="abc" GameType="2"/>
                </item>
                """;

            var exception = Assert.Throws<XmlContentParseException>(() => xml.ParseXmlItems<GameListEntry>());

            Assert.Equal("GameID", exception.AttributeName);
            Assert.Equal("abc", exception.RawValue);
            Assert.Equal(typeof(int), exception.TargetType);
            Assert.Equal(3, exception.LineNumber);
            Assert.Equal(1, exception.ItemIndex);
        }

        /// <summary>
        /// Required 只管「缺不缺」；属性写了但值不合法，即便 Required = false 也照样抛。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldThrow_WhenOptionalValueCannotConvert()
        {
            const string xml = """
                <item>
                	<item ID="1" 游戏ID="塔城麻将" GameID="2800" GameType="2" DistrictID="不是数字"/>
                </item>
                """;

            var exception = Assert.Throws<XmlContentParseException>(() => xml.ParseXmlItems<GameListEntry>());

            Assert.Equal("DistrictID", exception.AttributeName);
            Assert.Equal("不是数字", exception.RawValue);
        }

        /// <summary>
        /// 常用类型都应能从文本转换，布尔额外接受配置里常见的 1/0，枚举名称与数值皆可。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldConvertSupportedTypes()
        {
            const string xml = """
                <root>
                	<item Flag="1" Amount="12.34" StartTime="2026-09-01T08:30:00" Key="8c9d0a1b-2e3f-4a5b-8c9d-0a1b2e3f4a5b" Category="Mahjong" Ratio="0.75"/>
                	<item Flag="true" Amount="-0.5" StartTime="2026-01-31" Key="00000000-0000-0000-0000-000000000000" Category="3"/>
                </root>
                """;

            var entries = xml.ParseXmlItems<TypedEntry>();

            Assert.True(entries[0].Flag);
            Assert.Equal(12.34m, entries[0].Amount);
            Assert.Equal(new DateTime(2026, 9, 1, 8, 30, 0), entries[0].StartTime);
            Assert.Equal(Guid.Parse("8c9d0a1b-2e3f-4a5b-8c9d-0a1b2e3f4a5b"), entries[0].Key);
            Assert.Equal(GameCategory.Mahjong, entries[0].Category);
            Assert.Equal(0.75d, entries[0].Ratio);

            Assert.True(entries[1].Flag);
            Assert.Equal(-0.5m, entries[1].Amount);
            Assert.Equal(GameCategory.Poker, entries[1].Category);
            Assert.Null(entries[1].Ratio);
        }

        /// <summary>
        /// 运维在 Portal 上贴了畸形 XML 时，抛的应是本库统一的异常类型，
        /// 调用方只需 catch 一种就能兜住所有解析失败。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldThrowXmlContentParseException_WhenXmlIsMalformed()
        {
            const string xml = "<item><item ID=\"1\"</item>";

            var exception = Assert.Throws<XmlContentParseException>(() => xml.ParseXmlItems<GameListEntry>());

            Assert.IsType<System.Xml.XmlException>(exception.InnerException);
        }

        /// <summary>
        /// 不受支持的目标类型在建立映射时就报错，而不是等跑到某一行数据才炸。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldThrow_WhenPropertyTypeNotSupported()
        {
            const string xml = "<root><item Value=\"1\"/></root>";

            var exception = Assert.Throws<XmlContentParseException>(() => xml.ParseXmlItems<UnsupportedEntry>());

            Assert.Contains("Value", exception.Message);
            Assert.Equal(-1, exception.ItemIndex);
        }

        /// <summary>
        /// 标了映射特性却没有公开 set 访问器，属于 DTO 写错了，应尽早报错。
        /// </summary>
        [Fact]
        public void ParseXmlItems_ShouldThrow_WhenPropertyHasNoPublicSetter()
        {
            const string xml = "<root><item ID=\"1\"/></root>";

            var exception = Assert.Throws<XmlContentParseException>(() => xml.ParseXmlItems<ReadOnlyEntry>());

            Assert.Contains("set", exception.Message);
        }

        /// <summary>
        /// GetXmlItems 应从 {sectionKey}:content 取原文，与 AddQYQRawNamespace 的写入端对齐。
        /// </summary>
        [Fact]
        public void GetXmlItems_ShouldReadContentKeyAndParse()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["GameList:content"] = ExportedGameListXml,
                })
                .Build();

            var entries = configuration.GetXmlItems<GameListEntry>("GameList");

            Assert.Equal(2, entries.Count);
            Assert.Equal("塔城麻将", entries[0].Title);
        }

        /// <summary>
        /// 配置里根本没有该键时返回空集合，交由调用方决定是否沿用旧数据。
        /// </summary>
        [Fact]
        public void GetXmlItems_ShouldReturnEmpty_WhenKeyMissing()
        {
            var configuration = new ConfigurationBuilder().Build();

            Assert.Empty(configuration.GetXmlItems<GameListEntry>("GameList"));
        }

        /// <summary>
        /// sectionKey 为空会去读配置根级的 content，语义上必然是调用方写错了。
        /// </summary>
        /// <param name="sectionKey">待校验的配置键前缀。</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetXmlItems_ShouldThrowArgumentException_WhenSectionKeyIsBlank(string sectionKey)
        {
            var configuration = new ConfigurationBuilder().Build();

            var exception = Assert.Throws<ArgumentException>(() => configuration.GetXmlItems<GameListEntry>(sectionKey));

            Assert.Equal("sectionKey", exception.ParamName);
        }

        /// <summary>
        /// 游戏列表配置的投影模型，属性名全英文，中文只出现在映射特性的字符串里。
        /// </summary>
        public class GameListEntry
        {
            /// <summary>【含义】表内序号。</summary>
            [XmlAttributeName("ID")]
            public int Id { get; set; }

            /// <summary>【含义】游戏名称。表头历史上误写成「游戏ID」，此处按原样匹配。</summary>
            [XmlAttributeName("游戏ID")]
            public string Title { get; set; } = string.Empty;

            /// <summary>【含义】游戏编号。</summary>
            [XmlAttributeName("GameID")]
            public int GameId { get; set; }

            /// <summary>【含义】游戏玩法类型。</summary>
            [XmlAttributeName("GameType")]
            public int GameType { get; set; }

            /// <summary>【含义】所属地区编号，允许不配。</summary>
            [XmlAttributeName("DistrictID", Required = false)]
            public int? DistrictId { get; set; }

            /// <summary>【含义】未标注映射特性，用于验证不参与映射。</summary>
            public string Remark { get; set; } = "未参与映射";
        }

        /// <summary>
        /// 覆盖各类受支持的目标类型。
        /// </summary>
        public class TypedEntry
        {
            /// <summary>【含义】布尔值，接受 true/false 与 1/0。</summary>
            [XmlAttributeName("Flag")]
            public bool Flag { get; set; }

            /// <summary>【含义】金额。</summary>
            [XmlAttributeName("Amount")]
            public decimal Amount { get; set; }

            /// <summary>【含义】开始时间。</summary>
            [XmlAttributeName("StartTime")]
            public DateTime StartTime { get; set; }

            /// <summary>【含义】唯一标识。</summary>
            [XmlAttributeName("Key")]
            public Guid Key { get; set; }

            /// <summary>【含义】游戏分类，名称与数值都能识别。</summary>
            [XmlAttributeName("Category")]
            public GameCategory Category { get; set; }

            /// <summary>【含义】比例，允许不配。</summary>
            [XmlAttributeName("Ratio", Required = false)]
            public double? Ratio { get; set; }
        }

        /// <summary>
        /// 属性类型不受支持的反例。
        /// </summary>
        public class UnsupportedEntry
        {
            /// <summary>【含义】object 无法从文本转换，应在建立映射时被拦下。</summary>
            [XmlAttributeName("Value")]
            public object Value { get; set; } = new();
        }

        /// <summary>
        /// 标了映射特性却没有公开 set 访问器的反例。
        /// </summary>
        public class ReadOnlyEntry
        {
            /// <summary>【含义】只读属性，无法被赋值。</summary>
            [XmlAttributeName("ID")]
            public int Id { get; private set; }
        }

        /// <summary>
        /// 测试用的游戏分类枚举。
        /// </summary>
        public enum GameCategory
        {
            /// <summary>未知。</summary>
            Unknown = 0,

            /// <summary>麻将。</summary>
            Mahjong = 2,

            /// <summary>扑克。</summary>
            Poker = 3,
        }
    }
}
