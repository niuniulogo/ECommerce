using System.Security.Claims;
using ECommerce.Api.Filters;

namespace ECommerce.Api.Endpoints;

// ============================================================================
// 端点总注册：Program.cs 只调用 app.MapApiEndpoints()，
// 各业务模块的路由由各自的 *Endpoints 静态类负责声明。
// 约定：所有处理器都是具名静态方法，不写内联 Lambda，
//       避免每请求生成闭包对象，也便于直接对方法做单元测试。
// ============================================================================
public static class ApiEndpoints
{
    public const string ApiPrefix = "/api/v1";

    /// <summary>注册全部业务端点（统一挂在 /api/v1 前缀下）。</summary>
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        var api = app.MapGroup(ApiPrefix);

        ProductEndpoints.Map(api);
        AuthEndpoints.Map(api);
        SysUserEndpoints.Map(api);
        SysRoleEndpoints.Map(api);
        SysMenuEndpoints.Map(api);
        SysDeptEndpoints.Map(api);
        SysDictEndpoints.Map(api);
        SysNoticeEndpoints.Map(api);
        SysLogEndpoints.Map(api);
        SysConfigEndpoints.Map(api);
        UploadEndpoints.Map(api);
        SseEndpoints.Map(api);

        return app;
    }

    /// <summary>
    ///     创建一个业务模块路由组：统一打 Swagger 分组标签并挂上自动校验过滤器。
    /// </summary>
    /// <param name="routes">上级路由组（/api/v1）</param>
    /// <param name="prefix">模块路径前缀，如 "products"</param>
    /// <param name="tag">Swagger 分组标签</param>
    public static RouteGroupBuilder CreateModule(this IEndpointRouteBuilder routes, string prefix, string tag)
    {
        var group = routes.MapGroup(prefix);
        group.WithTags(tag);
        group.AddEndpointFilter<ValidationEndpointFilter>();
        return group;
    }
}

/// <summary>从当前登录态取用户标识等公共逻辑，替代 Controller 里的私有实例方法。</summary>
internal static class CurrentUser
{
    /// <summary>取当前登录用户 ID（JWT sub / NameIdentifier 声明）。</summary>
    public static long GetId(ClaimsPrincipal principal)
    {
        return long.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    /// <summary>取当前用户的全部角色编码。</summary>
    public static List<string> GetRoles(ClaimsPrincipal principal)
    {
        return principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }
}
