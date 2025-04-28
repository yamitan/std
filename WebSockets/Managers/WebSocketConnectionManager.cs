using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using std.WebSockets.Core;
using std.WebSockets.Models;

namespace std.WebSockets.Managers
{
    /// <summary>
    /// 管理 WebSocket 连接的实现
    /// </summary>
    public class WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger) : IWebSocketConnectionManager
    {
        private readonly ILogger<WebSocketConnectionManager> _logger = logger; // 日志记录器
        private readonly ConcurrentDictionary<string, IWebSocketConnection> _connections = new(); // 存储所有 WebSocket 连接

        /// <summary>
        /// 获取活跃连接数
        /// </summary>
        public int ActiveConnectionCount => _connections.Count;

        /// <summary>
        /// 添加新的 WebSocket 连接
        /// </summary>
        /// <param name="socket">要添加的 WebSocket 实例</param>
        /// <returns>返回新添加连接的ID</returns>
        public async Task<string> AddConnectionAsync(WebSocket socket)
        {
            var connection = new WebSocketConnection(socket); // 创建新的 WebSocket 连接
            _connections.TryAdd(connection.Id, connection); // 将连接添加到字典中

            _logger.LogInformation("WebSocket连接已添加，ID: {ConnectionId}，当前连接数: {Count}",
                connection.Id, _connections.Count); // 记录连接信息

            await Task.CompletedTask; // 这里可以是其他异步操作
            return connection.Id; // 返回新连接ID
        }

        /// <summary>
        /// 移除 WebSocket 连接
        /// </summary>
        /// <param name="id">要移除的连接 ID</param>
        /// <returns>如果连接被成功移除，则为true；否则为false</returns>
        public async Task<bool> RemoveConnectionAsync(string id)
        {
            bool removed = _connections.TryRemove(id, out var connection);
            if (removed)
            {
                _logger.LogInformation("WebSocket连接已移除，ID: {ConnectionId}，当前连接数: {Count}",
                    id, _connections.Count); // 记录移除信息
            }

            await Task.CompletedTask;
            return removed;
        }

        /// <summary>
        /// 尝试获取指定ID的连接
        /// </summary>
        /// <param name="id">连接ID</param>
        /// <param name="connection">如果找到连接，则包含该连接；否则为null</param>
        /// <returns>如果找到连接，则为true；否则为false</returns>
        public bool TryGetConnection(string id, out IWebSocketConnection connection)
        {
            return _connections.TryGetValue(id, out connection);
        }

        /// <summary>
        /// 获取所有连接
        /// </summary>
        /// <returns>所有WebSocket连接的集合</returns>
        public IEnumerable<IWebSocketConnection> GetAllConnections()
        {
            return _connections.Values;
        }

        /// <summary>
        /// 向所有连接广播文本消息
        /// </summary>
        /// <param name="message">要广播的消息</param>
        public async Task BroadcastAsync(string message)
        {
            var tasks = new List<Task>();
            var deadConnections = new List<string>();

            foreach (var connection in _connections)
            {
                try
                {
                    if (connection.Value.State == WebSocketState.Open)
                    {
                        tasks.Add(connection.Value.SendAsync(message));
                    }
                    else
                    {
                        deadConnections.Add(connection.Key);
                    }
                }
                catch (Exception)
                {
                    deadConnections.Add(connection.Key);
                }
            }

            // 移除已关闭的连接
            foreach (var id in deadConnections)
            {
                await RemoveConnectionAsync(id);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 向所有连接广播二进制数据
        /// </summary>
        /// <param name="binaryData">要广播的二进制数据</param>
        public async Task BroadcastAsync(ArraySegment<byte> binaryData)
        {
            var tasks = new List<Task>();
            var deadConnections = new List<string>();

            foreach (var connection in _connections)
            {
                try
                {
                    if (connection.Value.State == WebSocketState.Open)
                    {
                        tasks.Add(connection.Value.SendAsync(binaryData));
                    }
                    else
                    {
                        deadConnections.Add(connection.Key);
                    }
                }
                catch (Exception)
                {
                    deadConnections.Add(connection.Key);
                }
            }

            // 移除已关闭的连接
            foreach (var id in deadConnections)
            {
                await RemoveConnectionAsync(id);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// 向指定连接发送文本消息
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="message">要发送的消息</param>
        /// <returns>如果消息发送成功，则为true；否则为false</returns>
        public async Task<bool> SendToAsync(string connectionId, string message)
        {
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                try
                {
                    await connection.SendAsync(message);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发送消息到连接 {ConnectionId} 时出错", connectionId);
                    await RemoveConnectionAsync(connectionId);
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 向指定连接发送二进制数据
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="binaryData">要发送的二进制数据</param>
        /// <returns>如果数据发送成功，则为true；否则为false</returns>
        public async Task<bool> SendToAsync(string connectionId, ArraySegment<byte> binaryData)
        {
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                try
                {
                    await connection.SendAsync(binaryData);
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "发送二进制数据到连接 {ConnectionId} 时出错", connectionId);
                    await RemoveConnectionAsync(connectionId);
                    return false;
                }
            }
            return false;
        }
    }
}