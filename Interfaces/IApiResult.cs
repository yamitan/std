namespace std.Api.Interfaces
{
    /// <summary>   
    /// API 结果接口
    /// </summary>
    public interface IApiResult
    {
        bool Success { get; }
        string Message { get; }
        int Code { get; }
    }

    /// <summary>
    /// 带数据的 API 结果接口
    /// </summary>
    public interface IApiResult<T> : IApiResult
    {
        T? Data { get; }
    }
}