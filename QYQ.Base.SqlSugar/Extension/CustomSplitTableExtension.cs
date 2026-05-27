using SqlSugar;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.SnowId.Extension
{
    /// <summary>
    /// 分表信息列表的扩展方法类
    /// </summary>
    public static class CustomSplitTableExtension
    {
        /// <summary>
        /// 按时间范围过滤分表信息列表，返回符合条件的分表信息列表。分表类型可以是按年、季、月、周或日进行划分。
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="splitType"></param>
        /// <returns></returns>
        public static List<SplitTableInfo> FilterSplitTablesByRange(this List<SplitTableInfo> tables, DateTime start, DateTime end, SplitType splitType = SplitType.Season)
        {
            if (tables.Count == 0)
            {
                return new List<SplitTableInfo>();
            }

            if (start > end)
            {
                (start, end) = (end, start);
            }

            var normalizedStart = NormalizeSplitDate(start, splitType);
            var normalizedEnd = NormalizeSplitDate(end, splitType);

            List<SplitTableInfo> result = tables
                .Where(table =>
                {
                    var tableDate = NormalizeSplitDate(table.Date, splitType);
                    return tableDate >= normalizedStart && tableDate <= normalizedEnd;
                })
                .OrderBy(table => table.Date)
                .ToList();

            return result;
        }

        /// <summary>
        /// 按时间范围过滤分表信息列表，自动从实体类型 <typeparamref name="T"/> 上的
        /// <see cref="SplitTableAttribute"/> 特性中读取分表类型。
        /// 若实体类未标注该特性，则默认使用 <see cref="SplitType.Season"/>。
        /// </summary>
        /// <typeparam name="T">实体类型，应标注 [SplitTable(SplitType.xxx)] 特性</typeparam>
        /// <param name="tables">分表信息列表</param>
        /// <param name="start">开始时间</param>
        /// <param name="end">结束时间</param>
        /// <returns>符合条件的分表信息列表</returns>
        public static List<SplitTableInfo> FilterSplitTablesByRange<T>(
            this List<SplitTableInfo> tables,
            DateTime start,
            DateTime end)
            where T : class, new()
        {
            var attr = typeof(T).GetCustomAttribute<SplitTableAttribute>();
            var splitType = attr?.SplitType ?? SplitType.Season;
            return tables.FilterSplitTablesByRange(start, end, splitType);
        }

        private static DateTime NormalizeSplitDate(DateTime date, SplitType splitType)
        {
            return splitType switch
            {
                SplitType.Year => new DateTime(date.Year, 1, 1),
                SplitType.Season => new DateTime(date.Year, ((date.Month - 1) / 3) * 3 + 1, 1),
                SplitType.Month => new DateTime(date.Year, date.Month, 1),
                SplitType.Week => GetWeekStartDate(date),
                SplitType.Day => date.Date,
                _ => date.Date
            };
        }

        private static DateTime GetWeekStartDate(DateTime date)
        {
            var dayOfWeek = CultureInfo.InvariantCulture.DateTimeFormat.FirstDayOfWeek;
            var diff = (7 + (date.DayOfWeek - dayOfWeek)) % 7;
            return date.Date.AddDays(-diff);
        }
    }
}
