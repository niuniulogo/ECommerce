namespace ECommerce.Shared.Results;



/// <summary>
///     全局统一返回模型（带数据载荷）。
/// </summary>
public class ApiResult
{
    public object? Data { get; set; }
    
    public required string Code { get; set; }
    
    public bool Success { get; set; }
    
    public string? Msg { get; set; }
    

    public static ApiResult Ok(object? data, string? message = "ok")
    {
        return new ApiResult { Code = ResultCodes.Success, Success = true, Msg = message, Data = data };
    }



    public static ApiResult Fail(string? message = "fail", int code = ResultCodes.Fail)
    {
        return new ApiResult { Code = code.ToString(), Success = false, Msg = message, Data = default };
    }
}