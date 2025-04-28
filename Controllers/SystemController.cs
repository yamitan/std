using Microsoft.AspNetCore.Mvc;
using std.Data;
using std.Services;

namespace std.Controllers
{
    public class SystemController : BaseApiController<SystemController>
    {
        public SystemController(
            ILogger<SystemController> logger,
            ConfigService config,
            JwtService jwt,
            AppDbContext db
            )
            : base(logger, config, jwt, db)
        {
        }

        [HttpGet("info")]
        public IActionResult GetSystemInfo()
        {
            var info = new
            {
                ServerTime = DateTime.Now,
                ServerName = Environment.MachineName,
                OsVersion = Environment.OSVersion.ToString(),
                FrameworkVersion = Environment.Version.ToString()
            };

            return Success(info);
        }

        [HttpGet("health")]
        public IActionResult CheckHealth()
        {
            return Success("系统运行正常");
        }

        //[HttpGet("db/info")]
        //public IActionResult GetDatabaseInfo()
        //{
        //    try
        //    {
        //        var dbInfo = Db.GetDatabaseInfo();
        //        return Success(dbInfo);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError(ex, "获取数据库信息失败");
        //        return Fail("获取数据库信息失败: " + ex.Message);
        //    }
        //}
    }
}