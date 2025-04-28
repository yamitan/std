using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using std.Data;
using std.Services;
using System.Security.Claims;

namespace std.Controllers
{
    public class UserController : BaseApiController<UserController>
    {
        public UserController(
            ILogger<UserController> logger,
            ConfigService config,
            JwtService jwt,
            AppDbContext db)
            : base(logger, config, jwt, db)
        { }

        [HttpGet("public")]
        public IActionResult PublicInfo()
        {
            return Success("这是公开信息，无需JWT验证");
        }

        [HttpGet("profile")]
        [Authorize]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;

            return Success(new
            {
                UserId = userId,
                Username = username,
                Time = DateTime.Now
            });
        }
    }
} 