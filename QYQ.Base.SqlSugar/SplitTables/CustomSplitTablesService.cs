using SqlSugar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace QYQ.Base.SqlSugar.SplitTables
{
    /// <summary>
    /// 自定义 通用年/月/日分表服务（自动解析模板）
    /// </summary>
    public class CustomSplitTablesService : DateSplitTableService
    {
        /// <summary>
        /// 获取所有表
        /// </summary>
        /// <param name="db"></param>
        /// <param name="entityInfo"></param>
        /// <param name="tableInfos"></param>
        /// <returns></returns>
        public override List<SplitTableInfo> GetAllTables(ISqlSugarClient db, EntityInfo entityInfo, List<DbTableInfo> tableInfos)
        {
            var template = entityInfo.DbTableName; // 例如：gamegameStartUser{year}{month}{day}

            // 动态生成正则表达式
            string pattern = Regex.Escape(template) // 先把固定部分转义，避免特殊字符误匹配
                .Replace(@"\{year\}", @"(?<year>\d{4})") // 年：4位数字
                .Replace(@"\{month\}", @"(?<month>\d{0,2})") // 月：可选，最多2位
                .Replace(@"\{day\}", @"(?<day>\d{0,2})"); // 日：可选，最多2位

            pattern = "^" + pattern + "$"; // 完整匹配表名

            var result = new List<SplitTableInfo>();

            foreach (var table in tableInfos.Select(it => it.Name).Reverse())
            {
                var match = Regex.Match(table, pattern, RegexOptions.IgnoreCase);

                SplitTableInfo tableInfo = new SplitTableInfo
                {
                    TableName = table,
                    Date = DateTime.MinValue
                };

                if (match.Success)
                {
                    int year = ParseOrDefault(match.Groups["year"].Value, DateTime.Now.Year);
                    int month = ParseOrDefault(match.Groups["month"].Value, 1); // 默认 1 月
                    int day = ParseOrDefault(match.Groups["day"].Value, 1); // 默认 1 号

                    tableInfo.Date = SafeCreateDate(year, month, day);
                }

                result.Add(tableInfo);
            }

            return result.OrderByDescending(it => it.Date).ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private int ParseOrDefault(string value, int defaultValue)
        {
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        private DateTime SafeCreateDate(int year, int month, int day)
        {
            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}
