using Microsoft.IdentityModel.Tokens;
using std.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace std.Services
{
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expireMinutes;
        private readonly ConfigService _configService;
        private readonly ILogger<JwtService> _logger;

        public JwtService(ILogger<JwtService> logger, ConfigService configService)
        {
            _logger = logger;
            _configService = configService;
            Jwt jwt = _configService.GetJwtSetting();
            _secretKey = jwt.SecretKey;
            _issuer = jwt.Issuer;
            _audience = jwt.Audience;
            _expireMinutes = jwt.ExpireMinutes;
        }

        public string GenerateToken(string userId, string username)
        {
            try
            {
                // 1. 创建加密密钥
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                // 2. 组装Token内容
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, userId), // 用户唯一标识
                    new Claim(JwtRegisteredClaimNames.UniqueName, username), // 用户名
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // 防止重放攻击
                    new Claim("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()) // 自定义时间戳
                };

                // 3. 配置Token参数
                var tokenDescriptor = new JwtSecurityToken(
                    issuer: _issuer,    // 签发者（可自定义或留空）
                    audience: _audience,// 接收者（可自定义或留空）
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_expireMinutes), // 过期时间
                    signingCredentials: credentials
                );

                // 4. 生成Token字符串
                return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Token生成失败");
                throw;
            }
        }

        /// <summary>
        /// 验证Token（手动验证）
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="principal">ClaimsPrincipal</param>
        /// <returns>是否验证成功</returns>
        public bool ValidateToken(string token, out ClaimsPrincipal principal)
        {
            principal = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Token验证失败: {ex.Message}");
                return false;
            }
        }
    }
}
