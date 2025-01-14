using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.SnowId.Options
{
    /// <summary>
    /// 配置选项
    /// </summary>
    public class SnowIdOptions
    {
        /// <summary>
        /// 基础时间(utc时间)
        /// 默认值为 new DateTime(2020, 2, 20, 2, 20, 2, 20, DateTimeKind.Utc);
        /// </summary>
        public DateTime? BaseTime { get; set; }

        /// <summary>
        /// 数据中心Id
        /// </summary>
        public int DataCenterId {  get; set; }
    }
}
