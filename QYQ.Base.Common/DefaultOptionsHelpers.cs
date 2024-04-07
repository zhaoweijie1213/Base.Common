using System.Collections.Generic;

internal static class DefaultOptionsHelpers
{
    /// <summary>
    /// 默认忽略路径
    /// </summary>
    public static List<string> DefaultIgnorePath = ["/api/Health", "/grpc.health.v1.Health/Check", "swagger"];


}