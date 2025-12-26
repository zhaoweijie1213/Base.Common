namespace QYQ.Base.SnowId.Options
{
    /// <summary>
    /// SnowId Redis 配置项
    /// </summary>
    public class SnowIdRedisOptions
    {
        /// <summary>
        /// 默认 Provider 名称
        /// </summary>
        public const string DefaultProviderName = "SnowIdRedis";

        /// <summary>
        /// EasyCaching Provider 名称
        /// </summary>
        public string ProviderName { get; set; } = DefaultProviderName;
    }
}
