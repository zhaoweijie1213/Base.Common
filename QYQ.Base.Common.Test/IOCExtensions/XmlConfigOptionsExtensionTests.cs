using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QYQ.Base.Common.IOCExtensions;
using QYQ.Base.Common.Xml;
using Xunit;

namespace QYQ.Base.Common.Test.IOCExtensions
{
    /// <summary>
    /// 验证 XML 原文配置选项的注册与热更新行为。
    /// </summary>
    public class XmlConfigOptionsExtensionTests
    {
        /// <summary>
        /// 初始的两条游戏列表。
        /// </summary>
        private const string InitialXml = """
            <item>
            	<item ID="1" 游戏ID="塔城麻将" GameID="2800" GameType="2"/>
            	<item ID="2" 游戏ID="营口麻将" GameID="2809" GameType="2"/>
            </item>
            """;

        /// <summary>
        /// 模拟运维在 Apollo 上改完发布后的内容：改了一条、加了一条。
        /// </summary>
        private const string UpdatedXml = """
            <item>
            	<item ID="1" 游戏ID="塔城麻将" GameID="9999" GameType="2"/>
            	<item ID="2" 游戏ID="营口麻将" GameID="2809" GameType="2"/>
            	<item ID="3" 游戏ID="阿坝麻将" GameID="116" GameType="2"/>
            </item>
            """;

        /// <summary>
        /// 注册后应能直接从 IOptionsMonitor 拿到解析结果，使用方无需自定义 Options 类。
        /// </summary>
        [Fact]
        public void AddQYQXmlOptions_ShouldParseContent_IntoXmlConfigOptions()
        {
            var (monitor, _) = BuildMonitor(InitialXml);

            var items = monitor.CurrentValue.Items;

            Assert.Equal(2, items.Count);
            Assert.Equal("塔城麻将", items[0].Title);
            Assert.Equal(2800, items[0].GameId);
        }

        /// <summary>
        /// 配置变更（对应 Apollo 推送新内容）后 CurrentValue 必须重算。
        /// 这条守卫的是 ConfigurationChangeTokenSource：少了它编译照过、运行不报错，
        /// 只是热更新静默失效。
        /// </summary>
        [Fact]
        public void AddQYQXmlOptions_ShouldRecomputeCurrentValue_WhenConfigurationReloads()
        {
            var (monitor, source) = BuildMonitor(InitialXml);

            Assert.Equal(2, monitor.CurrentValue.Items.Count);
            Assert.Equal(2800, monitor.CurrentValue.Items[0].GameId);

            source.Provider.SetContent(UpdatedXml);

            Assert.Equal(3, monitor.CurrentValue.Items.Count);
            Assert.Equal(9999, monitor.CurrentValue.Items[0].GameId);
        }

        /// <summary>
        /// 变更通知也应能传到 OnChange 回调，供业务侧做刷新后的联动。
        /// </summary>
        [Fact]
        public void AddQYQXmlOptions_ShouldRaiseOnChange_WhenConfigurationReloads()
        {
            var (monitor, source) = BuildMonitor(InitialXml);

            var changedCount = 0;
            using var registration = monitor.OnChange(_ => changedCount++);

            source.Provider.SetContent(UpdatedXml);

            Assert.True(changedCount > 0);
        }

        /// <summary>
        /// 配置里没有该键时给空集合，交由调用方决定是否沿用旧数据。
        /// </summary>
        [Fact]
        public void AddQYQXmlOptions_ShouldReturnEmpty_WhenContentKeyMissing()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddQYQXmlOptions<GameListEntry>("GameList");

            using var provider = services.BuildServiceProvider();
            var monitor = provider.GetRequiredService<IOptionsMonitor<XmlConfigOptions<GameListEntry>>>();

            Assert.Empty(monitor.CurrentValue.Items);
        }

        /// <summary>
        /// 原文畸形时 CurrentValue 抛本库的统一异常，容错由业务侧自行决定。
        /// </summary>
        [Fact]
        public void AddQYQXmlOptions_ShouldThrow_WhenContentIsMalformed()
        {
            var (monitor, _) = BuildMonitor("<item><item ID=\"1\"</item>");

            Assert.Throws<XmlContentParseException>(() => monitor.CurrentValue);
        }

        /// <summary>
        /// sectionKey 为空会去读配置根级的 content，必然是调用方写错了。
        /// </summary>
        /// <param name="sectionKey">待校验的配置键前缀。</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddQYQXmlOptions_ShouldThrowArgumentException_WhenSectionKeyIsBlank(string sectionKey)
        {
            var services = new ServiceCollection();

            var exception = Assert.Throws<ArgumentException>(() => services.AddQYQXmlOptions<GameListEntry>(sectionKey));

            Assert.Equal("sectionKey", exception.ParamName);
        }

        /// <summary>
        /// 搭一套「可触发重载的配置 + DI 容器」，返回选项监视器与可改内容的配置源。
        /// </summary>
        /// <param name="content">初始的 XML 原文。</param>
        /// <returns>选项监视器与配置源。</returns>
        private static (IOptionsMonitor<XmlConfigOptions<GameListEntry>> Monitor, ReloadableContentSource Source) BuildMonitor(string content)
        {
            var source = new ReloadableContentSource("GameList", content);
            var configuration = new ConfigurationBuilder().Add(source).Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddQYQXmlOptions<GameListEntry>("GameList");

            var provider = services.BuildServiceProvider();

            return (provider.GetRequiredService<IOptionsMonitor<XmlConfigOptions<GameListEntry>>>(), source);
        }

        /// <summary>
        /// 测试用配置源，模拟 Apollo 把整段原文放在 {sectionKey}:content 上并可随时推送新内容。
        /// </summary>
        /// <param name="sectionKey">配置键前缀。</param>
        /// <param name="content">初始原文。</param>
        private sealed class ReloadableContentSource(string sectionKey, string content) : IConfigurationSource
        {
            /// <summary>【含义】可主动触发重载的配置提供程序。</summary>
            public ReloadableContentProvider Provider { get; } = new(sectionKey, content);

            /// <summary>
            /// 构建配置提供程序。
            /// </summary>
            /// <param name="builder">配置构建器。</param>
            /// <returns>配置提供程序。</returns>
            public IConfigurationProvider Build(IConfigurationBuilder builder) => Provider;
        }

        /// <summary>
        /// 测试用配置提供程序，SetContent 等价于 Apollo 长轮询拿到新内容后触发的配置重载。
        /// </summary>
        /// <param name="sectionKey">配置键前缀。</param>
        /// <param name="content">初始原文。</param>
        private sealed class ReloadableContentProvider(string sectionKey, string content) : ConfigurationProvider
        {
            private readonly string _contentKey = $"{sectionKey}:content";

            /// <summary>
            /// 载入初始原文。
            /// </summary>
            public override void Load() => Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [_contentKey] = content,
            };

            /// <summary>
            /// 【业务意图】替换原文并触发配置重载令牌，模拟 Apollo 推送。
            /// </summary>
            /// <param name="newContent">新的 XML 原文。</param>
            public void SetContent(string newContent)
            {
                Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    [_contentKey] = newContent,
                };
                OnReload();
            }
        }

        /// <summary>
        /// 游戏列表配置的投影模型。
        /// </summary>
        public class GameListEntry
        {
            /// <summary>【含义】表内序号。</summary>
            [XmlAttributeName("ID")]
            public int Id { get; set; }

            /// <summary>【含义】游戏名称。</summary>
            [XmlAttributeName("游戏ID")]
            public string Title { get; set; } = string.Empty;

            /// <summary>【含义】游戏编号。</summary>
            [XmlAttributeName("GameID")]
            public int GameId { get; set; }

            /// <summary>【含义】游戏玩法类型。</summary>
            [XmlAttributeName("GameType")]
            public int GameType { get; set; }
        }
    }
}
