using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Common.Tool
{
    /// <summary>
    /// 时间扩展
    /// </summary>
    public static class DateTimeExtension
    {
        /// <summary>
        /// 获取时间戳(毫秒)
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStamp(this DateTime date)
        {
            TimeSpan ts = date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
            //long epoch = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }


        /// <summary>
        /// 获取时间戳(秒)
        /// </summary>
        /// <returns></returns>
        public static long GetTimeStampFromSeconds(this DateTime date)
        {
            TimeSpan ts = date.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
            //long epoch = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }

        /// <summary>
        /// 时间戳(utc时间)转换为本地时间
        /// </summary>
        /// <param name="timestamp">时间戳(s)</param>
        /// <returns></returns>
        public static DateTime TimeSpanToLocalDataTime(this long timestamp)
        {

            //UTC时间转本地时区
            DateTime Start = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), TimeZoneInfo.Local);//等价的建议写法

            //秒转换为ticks 100纳秒为单位
            long ticks = timestamp * 10000000;
            TimeSpan toNow = new(ticks);
            //加上时间戳
            DateTime dateTime = Start.Add(toNow);
            //DateTime resTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

            return dateTime;
        }


        /// <summary>
        /// 时间戳转为utc时间
        /// </summary>
        /// <param name="timestamp">时间戳(s)</param>
        /// <returns></returns>
        public static DateTime TimeSpanToUtcDataTime(this long timestamp)
        {

            //UTC时间转本地时区
            DateTime Start = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);//等价的建议写法

            //秒转换为ticks 100纳秒为单位
            long ticks = timestamp * 10000000;
            TimeSpan toNow = new(ticks);
            //加上时间戳
            DateTime dateTime = Start.Add(toNow);
            //DateTime resTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);

            return dateTime;
        }

    }
}
