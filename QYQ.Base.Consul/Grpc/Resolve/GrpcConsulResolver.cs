using Consul;
using Grpc.Core;
using Grpc.Net.Client.Balancer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GrpcStatus = Grpc.Core.Status;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace QYQ.Base.Consul.Grpc.Resolve
{
    /// <summary>
    /// Grpc 自定义解析程序。
    /// 通过 Consul 的<b>阻塞查询（blocking query / watch）</b>感知服务实例变化：
    /// 无变化时请求在 Consul 端挂起、不产生无谓轮询；实例上/下线时秒级返回并刷新地址表。
    /// </summary>
    public class GrpcConsulResolver(ILoggerFactory loggerFactory) : PollingResolver(loggerFactory)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<GrpcConsulResolver>();

        /// <summary>
        /// Consul 客户端配置
        /// </summary>
        public ConsulServiceOptions? ConsulClientOption { get; set; }

        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// 阻塞查询单次最长挂起时间。到期后即使无变化也会返回，随即发起下一次 watch，形成长轮询。
        /// </summary>
        private static readonly TimeSpan _watchWaitTime = TimeSpan.FromMinutes(2);

        /// <summary>
        /// watch 循环异常后的退避时间，避免 Consul 抖动时打爆日志与请求。
        /// </summary>
        private static readonly TimeSpan _errorBackoff = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Consul 一致性索引，用于阻塞查询；仅当返回的 <see cref="QueryResult.LastIndex"/> 增大时才视为发生变化。
        /// </summary>
        private ulong _lastIndex;

        /// <summary>
        /// 串行化对 <see cref="PollingResolver.Listener"/> 的发布，避免 watch 循环与 <see cref="ResolveAsync"/> 并发发布产生竞争。
        /// </summary>
        private readonly object _publishLock = new();

        private CancellationTokenSource? _cts;

        private Task? _watchTask;

        private ConsulClient? _consulClient;

        /// <summary>
        /// 解析器启动：保留基类首次解析，并额外启动后台 watch 循环持续感知变化。
        /// </summary>
        protected override void OnStarted()
        {
            base.OnStarted();

            _cts = new CancellationTokenSource();
            _watchTask = Task.Run(() => WatchLoopAsync(_cts.Token));

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("OnStarted:{ServiceName} 后台 watch 循环已启动", ServiceName);
            }
        }

        /// <summary>
        /// 一次性解析路径：基类在首次启动与子通道连接失败时调用，做一次<b>非阻塞</b>查询并发布当前快照，
        /// 让连接失败时能立即重解析，与后台 watch 循环互补。
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        protected override async Task ResolveAsync(CancellationToken cancellationToken)
        {
            var client = EnsureClient();
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("ResolveAsync:开始对 {ServiceName} 进行一次性（非阻塞）解析", ServiceName);
            }
            // 非阻塞查询：WaitIndex 为 0，立即返回当前健康实例快照。
            var result = await client.Health.Service(ServiceName, tag: "", passingOnly: true, cancellationToken);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("ResolveAsync:{ServiceName} 一次性解析返回 {Count} 个健康实例", ServiceName, result.Response?.Length ?? 0);
            }
            Publish(result.Response);
        }

        /// <summary>
        /// 后台 watch 循环：以阻塞查询持续等待服务集变化，变化时刷新地址表。
        /// </summary>
        /// <param name="cancellationToken">取消令牌，随解析器释放而触发</param>
        private async Task WatchLoopAsync(CancellationToken cancellationToken)
        {
            var client = EnsureClient();
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var options = new QueryOptions
                    {
                        // 携带上次索引，让 Consul 在服务集变化或 WaitTime 到期前挂起本次请求。
                        WaitIndex = _lastIndex,
                        WaitTime = _watchWaitTime,
                    };
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("WatchLoopAsync:发起阻塞查询 {ServiceName}，WaitIndex={WaitIndex}，WaitTime={WaitTime}", ServiceName, _lastIndex, _watchWaitTime);
                    }
                    var result = await client.Health.Service(ServiceName, tag: "", passingOnly: true, options, cancellationToken);

                    // 仅在索引推进时才认为发生变化，避免 WaitTime 到期的无变化返回引起重复发布。
                    if (result.LastIndex > _lastIndex)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("WatchLoopAsync:检测到 {ServiceName} 服务集变化，索引 {OldIndex} -> {NewIndex}，健康实例 {Count} 个", ServiceName, _lastIndex, result.LastIndex, result.Response?.Length ?? 0);
                        }
                        _lastIndex = result.LastIndex;
                        Publish(result.Response);
                    }
                    else if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("WatchLoopAsync:{ServiceName} 本轮无变化，当前索引 {LastIndex}", ServiceName, _lastIndex);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // 释放时的正常取消，退出循环。
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError("WatchLoopAsync:{Message}\r\n{StackTrace}", ex.Message, ex.StackTrace);
                    // 索引重置，下轮从最新快照重新开始 watch。
                    _lastIndex = 0;
                    try
                    {
                        await Task.Delay(_errorBackoff, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 惰性创建 <see cref="ConsulClient"/>，配置校验失败时抛出。
        /// </summary>
        private ConsulClient EnsureClient()
        {
            if (ConsulClientOption == null)
            {
                const string message = "ConsulClientOption 未配置，无法解析服务地址";
                _logger.LogError(message);
                throw new InvalidOperationException(message);
            }
            if (_consulClient == null)
            {
                _consulClient = new ConsulClient(c =>
                {
                    c.Address = new Uri(ConsulClientOption.ConsulAddress);
                    c.Token = ConsulClientOption.Token;
                });
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("EnsureClient:已创建 Consul 客户端，地址 {ConsulAddress}", ConsulClientOption.ConsulAddress);
                }
            }
            return _consulClient;
        }

        /// <summary>
        /// 将 Consul 健康实例发布给负载均衡器。无可用实例时下发失败状态（保留现有子通道并按退避重试），
        /// 而非下发空地址表清空后端引发抖动。
        /// </summary>
        /// <param name="entries">Consul 返回的健康服务实例</param>
        private void Publish(ServiceEntry[]? entries)
        {
            var addresses = BuildAddresses(entries);
            lock (_publishLock)
            {
                if (addresses.Length == 0)
                {
                    // entries 为空/0 说明 Consul 本身没有返回健康实例（服务未注册、健康检查未通过或 tag/passingOnly 过滤掉）；
                    // entries 有值但地址为 0 则说明实例存在却映射不出地址（Service.Address 与 Node.Address 均为空），据此区分排查方向。
                    _logger.LogWarning("Publish:{ServiceName} 无可用服务地址，Consul 返回健康实例 {EntryCount} 个", ServiceName, entries?.Length ?? 0);
                    Listener(ResolverResult.ForFailure(new GrpcStatus(StatusCode.Unavailable, $"无可用的 {ServiceName} 服务实例")));
                    return;
                }
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Publish:向负载均衡器发布 {ServiceName} 的 {Count} 个服务地址", ServiceName, addresses.Length);
                }
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Publish:{ServiceName} 地址列表 {Addresses}", ServiceName, string.Join(", ", addresses.Select(a => $"{a.EndPoint.Host}:{a.EndPoint.Port}")));
                }
                // 解析结果通知负载均衡器
                Listener(ResolverResult.ForResult(addresses));
            }
        }

        /// <summary>
        /// 将 Consul 健康实例映射为 gRPC 负载均衡地址。
        /// 当服务未显式登记地址（<see cref="AgentService.Address"/> 为空）时回退到节点地址（<see cref="Node.Address"/>）。
        /// </summary>
        /// <param name="entries">Consul 返回的健康服务实例，可能为 null</param>
        /// <returns>负载均衡地址数组，无实例时为空数组</returns>
        internal static BalancerAddress[] BuildAddresses(IReadOnlyCollection<ServiceEntry>? entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return [];
            }
            return entries
                .Select(entry =>
                {
                    var host = string.IsNullOrEmpty(entry.Service.Address) ? entry.Node.Address : entry.Service.Address;
                    return new BalancerAddress(host, entry.Service.Port);
                })
                .ToArray();
        }

        /// <summary>
        /// 释放 watch 循环与 Consul 客户端，修复定时器/连接句柄泄漏。
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts?.Cancel();
                _consulClient?.Dispose();
                _cts?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
