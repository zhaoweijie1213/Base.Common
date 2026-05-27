using System;
using System.Collections.Generic;
using System.Linq;
using QYQ.Base.SnowId.Extension;
using SqlSugar;
using Xunit;

namespace QYQ.Base.Common.Test.SqlSugar
{
    /// <summary>
    /// CustomSplitTableExtension 泛型重载方法的单元测试。
    /// 验证从实体特性自动读取 SplitType 的行为。
    /// </summary>
    public class CustomSplitTableExtensionTests
    {
        // ----------------------------------------------------------------
        // 测试用实体类（内联定义，仅用于测试）
        // ----------------------------------------------------------------

        [SplitTable(SplitType.Season)]
        private class SeasonEntity { }

        [SplitTable(SplitType.Month)]
        private class MonthEntity { }

        [SplitTable(SplitType.Year)]
        private class YearEntity { }

        /// <summary>未标注 SplitTable 特性的实体，用于验证默认回退行为</summary>
        private class NoAttributeEntity { }

        // ----------------------------------------------------------------
        // 辅助方法
        // ----------------------------------------------------------------

        /// <summary>构造一组跨年度的按季度分表信息</summary>
        private static List<SplitTableInfo> BuildSeasonTables()
        {
            return new List<SplitTableInfo>
            {
                new SplitTableInfo { TableName = "order_2024q1", Date = new DateTime(2024, 1, 1) },
                new SplitTableInfo { TableName = "order_2024q2", Date = new DateTime(2024, 4, 1) },
                new SplitTableInfo { TableName = "order_2024q3", Date = new DateTime(2024, 7, 1) },
                new SplitTableInfo { TableName = "order_2024q4", Date = new DateTime(2024, 10, 1) },
                new SplitTableInfo { TableName = "order_2025q1", Date = new DateTime(2025, 1, 1) },
                new SplitTableInfo { TableName = "order_2025q2", Date = new DateTime(2025, 4, 1) },
            };
        }

        /// <summary>构造一组按月分表信息</summary>
        private static List<SplitTableInfo> BuildMonthTables()
        {
            return new List<SplitTableInfo>
            {
                new SplitTableInfo { TableName = "log_202401", Date = new DateTime(2024, 1, 1) },
                new SplitTableInfo { TableName = "log_202402", Date = new DateTime(2024, 2, 1) },
                new SplitTableInfo { TableName = "log_202403", Date = new DateTime(2024, 3, 1) },
                new SplitTableInfo { TableName = "log_202404", Date = new DateTime(2024, 4, 1) },
                new SplitTableInfo { TableName = "log_202405", Date = new DateTime(2024, 5, 1) },
            };
        }

        /// <summary>构造一组按年分表信息</summary>
        private static List<SplitTableInfo> BuildYearTables()
        {
            return new List<SplitTableInfo>
            {
                new SplitTableInfo { TableName = "archive_2022", Date = new DateTime(2022, 1, 1) },
                new SplitTableInfo { TableName = "archive_2023", Date = new DateTime(2023, 1, 1) },
                new SplitTableInfo { TableName = "archive_2024", Date = new DateTime(2024, 1, 1) },
                new SplitTableInfo { TableName = "archive_2025", Date = new DateTime(2025, 1, 1) },
            };
        }

        // ----------------------------------------------------------------
        // 测试用例
        // ----------------------------------------------------------------

        /// <summary>
        /// 验证：实体标注 SplitType.Season 时，泛型方法按季度过滤正确。
        /// </summary>
        [Fact]
        public void FilterByRange_WithSeasonAttribute_ReturnsCorrectTables()
        {
            var tables = BuildSeasonTables();

            // 2024-Q2 ~ 2024-Q4
            var result = tables.FilterSplitTablesByRange<SeasonEntity>(
                new DateTime(2024, 5, 15),
                new DateTime(2024, 11, 20));

            Assert.Equal(3, result.Count);
            Assert.Contains(result, t => t.TableName == "order_2024q2");
            Assert.Contains(result, t => t.TableName == "order_2024q3");
            Assert.Contains(result, t => t.TableName == "order_2024q4");
        }

        /// <summary>
        /// 验证：实体标注 SplitType.Month 时，泛型方法按月过滤正确。
        /// </summary>
        [Fact]
        public void FilterByRange_WithMonthAttribute_ReturnsCorrectTables()
        {
            var tables = BuildMonthTables();

            // 2024-02 ~ 2024-04
            var result = tables.FilterSplitTablesByRange<MonthEntity>(
                new DateTime(2024, 2, 10),
                new DateTime(2024, 4, 20));

            Assert.Equal(3, result.Count);
            Assert.Contains(result, t => t.TableName == "log_202402");
            Assert.Contains(result, t => t.TableName == "log_202403");
            Assert.Contains(result, t => t.TableName == "log_202404");
        }

        /// <summary>
        /// 验证：实体标注 SplitType.Year 时，泛型方法按年过滤正确。
        /// </summary>
        [Fact]
        public void FilterByRange_WithYearAttribute_ReturnsCorrectTables()
        {
            var tables = BuildYearTables();

            // 2023 ~ 2024
            var result = tables.FilterSplitTablesByRange<YearEntity>(
                new DateTime(2023, 6, 1),
                new DateTime(2024, 8, 1));

            Assert.Equal(2, result.Count);
            Assert.Contains(result, t => t.TableName == "archive_2023");
            Assert.Contains(result, t => t.TableName == "archive_2024");
        }

        /// <summary>
        /// 验证：实体未标注 SplitTable 特性时，默认回退到 SplitType.Season。
        /// </summary>
        [Fact]
        public void FilterByRange_WithNoAttribute_DefaultsToSeason()
        {
            var tables = BuildSeasonTables();

            // 与 SeasonEntity 结果一致（相同时间范围，相同默认 Season 逻辑）
            var resultGeneric = tables.FilterSplitTablesByRange<NoAttributeEntity>(
                new DateTime(2024, 5, 15),
                new DateTime(2024, 11, 20));

            var resultExplicit = tables.FilterSplitTablesByRange(
                new DateTime(2024, 5, 15),
                new DateTime(2024, 11, 20),
                SplitType.Season);

            Assert.Equal(resultExplicit.Count, resultGeneric.Count);
            Assert.Equal(
                resultExplicit.Select(t => t.TableName).OrderBy(x => x),
                resultGeneric.Select(t => t.TableName).OrderBy(x => x));
        }

        /// <summary>
        /// 验证：空列表输入时，返回空列表。
        /// </summary>
        [Fact]
        public void FilterByRange_EmptyList_ReturnsEmpty()
        {
            var tables = new List<SplitTableInfo>();

            var result = tables.FilterSplitTablesByRange<SeasonEntity>(
                new DateTime(2024, 1, 1),
                new DateTime(2024, 12, 31));

            Assert.Empty(result);
        }

        /// <summary>
        /// 验证：start &gt; end 时，自动交换后仍能正确过滤。
        /// </summary>
        [Fact]
        public void FilterByRange_StartGreaterThanEnd_SwapsAndFilters()
        {
            var tables = BuildSeasonTables();

            // 故意传入 end < start
            var result = tables.FilterSplitTablesByRange<SeasonEntity>(
                new DateTime(2024, 11, 20),   // 作为 start，但实际 > end
                new DateTime(2024, 5, 15));    // 作为 end

            Assert.Equal(3, result.Count);
            Assert.Contains(result, t => t.TableName == "order_2024q2");
            Assert.Contains(result, t => t.TableName == "order_2024q3");
            Assert.Contains(result, t => t.TableName == "order_2024q4");
        }
    }
}
