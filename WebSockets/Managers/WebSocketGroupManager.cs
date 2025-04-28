using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using std.WebSockets.Core;

namespace std.WebSockets.Managers
{
    /// <summary>
    /// WebSocket分组管理器实现
    /// </summary>
    public class WebSocketGroupManager : IWebSocketGroupManager
    {
        private readonly ILogger<WebSocketGroupManager> _logger;
        private readonly IWebSocketConnectionManager _connectionManager;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _groups = new();
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _connectionGroups = new();

        public WebSocketGroupManager(
            ILogger<WebSocketGroupManager> logger,
            IWebSocketConnectionManager connectionManager)
        {
            _logger = logger;
            _connectionManager = connectionManager;
        }

        /// <summary>
        /// 将连接添加到指定分组
        /// </summary>
        public async Task AddToGroupAsync(string connectionId, string groupName)
        {
            if (string.IsNullOrEmpty(connectionId) || string.IsNullOrEmpty(groupName))
                return;

            // 确保分组存在
            var group = _groups.GetOrAdd(groupName, _ => new ConcurrentDictionary<string, byte>());
            group.TryAdd(connectionId, 0);

            // 更新连接的分组映射
            var connectionGroups = _connectionGroups.GetOrAdd(connectionId, _ => new ConcurrentDictionary<string, byte>());
            connectionGroups.TryAdd(groupName, 0);

            _logger.LogInformation("连接 {ConnectionId} 已添加到分组 {GroupName}, 分组中连接数: {Count}", 
                connectionId, groupName, group.Count);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 将连接从指定分组移除
        /// </summary>
        public async Task RemoveFromGroupAsync(string connectionId, string groupName)
        {
            if (string.IsNullOrEmpty(connectionId) || string.IsNullOrEmpty(groupName))
                return;

            if (_groups.TryGetValue(groupName, out var group))
            {
                group.TryRemove(connectionId, out _);

                // 如果分组为空，移除分组
                if (group.IsEmpty)
                {
                    _groups.TryRemove(groupName, out _);
                }
            }

            // 更新连接的分组映射
            if (_connectionGroups.TryGetValue(connectionId, out var connectionGroups))
            {
                connectionGroups.TryRemove(groupName, out _);

                // 如果连接不在任何分组中，移除映射
                if (connectionGroups.IsEmpty)
                {
                    _connectionGroups.TryRemove(connectionId, out _);
                }
            }

            _logger.LogInformation("连接 {ConnectionId} 已从分组 {GroupName} 移除", connectionId, groupName);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 将连接从所有分组移除
        /// </summary>
        public async Task RemoveFromAllGroupsAsync(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
                return;

            if (_connectionGroups.TryRemove(connectionId, out var connectionGroups))
            {
                foreach (var groupName in connectionGroups.Keys)
                {
                    if (_groups.TryGetValue(groupName, out var group))
                    {
                        group.TryRemove(connectionId, out _);

                        // 如果分组为空，移除分组
                        if (group.IsEmpty)
                        {
                            _groups.TryRemove(groupName, out _);
                        }
                    }
                }
            }

            _logger.LogInformation("连接 {ConnectionId} 已从所有分组移除", connectionId);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 向分组发送文本消息
        /// </summary>
        public async Task<int> SendToGroupAsync(string groupName, string message)
        {
            if (string.IsNullOrEmpty(groupName))
                return 0;

            if (!_groups.TryGetValue(groupName, out var connectionIds))
                return 0;

            var tasks = new List<Task>();
            var successCount = 0;
            var deadConnections = new List<string>();

            foreach (var connectionId in connectionIds.Keys)
            {
                if (_connectionManager.TryGetConnection(connectionId, out var connection))
                {
                    if (connection.State == WebSocketState.Open)
                    {
                        try
                        {
                            tasks.Add(connection.SendAsync(message));
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "向分组 {GroupName} 中的连接 {ConnectionId} 发送消息时出错", 
                                groupName, connectionId);
                            deadConnections.Add(connectionId);
                        }
                    }
                    else
                    {
                        deadConnections.Add(connectionId);
                    }
                }
                else
                {
                    deadConnections.Add(connectionId);
                }
            }

            // 移除无效连接
            foreach (var connectionId in deadConnections)
            {
                await RemoveFromGroupAsync(connectionId, groupName);
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("向分组 {GroupName} 发送消息，成功发送连接数: {SuccessCount}", 
                groupName, successCount);

            return successCount;
        }

        /// <summary>
        /// 向分组发送二进制数据
        /// </summary>
        public async Task<int> SendToGroupAsync(string groupName, ArraySegment<byte> binaryData)
        {
            if (string.IsNullOrEmpty(groupName))
                return 0;

            if (!_groups.TryGetValue(groupName, out var connectionIds))
                return 0;

            var tasks = new List<Task>();
            var successCount = 0;
            var deadConnections = new List<string>();

            foreach (var connectionId in connectionIds.Keys)
            {
                if (_connectionManager.TryGetConnection(connectionId, out var connection))
                {
                    if (connection.State == WebSocketState.Open)
                    {
                        try
                        {
                            tasks.Add(connection.SendAsync(binaryData));
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "向分组 {GroupName} 中的连接 {ConnectionId} 发送二进制数据时出错", 
                                groupName, connectionId);
                            deadConnections.Add(connectionId);
                        }
                    }
                    else
                    {
                        deadConnections.Add(connectionId);
                    }
                }
                else
                {
                    deadConnections.Add(connectionId);
                }
            }

            // 移除无效连接
            foreach (var connectionId in deadConnections)
            {
                await RemoveFromGroupAsync(connectionId, groupName);
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("向分组 {GroupName} 发送二进制数据，成功发送连接数: {SuccessCount}", 
                groupName, successCount);

            return successCount;
        }

        /// <summary>
        /// 向分组发送对象（自动序列化为JSON）
        /// </summary>
        public async Task<int> SendObjectToGroupAsync<T>(string groupName, T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            return await SendToGroupAsync(groupName, json);
        }

        /// <summary>
        /// 获取分组中的所有连接
        /// </summary>
        public IEnumerable<IWebSocketConnection> GetConnectionsInGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName) || !_groups.TryGetValue(groupName, out var connectionIds))
                return Enumerable.Empty<IWebSocketConnection>();

            var connections = new List<IWebSocketConnection>();

            foreach (var connectionId in connectionIds.Keys)
            {
                if (_connectionManager.TryGetConnection(connectionId, out var connection))
                {
                    connections.Add(connection);
                }
            }

            return connections;
        }

        /// <summary>
        /// 获取包含指定连接的所有分组
        /// </summary>
        public IEnumerable<string> GetGroupsForConnection(string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId) || !_connectionGroups.TryGetValue(connectionId, out var groups))
                return Enumerable.Empty<string>();

            return groups.Keys.ToList();
        }

        /// <summary>
        /// 获取所有分组名称
        /// </summary>
        public IEnumerable<string> GetAllGroups()
        {
            return _groups.Keys.ToList();
        }

        /// <summary>
        /// 获取指定分组中的连接数
        /// </summary>
        public int GetConnectionCountInGroup(string groupName)
        {
            if (string.IsNullOrEmpty(groupName) || !_groups.TryGetValue(groupName, out var group))
                return 0;

            return group.Count;
        }
    }
} 