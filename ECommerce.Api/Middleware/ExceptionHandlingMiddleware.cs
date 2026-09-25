using System.Text.Encodings.Web;
using System.Text.Json;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Results;
using FluentValidation;

namespace ECommerce.Api.Middleware;

/// <summary>全局异常处理中间件：统一把异常转换为 ApiResult 返回。</summary>
public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        int code;
        string message;
        int httpStatus;

        switch (exception)
        {
            case ValidationException validation:
                code = ResultCodes.ValidationError;
                message = string.Join("; ", validation.Errors.Select(e => e.ErrorMessage));
                httpStatus = StatusCodes.Status400BadRequest;
                break;

            case BusinessException business:
                code = business.Code;
                message = business.Message;
                httpStatus = MapToHttpStatus(business.Code);
                break;

            default:
                code = ResultCodes.ServerError;
                message = $"服务器内部错误：{exception.Message}";
                httpStatus = StatusCodes.Status500InternalServerError;
                _logger.LogError(exception, "未处理的全局异常");
                break;
        }

        context.Response.Clear();
        context.Response.StatusCode = httpStatus;
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = ApiResult.Fail(message, code);
        var json = JsonSerializer.Serialize(payload, JsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static int MapToHttpStatus(int businessCode)
    {
        return businessCode switch
        {
            ResultCodes.ValidationError => StatusCodes.Status400BadRequest,
            ResultCodes.Unauthorized => StatusCodes.Status401Unauthorized,
            ResultCodes.Forbidden => StatusCodes.Status403Forbidden,
            ResultCodes.NotFound => StatusCodes.Status404NotFound,
            ResultCodes.Conflict => StatusCodes.Status409Conflict,
            ResultCodes.ServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }
}