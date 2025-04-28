using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using std.Data;
using std.Services;
using System.Data;
using static std.Models.Auth;

namespace std.Controllers
{

    public class AuthController : BaseApiController<AuthController>
    {


        public AuthController(
            ILogger<AuthController> logger,
            ConfigService config,
            JwtService jwt,
            AppDbContext db)
            : base(logger, config, jwt, db)
        { }

        [HttpPost("login")]
        public async Task<IActionResult> Login(AuthReq req)
        {
            try
            {
                // 使用EF Core查询用户
                var user = await Db.Users
                    .AsNoTracking()
                    .Where(u => u.Username == req.Username && u.Status == true)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Fail("用户不存在或已禁用");
                }

                // 验证密码
                if (user.Password != req.Password) // 实际应用中应使用加密比较
                {
                    return Fail("密码错误");
                }

                // 生成JWT令牌
                var token = Jwt.GenerateToken(user.UserId.ToString(), user.Username);

                // 返回登录成功结果
                return Success(new { token, user = new { user.UserId, user.Username } });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "登陆失败");
                return Fail("登陆失败: " + ex.Message);
            }
        }
    }
}
