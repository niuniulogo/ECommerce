using ECommerce.Domain.Repositories.Product;
using ECommerce.Domain.Repositories.System;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Persistence.Repositories.Product;
using ECommerce.Infrastructure.Persistence.Repositories.System;
using FreeSql;
using FreeSql.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    /// <summary>注册基础设施层：FreeSql、仓储、数据库初始化器。</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=mesmini";

        var fsql = new FreeSqlBuilder()
            .UseConnectionString(DataType.PostgreSQL, connectionString)
            .UseAutoSyncStructure(true)
            .UseNameConvert(NameConvertType.PascalCaseToUnderscore)
            .UseQuoteSqlName(false)
            .Build();

        services.AddSingleton<IFreeSql>(fsql);

        // ---- 系统管理（RBAC）仓储 ----
        services.AddScoped<ISysUserRepository, SysUserRepository>();
        services.AddScoped<ISysRoleRepository, SysRoleRepository>();
        services.AddScoped<ISysMenuRepository, SysMenuRepository>();
        services.AddScoped<ISysDeptRepository, SysDeptRepository>();
        services.AddScoped<ISysDictRepository, SysDictRepository>();
        services.AddScoped<ISysDictItemRepository, SysDictItemRepository>();
        services.AddScoped<ISysConfigRepository, SysConfigRepository>();
        services.AddScoped<ISysLogRepository, SysLogRepository>();
        services.AddScoped<ISysNoticeRepository, SysNoticeRepository>();
        services.AddScoped<ISysUserRoleRepository, SysUserRoleRepository>();
        services.AddScoped<ISysRoleMenuRepository, SysRoleMenuRepository>();
        services.AddScoped<ISysRoleDeptRepository, SysRoleDeptRepository>();

        // ---- 商品模块仓储 ----
        services.AddScoped<IProductRepository,ProductRepository>();

        services.AddScoped<DbSeeder>();
        return services;
    }

    /// <summary>应用启动时执行建表。</summary>
    public static IApplicationBuilder UseDatabaseInitializer(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
        seeder.Seed();
        return app;
    }
}