using System.Reflection;
using ECommerce.Application.Features.Product;
using ECommerce.Application.Features.SystemManage;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application;

public static class DependencyInjection
{
    /// <summary>注册应用层服务：业务用例服务与 FluentValidation 校验器。</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // 自动注册程序集中所有 AbstractValidator<T>。
        services.AddValidatorsFromAssembly(assembly);

        // ---- 系统管理（RBAC）模块 ----
        services.AddMemoryCache(); // 验证码答案缓存（幂等，与 Infrastructure 重复注册无害）
        services.AddSingleton<SysJwtService>();
        services.AddSingleton<ISseConnectionManager, SseConnectionManager>();
        services.AddScoped<SysCaptchaService>();
        services.AddScoped<SysAuthService>();
        services.AddScoped<SysUserService>();
        services.AddScoped<SysRoleService>();
        services.AddScoped<SysMenuService>();
        services.AddScoped<SysDeptService>();
        services.AddScoped<SysDictService>();
        services.AddScoped<SysNoticeService>();
        services.AddScoped<SysLogService>();
        services.AddScoped<SysConfigService>();

        // ---- 商品模块 ----
        services.AddScoped<ProductService>();

        return services;
    }
}