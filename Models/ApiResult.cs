using std.Api.Interfaces;

namespace std.Api.Models
{
    /// <summary>
    /// API 结果基类
    /// </summary>
    public class ApiResult : IApiResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }

        public ApiResult()
        {
            Success = true;
            Message = "操作成功";
            Code = 200;
        }

        public ApiResult(string message, bool success = false, int code = 400)
        {
            Success = success;
            Message = message;
            Code = code;
        }

        public static ApiResult Ok(string message = "操作成功")
        {
            return new ApiResult
            {
                Success = true,
                Message = message,
                Code = 200
            };
        }

        public static ApiResult Fail(string message = "操作失败", int code = 400)
        {
            return new ApiResult
            {
                Success = false,
                Message = message,
                Code = code
            };
        }
    }

    /// <summary>
    /// 带数据的 API 结果
    /// </summary>
    public class ApiResult<T> : ApiResult, IApiResult<T>
    {
        public T? Data { get; set; }

        public ApiResult() : base() { }

        public ApiResult(T data, string message = "操作成功") : base(message, true, 200)
        {
            Data = data;
        }

        public ApiResult(string message, bool success = false, int code = 400) : base(message, success, code) { }

        public static ApiResult<T> Ok(T data, string message = "操作成功")
        {
            return new ApiResult<T>
            {
                Success = true,
                Message = message,
                Code = 200,
                Data = data
            };
        }

        public new static ApiResult<T> Fail(string message = "操作失败", int code = 400)
        {
            return new ApiResult<T>
            {
                Success = false,
                Message = message,
                Code = code,
                Data = default
            };
        }
    }
} 