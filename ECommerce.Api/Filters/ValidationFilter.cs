using ECommerce.Shared.Results;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.Api.Filters;

/// <summary>
///     自动校验过滤器：在 Action 方法执行前，自动为入参解析并执行对应的 FluentValidation 校验器。
///     校验失败时直接短路返回统一 ApiResult（HTTP 400），业务 Service 无需再手写 ValidateAndThrow。
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
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

            // 把所有错误信息拼成一句话，包成统一返回结构。
            var message = string.Join("；", result.Errors.Select(e => e.ErrorMessage));
            context.Result = new ObjectResult(ApiResult.Fail(message, ResultCodes.ValidationError))
            {
                StatusCode = StatusCodes.Status400BadRequest
            };
            return;
        }

        await next();
    }
}