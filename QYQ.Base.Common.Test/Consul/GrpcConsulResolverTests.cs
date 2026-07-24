using Consul;
using QYQ.Base.Consul.Grpc.Resolve;
using Xunit;

namespace QYQ.Base.Common.Test.Consul
{
    /// <summary>
    /// 验证 <see cref="GrpcConsulResolver"/> 将 Consul 健康实例映射为 gRPC 负载均衡地址的行为，
    /// 重点覆盖服务地址为空时回退节点地址这一 Consul 常见坑。
    /// </summary>
    public class GrpcConsulResolverTests
    {
        /// <summary>
        /// 服务已显式登记地址时，应直接使用服务地址与端口。
        /// </summary>
        [Fact]
        public void BuildAddresses_ShouldUseServiceAddress_WhenPresent()
        {
            var entries = new[]
            {
                new ServiceEntry
                {
                    Node = new Node { Address = "10.0.0.1" },
                    Service = new AgentService { Address = "192.168.1.10", Port = 5001 },
                },
            };

            var addresses = GrpcConsulResolver.BuildAddresses(entries);

            var address = Assert.Single(addresses);
            Assert.Equal("192.168.1.10", address.EndPoint.Host);
            Assert.Equal(5001, address.EndPoint.Port);
        }

        /// <summary>
        /// 服务未登记地址（Address 为空）时，应回退到节点地址，端口仍取服务端口。
        /// </summary>
        [Fact]
        public void BuildAddresses_ShouldFallbackToNodeAddress_WhenServiceAddressEmpty()
        {
            var entries = new[]
            {
                new ServiceEntry
                {
                    Node = new Node { Address = "10.0.0.2" },
                    Service = new AgentService { Address = string.Empty, Port = 6001 },
                },
            };

            var addresses = GrpcConsulResolver.BuildAddresses(entries);

            var address = Assert.Single(addresses);
            Assert.Equal("10.0.0.2", address.EndPoint.Host);
            Assert.Equal(6001, address.EndPoint.Port);
        }

        /// <summary>
        /// 无健康实例（null 或空集合）时应返回空数组，由上层决定失败语义。
        /// </summary>
        [Fact]
        public void BuildAddresses_ShouldReturnEmpty_WhenNoEntries()
        {
            Assert.Empty(GrpcConsulResolver.BuildAddresses(null));
            Assert.Empty(GrpcConsulResolver.BuildAddresses([]));
        }
    }
}
