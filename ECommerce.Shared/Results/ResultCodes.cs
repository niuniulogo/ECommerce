namespace ECommerce.Shared.Results;

/// <summary>
///     全局业务状态码常量。
/// </summary>
public static class ResultCodes
{
    public const string Success = "00000";
    public const int Fail = 1;

    public const int ValidationError = 400;
    public const int Unauthorized = 401;
    public const int Forbidden = 403;
    public const int NotFound = 404;
    public const int Conflict = 409;
    public const int ServerError = 500;

    // Token 相关（与前端 ApiCodeEnum 对齐）
    public const string AccessTokenInvalid = "A0230";
    public const string RefreshTokenInvalid = "A0231";

    // 权限相关
    public const string PermissionDenied = "A0301";
}