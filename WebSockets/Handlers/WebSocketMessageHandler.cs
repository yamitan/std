using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using std.WebSockets.Core;

namespace std.WebSockets.Handlers
{
    /// <summary>
    /// 默认的 WebSocket 消息处理器
    /// </summary>
    public class WebSocketMessageHandler(ILogger<WebSocketMessageHandler> logger) : IWebSocketMessageHandler
    {
        private readonly ILogger<WebSocketMessageHandler> _logger = logger; // 日志记录器

        /// <summary>
        /// 处理接收到的 WebSocket 消息
        /// </summary>
        /// <param name="result">接收到的消息结果</param>
        /// <param name="buffer">接收缓冲区</param>
        /// <param name="connection">WebSocket 连接</param>
        public async Task HandleMessageAsync(WebSocketReceiveResult result, byte[] buffer, IWebSocketConnection connection)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count); // 将字节数组转换为字符串
            _logger.LogInformation("接收到消息: {Message}，连接ID: {ConnectionId}", message, connection.Id); // 记录接收到的消息

            // 处理消息逻辑...
            // 例如，您可以将消息广播到所有连接
            await Task.Run(() => { });
        }
    }
}