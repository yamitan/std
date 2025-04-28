using System.Net.WebSockets;

namespace std.WebSockets.Core
{
    /// <summary>
    /// WebSocket消息处理器接口
    /// </summary>
    public interface IWebSocketMessageHandler
    {
        Task HandleMessageAsync(WebSocketReceiveResult result, byte[] buffer, IWebSocketConnection connection);
    }
} 