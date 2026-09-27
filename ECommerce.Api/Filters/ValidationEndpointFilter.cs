using ECommerce.Shared.Results;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ECommerce.Api.Filters;

/// <summary>
///     自动校验端点过滤器（Minimal API 版 ValidationFilter）：在端点处理器执行前，
///     自动为已绑定的入参解析并执行对应的 FluentValidation 校验器。
///     校验失败时直接短路返回统一 ApiResult（HTTP 400），业务 Service 无需再手写 ValidateAndThrow。
///     注册为具名类型过滤器（无状态、由 DI 单例化），不会为每个请求产生闭包对象。
/// </summary>
public sealed class ValidationEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        foreach (var argument in context.Arguments)
        {
            if (argument is null)
                continue;

            // 按入参类型找 DI 中注册的 IValidator<T>（没有则跳过，不报错）。
            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

            if (result.IsValid)
                continue;

            // 把所有错误信息拼成一句话，包成统一返回结构（HTTP 400）。
            var message = string.Join("；", result.Errors.Select(e => e.ErrorMessage));
            return TypedResults.BadRequest(ApiResult.Fail(message, ResultCodes.ValidationError));
        }

        return await next(context);
    }
}
