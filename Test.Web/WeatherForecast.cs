using System.ComponentModel;

namespace Test.Web
{
    public class WeatherForecast
    {

        /// <summary>
        /// 日期
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// 摄氏度
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// 华氏度
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        /// <summary>
        /// 
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Day Day { get; set; } = Day.Monday;
    }

    /// <summary>
    /// 
    /// </summary>
    public enum Day
    {
        /// <summary>
        /// Monday
        /// </summary>
        [Description("星期一")]
        Monday = 0,
    }
}