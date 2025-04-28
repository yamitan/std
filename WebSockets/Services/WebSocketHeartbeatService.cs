using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using std.WebSockets.Core;

namespace std.WebSockets.Services
{
    /// <summary>
    /// WebSocket心跳配置选项
    /// </summary>
    public class WebSocketHeartbeatOptions
    {
        /// <summary>
        /// 心跳间隔时间，默认30秒
        /// </summary>
        public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(60);
        
        /// <summary>
        /// 连接超时时间，默认120秒
        /// </summary>
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(120);
        
        /// <summary>
        /// 是否启用心跳检测
        /// </summary>
        public bool EnableHeartbeat { get; set; } = true;
        
        /// <summary>
        /// 是否启用自动断开超时连接
        /// </summary>
        public bool EnableTimeoutDisconnect { get; set; } = true;
    }
    
    /// <summary>
    /// WebSocket心跳和连接监控服务
    /// </summary>
    public class WebSocketHeartbeatService : BackgroundService
    {
        private readonly ILogger<WebSocketHeartbeatService> _logger;
        private readonly IWebSocketConnectionManager _connectionManager;
        private readonly WebSocketHeartbeatOptions _options;
        
        public WebSocketHeartbeatService(
            ILogger<WebSocketHeartbeatService> logger,
            IWebSocketConnectionManager connectionManager,
            IOptions<WebSocketHeartbeatOptions> options)
        {
            _logger = logger;
            _connectionManager = connectionManager;
            _options = options.Value;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WebSocket心跳服务已启动，心跳间隔：{HeartbeatInterval}，超时时间：{ConnectionTimeout}",
                _options.HeartbeatInterval, _options.ConnectionTimeout);
            
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_options.HeartbeatInterval, stoppingToken);
                    await CheckConnectionsAsync();
                }
                catch (OperationCanceledException)
                {
                    // 服务停止时的正常取消，忽略异常
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "WebSocket心跳检测过程中发生错误");
                }
            }
            
            _logger.LogInformation("WebSocket心跳服务已停止");
        }
        
        /// <summary>
        /// 检查所有连接状态
        /// </summary>
        private async Task CheckConnectionsAsync()
        {
            var connections = _connectionManager.GetAllConnections().ToList();
            var now = DateTime.UtcNow;
            var deadConnections = new List<string>();
            var pingTasks = new List<Task>();
            
            _logger.LogDebug("开始检查 {ConnectionCount} 个WebSocket连接", connections.Count);
            
            foreach (var connection in connections)
            {
                // 检查连接是否超时
                if (_options.EnableTimeoutDisconnect && 
                    connection.LastActivityTime < now.Subtract(_options.ConnectionTimeout))
                {
                    _logger.LogWarning("检测到超时连接 {ConnectionId}，上次活动时间：{LastActivity}，将断开连接",
                        connection.Id, connection.LastActivityTime);
                    
                    deadConnections.Add(connection.Id);
                    continue;
                }
                
                // 发送心跳
                if (_options.EnableHeartbeat && connection.IsActive)
                {
                    pingTasks.Add(SendPingAsync(connection));
                }
            }
            
            // 等待所有心跳发送完成
            if (pingTasks.Count > 0)
            {
                await Task.WhenAll(pingTasks);
            }
            
            // 断开超时连接
            foreach (var connectionId in deadConnections)
            {
                if (_connectionManager.TryGetConnection(connectionId, out var connection))
                {
                    try
                    {
                        await connection.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "连接超时");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "关闭超时连接 {ConnectionId} 时发生错误", connectionId);
                    }
                    finally
                    {
                        await _connectionManager.RemoveConnectionAsync(connectionId);
                    }
                }
            }
            
            if (deadConnections.Count > 0)
            {
                _logger.LogInformation("已断开 {Count} 个超时连接", deadConnections.Count);
            }
        }
        
        /// <summary>
        /// 发送心跳消息
        /// </summary>
        private async Task SendPingAsync(IWebSocketConnection connection)
        {
            try
            {
                var pingMessage = JsonSerializer.Serialize(new { type = "ping", timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() });
                await connection.SendAsync(pingMessage);
                _logger.LogDebug("已发送心跳消息到连接 {ConnectionId}", connection.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "向连接 {ConnectionId} 发送心跳消息时出错", connection.Id);
            }
        }
    }
} 