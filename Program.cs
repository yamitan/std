using Microsoft.EntityFrameworkCore;
using std.Data;
using std.Services;
using std.WebSockets.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 添加 控制器
builder.Services.AddControllers();
// 添加 Swagger/OpenAPI 服务
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 CORS 服务
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 注册日志服务
builder.Services.AddLogging();

// 注册配置服务
builder.Services.AddSingleton<ConfigService>();

// 注册WebSocket服务
builder.Services.AddWebSocketServer(options =>
{
    options.HeartbeatInterval = TimeSpan.FromSeconds(60);
    options.ConnectionTimeout = TimeSpan.FromMinutes(2);
});

// 注册定时服务
builder.Services.AddHostedService<TimerService>();

//注册jwt服务
builder.Services.AddSingleton<JwtService>();

// 添加JWT身份验证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<std.Models.Jwt>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// 注册DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["Database:ConnectionString"]);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 启用 CORS
app.UseCors("AllowAll");

// 启用WebSocket
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
});

// 注册WebSocket服务器中间件
app.UseWebSocketServer();

// websocket测试页面 https://localhost:port/websocket.html
// api测试页面 https://localhost:port/api.html

// 启用静态文件
app.UseStaticFiles();

// 启用认证和授权
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
