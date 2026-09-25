using System.Security.Claims;
using ECommerce.Application.Features.SystemManage;
using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

// ============ 用户管理 ============
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class SysUserController : ControllerBase
{
    private readonly SysAuthService _auth;
    private readonly SysMenuService _menus;
    private readonly SysUserService _svc;

    public SysUserController(SysUserService svc, SysAuthService auth, SysMenuService menus)
    {
        _svc = svc;
        _auth = auth;
        _menus = menus;
    }

    /// <summary>当前登录用户信息（前端登录后调 /users/me）。</summary>
    [HttpGet("me")]
    public async Task<ApiResult> Me(CancellationToken ct)
    {
        return ApiResult.Ok(await _auth.GetUserInfoAsync(CurrentUserId(), ct));
    }

    /// <summary>当前用户可访问的前端路由树（前端调 /menus/routes）。</summary>
    [HttpGet("me/routes")]
    public async Task<ApiResult> MyRoutes(CancellationToken ct)
    {
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var tree = await _menus.GetRoutesAsync(CurrentUserId(), roles, ct);
        return ApiResult.Ok(tree);
    }

    [HttpGet]
    public async Task<ApiResult> GetPage([FromQuery] SysUserQuery q, CancellationToken ct)
    {
        var (list, total) = await _svc.GetPageAsync(q, ct);
        return ApiResult.Ok(new { list, total });
    }

    [HttpGet("{id:long}/form")]
    public async Task<ApiResult> GetForm(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetFormAsync(id, ct));
    }

    [HttpPost]
    public async Task<ApiResult> Create([FromBody] SysUserDto dto, CancellationToken ct)
    {
        await _svc.CreateAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] SysUserDto dto, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    [HttpDelete("{ids}")]
    public async Task<ApiResult> Delete(string ids, CancellationToken ct)
    {
        await _svc.DeleteAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    [HttpPut("{id:long}/password/reset")]
    public async Task<ApiResult> ResetPwd(long id, [FromBody] SysResetPasswordDto dto, CancellationToken ct)
    {
        await _svc.ResetPasswordAsync(id, dto.Password, ct);
        return ApiResult.Ok("重置成功");
    }

    [HttpGet("options")]
    public async Task<ApiResult> Options(CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetOptionsAsync(ct));
    }

    /// <summary>修改个人资料。</summary>
    [HttpPut("profile")]
    public async Task<ApiResult> UpdateProfile([FromBody] SysProfileUpdateDto dto, CancellationToken ct)
    {
        await _auth.UpdateProfileAsync(CurrentUserId(), dto, ct);
        return ApiResult.Ok("更新成功");
    }

    /// <summary>修改自己的密码。</summary>
    [HttpPut("password")]
    public async Task<ApiResult> ChangePassword([FromBody] SysPasswordChangeDto dto, CancellationToken ct)
    {
        await _auth.ChangePasswordAsync(CurrentUserId(), dto, ct);
        return ApiResult.Ok("修改成功");
    }

    private long CurrentUserId()
    {
        return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}

// ============ 角色管理 ============
[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class SysRoleController : ControllerBase
{
    private readonly SysRoleService _svc;

    public SysRoleController(SysRoleService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<ApiResult> GetPage([FromQuery] string? keywords, [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetPageAsync(keywords, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    [HttpPost]
    public async Task<ApiResult> Create([FromBody] SysRoleFormDto dto, CancellationToken ct)
    {
        await _svc.CreateAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    [HttpGet("{id:long}/form")]
    public async Task<ApiResult> GetForm(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetFormAsync(id, ct));
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] SysRoleFormDto dto, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    [HttpDelete("{ids}")]
    public async Task<ApiResult> Delete(string ids, CancellationToken ct)
    {
        await _svc.DeleteAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    /// <summary>角色已分配的菜单 ID 列表（前端调 /roles/{id}/menu-ids）。</summary>
    [HttpGet("{id:long}/menu-ids")]
    public async Task<ApiResult> GetMenuIds(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetMenuIdsAsync(id, ct));
    }

    [HttpPut("{id:long}/menus")]
    public async Task<ApiResult> SetMenus(long id, [FromBody] List<long> menuIds, CancellationToken ct)
    {
        await _svc.SetMenusAsync(id, menuIds, ct);
        return ApiResult.Ok("分配成功");
    }

    [HttpGet("options")]
    public async Task<ApiResult> Options(CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetOptionsAsync(ct));
    }
}

// ============ 菜单管理 ============
[ApiController]
[Route("api/v1/menus")]
[Authorize]
public class SysMenuController : ControllerBase
{
    private readonly SysMenuService _svc;

    public SysMenuController(SysMenuService svc)
    {
        _svc = svc;
    }

    /// <summary>当前用户可访问的前端路由树（前端调 /menus/routes）。</summary>
    [HttpGet("routes")]
    public async Task<ApiResult> Routes(CancellationToken ct)
    {
        var uid = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var tree = await _svc.GetRoutesAsync(uid, roles, ct);
        return ApiResult.Ok(tree);
    }

    /// <summary>菜单下拉选项树（前端调 /menus/options）。</summary>
    [HttpGet("options")]
    public async Task<ApiResult> Options(CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetOptionsAsync(ct));
    }

    [HttpGet]
    public async Task<ApiResult> GetTree([FromQuery] string? keywords, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetTreeAsync(keywords, ct));
    }

    [HttpGet("{id:long}/form")]
    public async Task<ApiResult> GetForm(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetFormAsync(id, ct));
    }

    [HttpPost]
    public async Task<ApiResult> Create([FromBody] SysMenuFormDto dto, CancellationToken ct)
    {
        await _svc.CreateAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] SysMenuFormDto dto, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    [HttpDelete("{id:long}")]
    public async Task<ApiResult> Delete(long id, CancellationToken ct)
    {
        await _svc.DeleteAsync(id, ct);
        return ApiResult.Ok("删除成功");
    }
}

// ============ 部门管理 ============
[ApiController]
[Route("api/v1/depts")]
[Authorize]
public class SysDeptController : ControllerBase
{
    private readonly SysDeptService _svc;

    public SysDeptController(SysDeptService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<ApiResult> GetTree([FromQuery] string? keywords, [FromQuery] int? status,
        CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetTreeAsync(keywords, status, ct));
    }

    [HttpGet("{id:long}/form")]
    public async Task<ApiResult> GetForm(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetFormAsync(id, ct));
    }

    [HttpPost]
    public async Task<ApiResult> Create([FromBody] SysDeptFormDto dto, CancellationToken ct)
    {
        await _svc.CreateAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] SysDeptFormDto dto, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    [HttpDelete("{ids}")]
    public async Task<ApiResult> Delete(string ids, CancellationToken ct)
    {
        await _svc.DeleteAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    [HttpGet("options")]
    public async Task<ApiResult> Options(CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetOptionsAsync(ct));
    }
}

// ============ 字典管理 ============
[ApiController]
[Route("api/v1/dicts")]
[Authorize]
public class SysDictController : ControllerBase
{
    private readonly SysDictService _svc;

    public SysDictController(SysDictService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<ApiResult> GetTypePage([FromQuery] string? keywords, [FromQuery] int? status,
        [FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetTypePageAsync(keywords, status, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    [HttpPost]
    public async Task<ApiResult> CreateType([FromBody] SysDictTypeFormDto dto, CancellationToken ct)
    {
        await _svc.CreateTypeAsync(dto, ct);
        return ApiResult.Ok("创建成功");
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> UpdateType(long id, [FromBody] SysDictTypeFormDto dto, CancellationToken ct)
    {
        await _svc.UpdateTypeAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    [HttpDelete("{ids}")]
    public async Task<ApiResult> DeleteType(string ids, CancellationToken ct)
    {
        await _svc.DeleteTypeAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    [HttpGet("{id:long}/form")]
    public async Task<ApiResult> GetTypeForm(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetTypeFormAsync(id, ct));
    }

    [HttpGet("{dictCode}/items")]
    public async Task<ApiResult> GetItems(string dictCode, [FromQuery] string? keywords,
        [FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetItemPageAsync(dictCode, keywords, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    [HttpPost("{dictCode}/items")]
    public async Task<ApiResult> CreateItem(string dictCode, [FromBody] SysDictItemFormDto dto, CancellationToken ct)
    {
        await _svc.CreateItemAsync(dictCode, dto, ct);
        return ApiResult.Ok("创建成功");
    }

    [HttpPut("{dictCode}/items/{id:long}")]
    public async Task<ApiResult> UpdateItem(string dictCode, long id, [FromBody] SysDictItemFormDto dto,
        CancellationToken ct)
    {
        await _svc.UpdateItemAsync(dictCode, id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    [HttpDelete("{dictCode}/items/{ids}")]
    public async Task<ApiResult> DeleteItem(string dictCode, string ids, CancellationToken ct)
    {
        await _svc.DeleteItemAsync(dictCode, ids, ct);
        return ApiResult.Ok("删除成功");
    }

    /// <summary>字典项下拉选项（前端调 /dicts/{dictCode}/items/options）。</summary>
    [HttpGet("{dictCode}/items/options")]
    public async Task<ApiResult> ItemOptions(string dictCode, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetItemOptionsAsync(dictCode, ct));
    }

    [HttpGet("{dictCode}/items/{id:long}/form")]
    public async Task<ApiResult> GetItemForm(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetItemFormAsync(id, ct));
    }
}

// ============ 通知公告 ============
[ApiController]
[Route("api/v1/notices")]
[Authorize]
public class SysNoticeController : ControllerBase
{
    private readonly SysNoticeService _svc;

    public SysNoticeController(SysNoticeService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<ApiResult> GetPage([FromQuery] string? title, [FromQuery] int? publishStatus,
        [FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetPageAsync(title, publishStatus, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    [HttpPost]
    public async Task<ApiResult> Create([FromBody] SysNoticeFormDto dto, CancellationToken ct)
    {
        await _svc.CreateAsync(dto, CurrentUserId(), ct);
        return ApiResult.Ok("创建成功");
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] SysNoticeFormDto dto, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, dto, ct);
        return ApiResult.Ok("更新成功");
    }

    [HttpDelete("{ids}")]
    public async Task<ApiResult> Delete(string ids, CancellationToken ct)
    {
        await _svc.DeleteAsync(ids, ct);
        return ApiResult.Ok("删除成功");
    }

    /// <summary>发布（前端 PUT /notices/{id}/publish）。</summary>
    [HttpPut("{id:long}/publish")]
    public async Task<ApiResult> Publish(long id, CancellationToken ct)
    {
        await _svc.PublishAsync(id, CurrentUserId(), ct);
        return ApiResult.Ok("发布成功");
    }

    /// <summary>撤回（前端 PUT /notices/{id}/revoke）。</summary>
    [HttpPut("{id:long}/revoke")]
    public async Task<ApiResult> Revoke(long id, CancellationToken ct)
    {
        await _svc.RevokeAsync(id, ct);
        return ApiResult.Ok("撤回成功");
    }

    /// <summary>编辑回填（前端 GET /notices/{id}/form）。</summary>
    [HttpGet("{id:long}/form")]
    public async Task<ApiResult> GetForm(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetFormAsync(id, ct));
    }

    /// <summary>通知详情（前端 GET /notices/{id}/detail）。</summary>
    [HttpGet("{id:long}/detail")]
    public async Task<ApiResult> GetDetail(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetDetailAsync(id, CurrentUserId(), ct));
    }

    /// <summary>我的通知（前端 GET /notices/my）。</summary>
    [HttpGet("my")]
    public async Task<ApiResult> GetMy([FromQuery] string? title, [FromQuery] int? isRead,
        [FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetMyPageAsync(CurrentUserId(), title, isRead, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    /// <summary>全部已读。</summary>
    [HttpPut("read-all")]
    public async Task<ApiResult> ReadAll(CancellationToken ct)
    {
        await _svc.MarkAllReadAsync(CurrentUserId(), ct);
        return ApiResult.Ok("全部已读");
    }

    private long CurrentUserId()
    {
        return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}

// ============ 日志 ============
[ApiController]
[Route("api/v1/logs")]
[Authorize]
public class SysLogController : ControllerBase
{
    private readonly SysLogService _svc;

    public SysLogController(SysLogService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<ApiResult> GetPage([FromQuery] string? keywords, [FromQuery] string[]? createTime,
        [FromQuery] int pageNum = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetPageAsync(keywords, createTime, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    [HttpGet("analytics/overview")]
    public async Task<ApiResult> Overview(CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetOverviewAsync(ct));
    }

    [HttpGet("analytics/trend")]
    public async Task<ApiResult> Trend([FromQuery] string? startDate, [FromQuery] string? endDate,
        CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetTrendAsync(
            DateTime.TryParse(startDate, out var s) ? s : null,
            DateTime.TryParse(endDate, out var e) ? e : null, ct));
    }
}

// ============ 系统配置 ============
[ApiController]
[Route("api/v1/configs")]
[Authorize]
public class SysConfigController : ControllerBase
{
    private readonly SysConfigService _svc;

    public SysConfigController(SysConfigService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<ApiResult> GetPage([FromQuery] string? keywords, [FromQuery] int pageNum = 1,
        [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetPageAsync(keywords, pageNum, pageSize, ct);
        return ApiResult.Ok(new { list = items, total });
    }

    [HttpPost]
    public async Task<ApiResult> Create([FromBody] SysConfigFormDto dto, CancellationToken ct)
    {
        await _svc.CreateAsync(dto, CurrentUserId(), ct);
        return ApiResult.Ok("创建成功");
    }

    [HttpPut("{id:long}")]
    public async Task<ApiResult> Update(long id, [FromBody] SysConfigFormDto dto, CancellationToken ct)
    {
        await _svc.UpdateAsync(id, dto, CurrentUserId(), ct);
        return ApiResult.Ok("更新成功");
    }

    [HttpGet("{id:long}/form")]
    public async Task<ApiResult> GetForm(long id, CancellationToken ct)
    {
        return ApiResult.Ok(await _svc.GetFormAsync(id, ct));
    }

    /// <summary>刷新配置缓存（我们无缓存，空实现）。</summary>
    [HttpPut("refresh")]
    public async Task<ApiResult> Refresh(CancellationToken ct)
    {
        await _svc.RefreshAsync(ct);
        return ApiResult.Ok("刷新成功");
    }

    private long CurrentUserId()
    {
        return long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}