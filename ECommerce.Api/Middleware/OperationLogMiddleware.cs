using System.Diagnostics;
using System.Security.Claims;
using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;

namespace ECommerce.Api.Middleware;

/// <summary>
/// 全局操作日志中间件：记录每个已认证请求到 sys_log 表
/// 只记录写操作（POST/PUT/DELETE），GET 不记录
/// </summary>
public class OperationLogMiddleware
{
    private readonly RequestDelegate _next;

    public OperationLogMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISysLogRepository logRepo)
    {
        var method = context.Request.Method;

        // GET 请求不记录，未认证不记录
        if (HttpMethods.IsGet(method) || !context.User.Identity?.IsAuthenticated == true)
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = context.User.FindFirstValue(ClaimTypes.Name) ?? context.User.Identity?.Name;

        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            try
            {
                var log = new SysLog
                {
                    Title = DeriveTitle(context.Request.Path),
                    RequestUri = context.Request.Path,
                    RequestMethod = method,
                    Ip = context.Connection.RemoteIpAddress?.ToString(),
                    OperatorId = long.TryParse(userIdStr, out var uid) ? uid : null,
                    OperatorName = userName,
                    Status = context.Response.StatusCode < 400 ? 1 : 0,
                    ErrorMsg = context.Response.StatusCode >= 400 ? $"HTTP {context.Response.StatusCode}" : null,
                    ExecutionTime = (int)sw.ElapsedMilliseconds,
                    CreateTime = DateTime.UtcNow,
                };

                await logRepo.InsertAsync(log);
            }
            catch
            {
                // 日志写入失败不影响主流程
            }
        }
    }

    /// <summary>从路径推导操作标题，如 /api/v1/users → 用户管理</summary>
    private static string DeriveTitle(PathString path)
    {
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (segments.Length >= 3)
        {
            return segments[2] switch
            {
                "users" => "用户管理",
                "roles" => "角色管理",
                "depts" => "部门管理",
                "menus" => "菜单管理",
                "dicts" => "字典管理",
                "notices" => "通知公告",
                "configs" => "系统配置",
                "logs" => "系统日志",
                "auth" => "认证授权",
                _ => segments[2]
            };
        }
        return path.Value ?? "";
    }
}
