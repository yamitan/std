using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using std.WebSockets.Core;

namespace std.WebSockets.Models
{
    /// <summary>
    /// 表示一个 WebSocket 连接的实现
    /// </summary>
    public class WebSocketConnection : IWebSocketConnection
    {
        private readonly CancellationTokenSource _cts = new();
        
        public WebSocketConnection(WebSocket socket, string remoteIpAddress = null)
        {
            Socket = socket; // 初始化 WebSocket 实例
            Id = Guid.NewGuid().ToString(); // 生成唯一连接 ID
            ConnectedAt = DateTime.UtcNow; // 记录连接时间
            LastActivityTime = ConnectedAt; // 初始化最后活动时间
            RemoteIpAddress = remoteIpAddress ?? "未知"; // 设置IP地址
            Items = new Dictionary<string, object>(); // 初始化用户数据字典
        }

        /// <summary>
        /// 获取连接的唯一标识符
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 获取 WebSocket 实例
        /// </summary>
        public WebSocket Socket { get; }

        /// <summary>
        /// 获取WebSocket连接状态
        /// </summary>
        public WebSocketState State => Socket.State;
        
        /// <summary>
        /// 获取连接建立时间
        /// </summary>
        public DateTime ConnectedAt { get; }
        
        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        public string RemoteIpAddress { get; }
        
        /// <summary>
        /// 获取上次活动时间
        /// </summary>
        public DateTime LastActivityTime { get; private set; }
        
        /// <summary>
        /// 获取用户数据字典
        /// </summary>
        public IDictionary<string, object> Items { get; }
        
        /// <summary>
        /// 连接断开时触发的事件
        /// </summary>
        public event EventHandler OnDisconnected;
        
        /// <summary>
        /// 收到消息时触发的事件
        /// </summary>
        public event EventHandler<WebSocketMessageEventArgs> OnMessageReceived;
        
        /// <summary>
        /// 连接是否处于活动状态
        /// </summary>
        public bool IsActive => State == WebSocketState.Open;
        
        /// <summary>
        /// 获取连接空闲时间
        /// </summary>
        public TimeSpan IdleTime => DateTime.UtcNow - LastActivityTime;

        /// <summary>
        /// 发送文本消息到 WebSocket 连接
        /// </summary>
        /// <param name="message">要发送的消息</param>
        public async Task SendAsync(string message)
        {
            UpdateLastActivity();
            var buffer = Encoding.UTF8.GetBytes(message); // 将消息编码为字节数组
            var segment = new ArraySegment<byte>(buffer); // 创建字节数组段
            await Socket.SendAsync(segment, WebSocketMessageType.Text, true, _cts.Token); // 发送消息
        }

        /// <summary>
        /// 发送二进制数据到 WebSocket 连接
        /// </summary>
        /// <param name="binaryData">要发送的二进制数据</param>
        public async Task SendAsync(ArraySegment<byte> binaryData)
        {
            UpdateLastActivity();
            await Socket.SendAsync(binaryData, WebSocketMessageType.Binary, true, _cts.Token);
        }
        
        /// <summary>
        /// 发送对象（自动序列化为JSON）
        /// </summary>
        public async Task SendObjectAsync<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj);
            await SendAsync(json);
        }

        /// <summary>
        /// 关闭 WebSocket 连接
        /// </summary>
        public async Task CloseAsync()
        {
            await CloseAsync(WebSocketCloseStatus.NormalClosure, "连接正常关闭");
        }
        
        /// <summary>
        /// 使用指定状态码和原因关闭连接
        /// </summary>
        public async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription)
        {
            if (Socket.State == WebSocketState.Open || Socket.State == WebSocketState.CloseReceived)
            {
                try
                {
                    await Socket.CloseAsync(closeStatus, statusDescription, _cts.Token);
                    OnDisconnected?.Invoke(this, EventArgs.Empty);
                }
                catch
                {
                    // 如果关闭过程中发生异常，强制取消所有操作
                    _cts.Cancel();
                    throw;
                }
            }
        }
        
        /// <summary>
        /// 启动消息接收循环，自动处理收到的消息并触发事件
        /// </summary>
        public async Task StartReceiveLoop()
        {
            var buffer = new byte[4096];
            var receiveBuffer = new ArraySegment<byte>(buffer);
            
            try
            {
                while (Socket.State == WebSocketState.Open && !_cts.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = await Socket.ReceiveAsync(receiveBuffer, _cts.Token);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await CloseAsync();
                        break;
                    }
                    
                    UpdateLastActivity();
                    
                    // 处理完整消息
                    byte[] messageData;
                    string text = null;
                    
                    if (result.Count < buffer.Length)
                    {
                        messageData = buffer.Take(result.Count).ToArray();
                        
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            text = Encoding.UTF8.GetString(messageData);
                        }
                        
                        OnMessageReceived?.Invoke(this, new WebSocketMessageEventArgs(
                            result.MessageType, messageData, text));
                    }
                    else
                    {
                        // 处理大型消息（超过缓冲区大小）
                        using var ms = new MemoryStream();
                        ms.Write(buffer, 0, result.Count);
                        
                        while (!result.EndOfMessage)
                        {
                            result = await Socket.ReceiveAsync(receiveBuffer, _cts.Token);
                            ms.Write(buffer, 0, result.Count);
                        }
                        
                        messageData = ms.ToArray();
                        
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            text = Encoding.UTF8.GetString(messageData);
                        }
                        
                        OnMessageReceived?.Invoke(this, new WebSocketMessageEventArgs(
                            result.MessageType, messageData, text));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
            catch (Exception)
            {
                // 处理接收循环中的异常
                await CloseAsync(WebSocketCloseStatus.InternalServerError, "服务器内部错误");
            }
            finally
            {
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }
        
        /// <summary>
        /// 更新最后活动时间
        /// </summary>
        private void UpdateLastActivity()
        {
            LastActivityTime = DateTime.UtcNow;
        }
    }
} 