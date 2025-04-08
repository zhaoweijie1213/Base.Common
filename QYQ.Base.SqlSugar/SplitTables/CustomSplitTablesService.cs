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
        /// 根据已有表信息，返回所有分表信息（用于查询时确定目标分表）
        /// </summary>
        public override List<SplitTableInfo> GetAllTables(ISqlSugarClient db, EntityInfo EntityInfo, List<DbTableInfo> tableInfos)
        {
            //CheckTableName(EntityInfo.DbTableName);
            // 根据模板动态构造正则表达式
            string regex = "^" + EntityInfo.DbTableName
                .Replace("{year}", "([0-9]{2,4})");

            bool hasMonth = EntityInfo.DbTableName.Contains("{month}");
            bool hasDay = EntityInfo.DbTableName.Contains("{day}");

            if (hasMonth)
            {
                regex = regex.Replace("{month}", "([0-9]{1,2})");
            }
            if (hasDay)
            {
                regex = regex.Replace("{day}", "([0-9]{1,2})");
            }

            var currentTables = tableInfos
                .Where(it => Regex.IsMatch(it.Name, regex, RegexOptions.IgnoreCase))
                .Select(it => it.Name)
                .Reverse()
                .ToList();

            List<SplitTableInfo> result = new List<SplitTableInfo>();
            foreach (var item in currentTables)
            {
                SplitTableInfo tableInfo = new SplitTableInfo();
                tableInfo.TableName = item;

                //得到表前缀
                string tableNamePrefix = Regex.Replace(item, @"(\d{4,})(\d{2})?(\d{2})?$", "");
                //获取日期
                string date = item.Replace(tableNamePrefix, "");
                if (date.Length == 4)
                {
                    tableInfo.Date = new DateTime(Convert.ToInt32(date), 1, 1);
                }
                else
                {
                    var match = Regex.Match(item, regex, RegexOptions.IgnoreCase);
                    //var oldMatch = Regex.Match(item, oldRegex, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int year = 0, month = 1, day = 1; // 默认月日为1
                        if (match.Groups.Count > 1)
                        {
                            int.TryParse(match.Groups[1].Value, out year);
                        }
                        if (hasMonth && match.Groups.Count > 2)
                        {
                            int.TryParse(match.Groups[2].Value, out month);
                        }
                        if (hasDay && match.Groups.Count > 3)
                        {
                            int.TryParse(match.Groups[3].Value, out day);
                        }
                        // 构造日期，如果year解析失败，则使用默认值（比如当前年份或其它逻辑）
                        try
                        {
                            tableInfo.Date = new DateTime(year, month, day);
                        }
                        catch
                        {
                            tableInfo.Date = DateTime.MinValue;
                        }
                    }
                }
                result.Add(tableInfo);
            }

            result = result.OrderByDescending(it => it.Date).ToList();
            return result;
        }
    }
}
