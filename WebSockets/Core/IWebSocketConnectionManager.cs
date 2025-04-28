using System.Net.WebSockets;

namespace std.WebSockets.Core
{
    /// <summary>
    /// WebSocket连接管理器接口
    /// </summary>
    public interface IWebSocketConnectionManager
    {
        // 连接管理
        Task<string> AddConnectionAsync(WebSocket socket);
        Task<bool> RemoveConnectionAsync(string id);
        bool TryGetConnection(string id, out IWebSocketConnection connection);
        IEnumerable<IWebSocketConnection> GetAllConnections();
        int ActiveConnectionCount { get; }

        // 消息发送
        Task BroadcastAsync(string message);
        Task BroadcastAsync(ArraySegment<byte> binaryData);
        Task<bool> SendToAsync(string connectionId, string message);
        Task<bool> SendToAsync(string connectionId, ArraySegment<byte> binaryData);
    }


}