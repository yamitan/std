using System.Net.WebSockets;

namespace std.WebSockets.Core
{
    /// <summary>
    /// WebSocket路由处理器接口
    /// </summary>
    public interface IWebSocketRouteHandler
    {
        bool HasRoute(string path);
        Task HandleRouteAsync(string path, WebSocket webSocket, IWebSocketConnection connection);
        void AddRoute(string path, Func<WebSocket, IWebSocketConnection, Task> handler);
    }
}