using std.WebSockets.Core;
using System.Net.WebSockets;
using System.Text.Json;

namespace std.WebSockets.Handlers
{
    /// <summary>
    /// WebSocket路由处理器，管理WebSocket的路由映射和请求分发
    /// </summary>
    public class WebSocketRouteHandler : IWebSocketRouteHandler
    {
        private readonly Dictionary<string, Func<WebSocket, IWebSocketConnection, Task>> _routeHandlers;
        private readonly ILogger<WebSocketRouteHandler> _logger;

        public WebSocketRouteHandler(
            ILogger<WebSocketRouteHandler> logger,
            IWebSocketMessageHandler messageHandler)
        {
            _logger = logger;

            // 初始化路由处理字典
            _routeHandlers = new Dictionary<string, Func<WebSocket, IWebSocketConnection, Task>>
            {
                { "/ws", async (webSocket, connection) =>
                    {
                        await HandleWebSocketMessageAsync(webSocket, connection, messageHandler);
                    }
                }
            };
        }

        /// <summary>
        /// 检查指定路径是否有对应的路由处理器
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <returns>是否存在处理器</returns>
        public bool HasRoute(string path)
        {
            return _routeHandlers.ContainsKey(path);
        }

        /// <summary>
        /// 处理WebSocket连接
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <param name="webSocket">WebSocket实例</param>
        /// <param name="connection">WebSocket连接</param>
        /// <returns>处理任务</returns>
        public async Task HandleRouteAsync(string path, WebSocket webSocket, IWebSocketConnection connection)
        {
            if (_routeHandlers.TryGetValue(path, out var handler))
            {
                await handler(webSocket, connection);
            }
            else
            {
                throw new KeyNotFoundException($"未找到路径 {path} 的WebSocket处理器");
            }
        }

        /// <summary>
        /// 添加路由处理器
        /// </summary>
        /// <param name="path">请求路径</param>
        /// <param name="handler">处理函数</param>
        public void AddRoute(string path, Func<WebSocket, IWebSocketConnection, Task> handler)
        {
            _routeHandlers[path] = handler;
        }

        /// <summary>
        /// 处理 WebSocket 消息
        /// </summary>
        /// <param name="webSocket">WebSocket 实例</param>
        /// <param name="connection">WebSocket 连接</param>
        /// <param name="messageHandler">消息处理器</param>
        private async Task HandleWebSocketMessageAsync(WebSocket webSocket, IWebSocketConnection connection, IWebSocketMessageHandler messageHandler)
        {
            var buffer = new byte[1024 * 4]; // 创建接收缓冲区

            try
            {
                // 发送连接ID消息给客户端
                var connectionIdMessage = JsonSerializer.Serialize(new { type = "connection", id = connection.Id });
                await connection.SendAsync(connectionIdMessage);

                WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None); // 接收消息

                while (!result.CloseStatus.HasValue) // 处理接收到的消息
                {
                    await messageHandler.HandleMessageAsync(result, buffer, connection); // 处理消息

                    result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None); // 继续接收消息
                }

                await webSocket.CloseAsync(
                    result.CloseStatus.Value,
                    result.CloseStatusDescription,
                    CancellationToken.None); // 关闭 WebSocket 连接
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理 WebSocket 连接时出错，连接ID: {ConnectionId}", connection.Id); // 记录错误
            }
        }
    }


}