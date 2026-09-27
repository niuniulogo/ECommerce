using System.Security.Claims;
using ECommerce.Application.Features.SystemManage;
using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Endpoints;

// ============================================================================
// 系统管理模块（RBAC + 基础数据）：全部需要登录。
// 每个静态类对应原一个 Controller，处理器全部为具名静态方法。
// ============================================================================

// ============ 用户管理 ============
public static class SysUserEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("users", "User").RequireAuthorization();

        group.MapGet("me", Me);                           // GET    /users/me
        group.MapGet("me/routes", MyRoutes);              // GET    /users/me/routes
        group.MapGet("", GetPage);                        // GET    /users
        group.MapGet("options", Options);                 // GET    /users/options
        group.MapGet("{id:long}/form", GetForm);          // GET    /users/{id}/form
        group.MapPost("", Create);                        // POST   /users
        group.MapPut("profile", UpdateProfile);           // PUT    /users/profile
        group.MapPut("password", ChangePassword);         // PUT    /users/password
        group.MapPut("{id:long}", Update);                // PUT    /users/{id}
        group.MapPut("{id:long}/password/reset", ResetPwd); // PUT  /users/{id}/password/reset
        group.MapDelete("{ids}", Delete);                 // DELETE /users/{ids}
    }

    /// <summary>当前登录用户信息（前端登录后调 /users/me）。</summary>
    private static async Task<ApiResult> Me(ClaimsPrincipal principal,
        [FromServices] SysAuthService auth, CancellationToken ct)
    {
        return ApiResult.Ok(await auth.GetUserInfoAsync(CurrentUser.GetId(principal), ct));
    }

    /// <summary>当前用户可访问的前端路由树。</summary>
    private static async Task<ApiResult> MyRoutes(ClaimsPrincipal principal,
        [FromServices] SysMenuService menus, CancellationToken ct)
    {
        var tree = await menus.GetRoutesAsync(
            CurrentUser.GetId(principal), CurrentUser.GetRoles(principal), ct);
        return ApiResult.Ok(tree);
    }

    private static async Task<ApiResult> GetPage([AsParameters] SysUserQuery q,
        [FromServices] SysUserService svc, CancellationToken ct)
    {
        var (list, total) = await svc.GetPageAsync(q, ct);
        return ApiResult.Ok(new { list, total });
    }

    private static async Task<ApiResult> GetForm(long id,
        [FromServices] SysUserService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetFormAsync(id, ct));
    }

    private static async Task<ApiResult> Create([FromBody] SysUserDto dto,
        [FromServices] SysUserService svc, CancellationToken ct)
    {
        await svc.CreateAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    private static async Task<ApiResult> Update(long id, [FromBody] SysUserDto dto,
        [FromServices] SysUserService svc, CancellationToken ct)
    {
        await svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    private static async Task<ApiResult> Delete(string ids,
        [FromServices] SysUserService svc, CancellationToken ct)
    {
        await svc.DeleteAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    private static async Task<ApiResult> ResetPwd(long id, [FromBody] SysResetPasswordDto dto,
        [FromServices] SysUserService svc, CancellationToken ct)
    {
        await svc.ResetPasswordAsync(id, dto.Password, ct);
        return ApiResult.Ok("重置成功");
    }

    private static async Task<ApiResult> Options([FromServices] SysUserService svc,
        CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetOptionsAsync(ct));
    }

    /// <summary>修改个人资料。</summary>
    private static async Task<ApiResult> UpdateProfile([FromBody] SysProfileUpdateDto dto,
        ClaimsPrincipal principal, [FromServices] SysAuthService auth, CancellationToken ct)
    {
        await auth.UpdateProfileAsync(CurrentUser.GetId(principal), dto, ct);
        return ApiResult.Ok("更新成功");
    }

    /// <summary>修改自己的密码。</summary>
    private static async Task<ApiResult> ChangePassword([FromBody] SysPasswordChangeDto dto,
        ClaimsPrincipal principal, [FromServices] SysAuthService auth, CancellationToken ct)
    {
        await auth.ChangePasswordAsync(CurrentUser.GetId(principal), dto, ct);
        return ApiResult.Ok("修改成功");
    }
}

// ============ 角色管理 ============
public static class SysRoleEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("roles", "Role").RequireAuthorization();

        group.MapGet("", GetPage);                        // GET    /roles
        group.MapGet("options", Options);                 // GET    /roles/options
        group.MapGet("{id:long}/form", GetForm);          // GET    /roles/{id}/form
        group.MapGet("{id:long}/menu-ids", GetMenuIds);   // GET    /roles/{id}/menu-ids
        group.MapPost("", Create);                        // POST   /roles
        group.MapPut("{id:long}", Update);                // PUT    /roles/{id}
        group.MapPut("{id:long}/menus", SetMenus);        // PUT    /roles/{id}/menus
        group.MapDelete("{ids}", Delete);                 // DELETE /roles/{ids}
    }

    private static async Task<ApiResult> GetPage(
        [FromServices] SysRoleService svc,
        [FromQuery] string? keywords = null,
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await svc.GetPageAsync(keywords, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    private static async Task<ApiResult> Create([FromBody] SysRoleFormDto dto,
        [FromServices] SysRoleService svc, CancellationToken ct)
    {
        await svc.CreateAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    private static async Task<ApiResult> GetForm(long id,
        [FromServices] SysRoleService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetFormAsync(id, ct));
    }

    private static async Task<ApiResult> Update(long id, [FromBody] SysRoleFormDto dto,
        [FromServices] SysRoleService svc, CancellationToken ct)
    {
        await svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    private static async Task<ApiResult> Delete(string ids,
        [FromServices] SysRoleService svc, CancellationToken ct)
    {
        await svc.DeleteAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    /// <summary>角色已分配的菜单 ID 列表。</summary>
    private static async Task<ApiResult> GetMenuIds(long id,
        [FromServices] SysRoleService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetMenuIdsAsync(id, ct));
    }

    private static async Task<ApiResult> SetMenus(long id, [FromBody] List<long> menuIds,
        [FromServices] SysRoleService svc, CancellationToken ct)
    {
        await svc.SetMenusAsync(id, menuIds, ct);
        return ApiResult.Ok("分配成功");
    }

    private static async Task<ApiResult> Options([FromServices] SysRoleService svc,
        CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetOptionsAsync(ct));
    }
}

// ============ 菜单管理 ============
public static class SysMenuEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("menus", "Menu").RequireAuthorization();

        group.MapGet("", GetTree);                        // GET    /menus
        group.MapGet("routes", Routes);                   // GET    /menus/routes
        group.MapGet("options", Options);                 // GET    /menus/options
        group.MapGet("{id:long}/form", GetForm);          // GET    /menus/{id}/form
        group.MapPost("", Create);                        // POST   /menus
        group.MapPut("{id:long}", Update);                // PUT    /menus/{id}
        group.MapDelete("{id:long}", Delete);             // DELETE /menus/{id}
    }

    /// <summary>当前用户可访问的前端路由树。</summary>
    private static async Task<ApiResult> Routes(ClaimsPrincipal principal,
        [FromServices] SysMenuService svc, CancellationToken ct)
    {
        var tree = await svc.GetRoutesAsync(
            CurrentUser.GetId(principal), CurrentUser.GetRoles(principal), ct);
        return ApiResult.Ok(tree);
    }

    /// <summary>菜单下拉选项树。</summary>
    private static async Task<ApiResult> Options([FromServices] SysMenuService svc,
        CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetOptionsAsync(ct));
    }

    private static async Task<ApiResult> GetTree([FromQuery] string? keywords,
        [FromServices] SysMenuService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetTreeAsync(keywords, ct));
    }

    private static async Task<ApiResult> GetForm(long id,
        [FromServices] SysMenuService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetFormAsync(id, ct));
    }

    private static async Task<ApiResult> Create([FromBody] SysMenuFormDto dto,
        [FromServices] SysMenuService svc, CancellationToken ct)
    {
        await svc.CreateAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    private static async Task<ApiResult> Update(long id, [FromBody] SysMenuFormDto dto,
        [FromServices] SysMenuService svc, CancellationToken ct)
    {
        await svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    private static async Task<ApiResult> Delete(long id,
        [FromServices] SysMenuService svc, CancellationToken ct)
    {
        await svc.DeleteAsync(id, ct);
        return ApiResult.Ok("删除成功");
    }
}

// ============ 部门管理 ============
public static class SysDeptEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("depts", "Dept").RequireAuthorization();

        group.MapGet("", GetTree);                        // GET    /depts
        group.MapGet("options", Options);                 // GET    /depts/options
        group.MapGet("{id:long}/form", GetForm);          // GET    /depts/{id}/form
        group.MapPost("", Create);                        // POST   /depts
        group.MapPut("{id:long}", Update);                // PUT    /depts/{id}
        group.MapDelete("{ids}", Delete);                 // DELETE /depts/{ids}
    }

    private static async Task<ApiResult> GetTree([FromQuery] string? keywords,
        [FromQuery] int? status, [FromServices] SysDeptService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetTreeAsync(keywords, status, ct));
    }

    private static async Task<ApiResult> GetForm(long id,
        [FromServices] SysDeptService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetFormAsync(id, ct));
    }

    private static async Task<ApiResult> Create([FromBody] SysDeptFormDto dto,
        [FromServices] SysDeptService svc, CancellationToken ct)
    {
        await svc.CreateAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    private static async Task<ApiResult> Update(long id, [FromBody] SysDeptFormDto dto,
        [FromServices] SysDeptService svc, CancellationToken ct)
    {
        await svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    private static async Task<ApiResult> Delete(string ids,
        [FromServices] SysDeptService svc, CancellationToken ct)
    {
        await svc.DeleteAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    private static async Task<ApiResult> Options([FromServices] SysDeptService svc,
        CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetOptionsAsync(ct));
    }
}

// ============ 字典管理 ============
public static class SysDictEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("dicts", "Dict").RequireAuthorization();

        group.MapGet("", GetTypePage);                          // GET    /dicts
        group.MapGet("{id:long}/form", GetTypeForm);            // GET    /dicts/{id}/form
        group.MapPost("", CreateType);                          // POST   /dicts
        group.MapPut("{id:long}", UpdateType);                  // PUT    /dicts/{id}
        group.MapDelete("{ids}", DeleteType);                   // DELETE /dicts/{ids}

        group.MapGet("{dictCode}/items", GetItems);             // GET    /dicts/{code}/items
        group.MapGet("{dictCode}/items/options", ItemOptions);  // GET    /dicts/{code}/items/options
        group.MapGet("{dictCode}/items/{id:long}/form", GetItemForm); // GET /dicts/{code}/items/{id}/form
        group.MapPost("{dictCode}/items", CreateItem);          // POST   /dicts/{code}/items
        group.MapPut("{dictCode}/items/{id:long}", UpdateItem); // PUT    /dicts/{code}/items/{id}
        group.MapDelete("{dictCode}/items/{ids}", DeleteItem);  // DELETE /dicts/{code}/items/{ids}
    }

    private static async Task<ApiResult> GetTypePage(
        [FromServices] SysDictService svc,
        [FromQuery] string? keywords = null,
        [FromQuery] int? status = null,
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await svc.GetTypePageAsync(keywords, status, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    private static async Task<ApiResult> CreateType([FromBody] SysDictTypeFormDto dto,
        [FromServices] SysDictService svc, CancellationToken ct)
    {
        await svc.CreateTypeAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    private static async Task<ApiResult> UpdateType(long id, [FromBody] SysDictTypeFormDto dto,
        [FromServices] SysDictService svc, CancellationToken ct)
    {
        await svc.UpdateTypeAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    private static async Task<ApiResult> DeleteType(string ids,
        [FromServices] SysDictService svc, CancellationToken ct)
    {
        await svc.DeleteTypeAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    private static async Task<ApiResult> GetTypeForm(long id,
        [FromServices] SysDictService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetTypeFormAsync(id, ct));
    }

    private static async Task<ApiResult> GetItems(string dictCode,
        [FromServices] SysDictService svc,
        [FromQuery] string? keywords = null,
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await svc.GetItemPageAsync(dictCode, keywords, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    private static async Task<ApiResult> CreateItem(string dictCode, [FromBody] SysDictItemFormDto dto,
        [FromServices] SysDictService svc, CancellationToken ct)
    {
        await svc.CreateItemAsync(dictCode, dto, ct);
        return ApiResult.Ok("创建成功");
    }

    private static async Task<ApiResult> UpdateItem(string dictCode, long id,
        [FromBody] SysDictItemFormDto dto, [FromServices] SysDictService svc, CancellationToken ct)
    {
        await svc.UpdateItemAsync(dictCode, id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    private static async Task<ApiResult> DeleteItem(string dictCode, string ids,
        [FromServices] SysDictService svc, CancellationToken ct)
    {
        await svc.DeleteItemAsync(dictCode, ids, ct);
        return ApiResult.Ok("删除成功");
    }

    /// <summary>字典项下拉选项。</summary>
    private static async Task<ApiResult> ItemOptions(string dictCode,
        [FromServices] SysDictService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetItemOptionsAsync(dictCode, ct));
    }

    private static async Task<ApiResult> GetItemForm(long id,
        [FromServices] SysDictService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetItemFormAsync(id, ct));
    }
}

// ============ 通知公告 ============
public static class SysNoticeEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("notices", "Notice").RequireAuthorization();

        group.MapGet("", GetPage);                        // GET    /notices
        group.MapGet("my", GetMy);                        // GET    /notices/my
        group.MapGet("{id:long}/form", GetForm);          // GET    /notices/{id}/form
        group.MapGet("{id:long}/detail", GetDetail);      // GET    /notices/{id}/detail
        group.MapPost("", Create);                        // POST   /notices
        group.MapPut("read-all", ReadAll);                // PUT    /notices/read-all
        group.MapPut("{id:long}", Update);                // PUT    /notices/{id}
        group.MapPut("{id:long}/publish", Publish);       // PUT    /notices/{id}/publish
        group.MapPut("{id:long}/revoke", Revoke);         // PUT    /notices/{id}/revoke
        group.MapDelete("{ids}", Delete);                 // DELETE /notices/{ids}
    }

    private static async Task<ApiResult> GetPage(
        [FromServices] SysNoticeService svc,
        [FromQuery] string? title = null,
        [FromQuery] int? publishStatus = null,
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await svc.GetPageAsync(title, publishStatus, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    private static async Task<ApiResult> Create([FromBody] SysNoticeFormDto dto,
        ClaimsPrincipal principal, [FromServices] SysNoticeService svc, CancellationToken ct)
    {
        await svc.CreateAsync(dto, CurrentUser.GetId(principal), ct);
        return ApiResult.Ok("创建成功");
    }

    private static async Task<ApiResult> Update(long id, [FromBody] SysNoticeFormDto dto,
        [FromServices] SysNoticeService svc, CancellationToken ct)
    {
        await svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    private static async Task<ApiResult> Delete(string ids,
        [FromServices] SysNoticeService svc, CancellationToken ct)
    {
        await svc.DeleteAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    /// <summary>发布。</summary>
    private static async Task<ApiResult> Publish(long id, ClaimsPrincipal principal,
        [FromServices] SysNoticeService svc, CancellationToken ct)
    {
        await svc.PublishAsync(id, CurrentUser.GetId(principal), ct);
        return ApiResult.Ok("发布成功");
    }

    /// <summary>撤回。</summary>
    private static async Task<ApiResult> Revoke(long id,
        [FromServices] SysNoticeService svc, CancellationToken ct)
    {
        await svc.RevokeAsync(id, ct);
        return ApiResult.Ok("撤回成功");
    }

    /// <summary>编辑回填。</summary>
    private static async Task<ApiResult> GetForm(long id,
        [FromServices] SysNoticeService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetFormAsync(id, ct));
    }

    /// <summary>通知详情。</summary>
    private static async Task<ApiResult> GetDetail(long id, ClaimsPrincipal principal,
        [FromServices] SysNoticeService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetDetailAsync(id, CurrentUser.GetId(principal), ct));
    }

    /// <summary>我的通知。</summary>
    private static async Task<ApiResult> GetMy(
        ClaimsPrincipal principal,
        [FromServices] SysNoticeService svc,
        [FromQuery] string? title = null,
        [FromQuery] int? isRead = null,
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await svc.GetMyPageAsync(
            CurrentUser.GetId(principal), title, isRead, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    /// <summary>全部已读。</summary>
    private static async Task<ApiResult> ReadAll(ClaimsPrincipal principal,
        [FromServices] SysNoticeService svc, CancellationToken ct)
    {
        await svc.MarkAllReadAsync(CurrentUser.GetId(principal), ct);
        return ApiResult.Ok("全部已读");
    }
}

// ============ 日志 ============
public static class SysLogEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("logs", "Log").RequireAuthorization();

        group.MapGet("", GetPage);                        // GET /logs
        group.MapGet("analytics/overview", Overview);    // GET /logs/analytics/overview
        group.MapGet("analytics/trend", Trend);          // GET /logs/analytics/trend
    }

    private static async Task<ApiResult> GetPage(
        [FromServices] SysLogService svc,
        [FromQuery] string? keywords = null,
        [FromQuery] string[]? createTime = null,
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await svc.GetPageAsync(keywords, createTime, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    private static async Task<ApiResult> Overview([FromServices] SysLogService svc,
        CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetOverviewAsync(ct));
    }

    private static async Task<ApiResult> Trend([FromQuery] string? startDate,
        [FromQuery] string? endDate, [FromServices] SysLogService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetTrendAsync(
            DateTime.TryParse(startDate, out var s) ? s : null,
            DateTime.TryParse(endDate, out var e) ? e : null, ct));
    }
}

// ============ 系统配置 ============
public static class SysConfigEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("configs", "Config").RequireAuthorization();

        group.MapGet("", GetPage);                        // GET    /configs
        group.MapGet("{id:long}/form", GetForm);          // GET    /configs/{id}/form
        group.MapPost("", Create);                        // POST   /configs
        group.MapPut("refresh", Refresh);                 // PUT    /configs/refresh
        group.MapPut("{id:long}", Update);                // PUT    /configs/{id}
    }

    private static async Task<ApiResult> GetPage(
        [FromServices] SysConfigService svc,
        [FromQuery] string? keywords = null,
        [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, total) = await svc.GetPageAsync(keywords, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    private static async Task<ApiResult> Create([FromBody] SysConfigFormDto dto,
        ClaimsPrincipal principal, [FromServices] SysConfigService svc, CancellationToken ct)
    {
        await svc.CreateAsync(dto, CurrentUser.GetId(principal), ct);
        return ApiResult.Ok("创建成功");
    }

    private static async Task<ApiResult> Update(long id, [FromBody] SysConfigFormDto dto,
        ClaimsPrincipal principal, [FromServices] SysConfigService svc, CancellationToken ct)
    {
        await svc.UpdateAsync(id, dto, CurrentUser.GetId(principal), ct);
        return ApiResult.Ok("更新成功");
    }

    private static async Task<ApiResult> GetForm(long id,
        [FromServices] SysConfigService svc, CancellationToken ct)
    {
        return ApiResult.Ok(await svc.GetFormAsync(id, ct));
    }

    /// <summary>刷新配置缓存（无缓存，空实现）。</summary>
    private static async Task<ApiResult> Refresh([FromServices] SysConfigService svc,
        CancellationToken ct)
    {
        await svc.RefreshAsync(ct);
        return ApiResult.Ok("刷新成功");
    }
}
