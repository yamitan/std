using System.Net.WebSockets;
using std.WebSockets.Core;
using std.WebSockets.Handlers;
using std.WebSockets.Managers;
using std.WebSockets.Services;

namespace std.WebSockets.Middleware
{
    /// <summary>
    /// WebSocket 服务器中间件，用于处理 WebSocket 连接
    /// </summary>
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next; // 下一个中间件
        private readonly ILogger<WebSocketServerMiddleware> _logger; // 日志记录器
        private readonly IWebSocketConnectionManager _connectionManager; // WebSocket 连接管理器
        private readonly IWebSocketRouteHandler _routeHandler; // WebSocket 路由处理器

        /// <summary>
        /// 构造WebSocket服务器中间件
        /// </summary>
        public WebSocketServerMiddleware(
            RequestDelegate next,
            ILogger<WebSocketServerMiddleware> logger,
            IWebSocketConnectionManager connectionManager,
            IWebSocketRouteHandler routeHandler)
        {
            _next = next;
            _logger = logger;
            _connectionManager = connectionManager;
            _routeHandler = routeHandler;
        }

        /// <summary>
        /// 处理 WebSocket 请求
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // 检查是否有对应路径的处理器
            if (_routeHandler.HasRoute(context.Request.Path))
            {
                if (context.WebSockets.IsWebSocketRequest) // 检查是否为 WebSocket 请求
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(); // 接受 WebSocket 请求
                    var connectionId = await _connectionManager.AddConnectionAsync(webSocket); // 添加连接
                    
                    // 获取连接对象
                    if (_connectionManager.TryGetConnection(connectionId, out var connection))
                    {
                        try
                        {
                            await _routeHandler.HandleRouteAsync(context.Request.Path, webSocket, connection); // 调用对应的处理器
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "处理WebSocket路由时出错: {Path}, 连接ID: {ConnectionId}", 
                                context.Request.Path, connectionId);
                        }
                        finally
                        {
                            await _connectionManager.RemoveConnectionAsync(connectionId); // 移除连接
                        }
                    }
                    else
                    {
                        _logger.LogError("无法获取WebSocket连接: {ConnectionId}", connectionId);
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest; // 非 WebSocket 请求返回 400
                }
            }
            else
            {
                await _next(context); // 调用下一个中间件
            }
        }
    }

    // 扩展方法
    public static class WebSocketServerMiddlewareExtensions
    {
        /// <summary>
        /// 注册WebSocket服务器中间件
        /// </summary>
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder app)
        {
            return app.UseMiddleware<WebSocketServerMiddleware>(); // 注册中间件
        }
        
        /// <summary>
        /// 注册WebSocket服务器所需的所有服务
        /// </summary>
        /// <param name="services">服务集合</param>
        /// <param name="configureOptions">WebSocket心跳配置选项</param>
        /// <returns>服务集合</returns>
        public static IServiceCollection AddWebSocketServer(
            this IServiceCollection services, 
            Action<WebSocketHeartbeatOptions> configureOptions = null)
        {
            // 注册WebSocket连接管理服务
            services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
            services.AddSingleton<IWebSocketGroupManager, WebSocketGroupManager>();
            services.AddSingleton<IWebSocketMessageHandler, WebSocketMessageHandler>();
            services.AddSingleton<IWebSocketRouteHandler, WebSocketRouteHandler>();
            
            // 配置WebSocket心跳服务
            if (configureOptions != null)
            {
                services.Configure(configureOptions);
            }
            else
            {
                services.Configure<WebSocketHeartbeatOptions>(options => {
                    options.HeartbeatInterval = TimeSpan.FromSeconds(30);
                    options.ConnectionTimeout = TimeSpan.FromMinutes(2);
                    options.EnableHeartbeat = true;
                    options.EnableTimeoutDisconnect = true;
                });
            }
            
            // 注册WebSocket心跳服务
            services.AddHostedService<WebSocketHeartbeatService>();
            
            return services;
        }
    }
}
