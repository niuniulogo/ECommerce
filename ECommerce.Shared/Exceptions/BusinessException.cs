using ECommerce.Shared.Results;

namespace ECommerce.Shared.Exceptions;

/// <summary>
///     业务异常：由业务规则主动抛出，由全局异常中间件统一转换为 ApiResult。
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message, int code = ResultCodes.Fail) : base(message)
    {
        Code = code;
    }

    public int Code { get; }

    public static BusinessException NotFound(string resource, object key)
    {
        return new BusinessException($"{resource}（{key}）不存在", ResultCodes.NotFound);
    }

    public static BusinessException Conflict(string message)
    {
        return new BusinessException(message, ResultCodes.Conflict);
    }
}