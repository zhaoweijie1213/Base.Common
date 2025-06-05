using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QYQ.Base.Consul
{
    /// <summary>
    /// 网络相关工具类
    /// </summary>
    public static class NetworkUtil
    {
        /// <summary>
        /// 获取“最合适”的主机 IPv4 地址：
        /// 1、优先从配置中读 HostIPAddress（如果外部传入了），
        /// 2、其次通过 UDP 模拟出口路由获取，
        /// 3、最后回退到遍历网卡挑第一个 Up/非 Loopback/非 Tunnel 的 IPv4。
        /// 如果三者都失败，则返回空字符串。
        /// </summary>
        public static string GetHostIPv4(string configuredIp = null)
        {
            // 1. 如果调用方已在配置里指定，直接用它
            if (!string.IsNullOrEmpty(configuredIp))
            {
                return configuredIp;
            }

            // 2. 尝试通过 UDP “出口” 方式获取
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
                {
                    // 连接到一个公共地址，不用真的发包，触发路由抉择
                    socket.Connect("1.1.1.1", 53);
                    if (socket.LocalEndPoint is IPEndPoint localEP)
                    {
                        return localEP.Address.ToString();
                    }
                }
            }
            catch
            {
                // 忽略异常，继续下面再用网卡遍历法
            }

            // 3. 回退到 “枚举网卡” 方式
            var candidates = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    ni.OperationalStatus == OperationalStatus.Up &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                    ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                )
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses
                    .Where(uni =>
                        uni.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(uni.Address)
                    )
                    .Select(uni => uni.Address))
                .ToList();

            return candidates.FirstOrDefault()?.ToString() ?? string.Empty;
        }
    }
}
