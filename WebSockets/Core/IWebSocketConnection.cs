using System.Net.WebSockets;

namespace std.WebSockets.Core
{
    /// <summary>
    /// 表示WebSocket连接的接口
    /// </summary>
    public interface IWebSocketConnection
    {
        /// <summary>
        /// 获取连接的唯一标识符
        /// </summary>
        string Id { get; }
        
        /// <summary>
        /// 获取连接状态
        /// </summary>
        WebSocketState State { get; }
        
        /// <summary>
        /// 获取连接建立时间
        /// </summary>
        DateTime ConnectedAt { get; }
        
        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        string RemoteIpAddress { get; }
        
        /// <summary>
        /// 获取上次活动时间
        /// </summary>
        DateTime LastActivityTime { get; }
        
        /// <summary>
        /// 获取用户数据字典，可用于存储连接相关的自定义数据
        /// </summary>
        IDictionary<string, object> Items { get; }
        
        /// <summary>
        /// 发送文本消息
        /// </summary>
        Task SendAsync(string message);
        
        /// <summary>
        /// 发送二进制数据
        /// </summary>
        Task SendAsync(ArraySegment<byte> binaryData);
        
        /// <summary>
        /// 发送对象（自动序列化为JSON）
        /// </summary>
        Task SendObjectAsync<T>(T obj);
        
        /// <summary>
        /// 关闭连接
        /// </summary>
        Task CloseAsync();
        
        /// <summary>
        /// 使用指定状态码和原因关闭连接
        /// </summary>
        Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription);
        
        /// <summary>
        /// 连接断开时触发的事件
        /// </summary>
        event EventHandler OnDisconnected;
        
        /// <summary>
        /// 收到消息时触发的事件
        /// </summary>
        event EventHandler<WebSocketMessageEventArgs> OnMessageReceived;
        
        /// <summary>
        /// 连接是否处于活动状态
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// 获取连接空闲时间
        /// </summary>
        TimeSpan IdleTime { get; }
    }
    
    /// <summary>
    /// WebSocket消息事件参数
    /// </summary>
    public class WebSocketMessageEventArgs : EventArgs
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public WebSocketMessageType MessageType { get; }
        
        /// <summary>
        /// 消息数据
        /// </summary>
        public byte[] Data { get; }
        
        /// <summary>
        /// 消息文本（如果为文本消息）
        /// </summary>
        public string Text { get; }
        
        public WebSocketMessageEventArgs(WebSocketMessageType messageType, byte[] data, string text = null)
        {
            MessageType = messageType;
            Data = data;
            Text = text;
        }
    }
}