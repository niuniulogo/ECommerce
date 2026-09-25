using ECommerce.Domain.Entities.System;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>建表（幂等，可重复执行）。</summary>
public class DbSeeder
{
    private readonly IFreeSql _fsql;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(IFreeSql fsql, ILogger<DbSeeder> logger)
    {
        _fsql = fsql;
        _logger = logger;
    }

    public void Seed()
    {
        // CodeFirst：自动创建 / 同步表结构。
        _fsql.CodeFirst.SyncStructure(
            typeof(SysUser), typeof(SysRole), typeof(SysMenu), typeof(SysDept),
            typeof(SysDict), typeof(SysDictItem), typeof(SysConfig), typeof(SysLog),
            typeof(SysNotice), typeof(SysUserNotice), typeof(SysUserRole),
            typeof(SysRoleMenu), typeof(SysRoleDept));
    }
}