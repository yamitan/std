using Microsoft.AspNetCore.Mvc;
using std.Api.Models;
using std.Data;
using std.Services;

namespace std.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseApiController<T>(
        ILogger<T> logger,
        ConfigService config,
        JwtService jwt,
        AppDbContext db) : ControllerBase where T : BaseApiController<T>
    {
        protected readonly ILogger<T> Logger = logger;
        protected readonly ConfigService Config = config;
        protected readonly JwtService Jwt = jwt;
        protected readonly AppDbContext Db = db;
        protected IActionResult Success(string message = "操作成功")
        {
            return Ok(ApiResult.Ok(message));
        }

        protected IActionResult Success<TData>(TData data, string message = "操作成功")
        {
            return Ok(ApiResult<TData>.Ok(data, message));
        }

        protected IActionResult Fail(string message = "操作失败", int code = 400)
        {
            return BadRequest(ApiResult.Fail(message, code));
        }
    }
}