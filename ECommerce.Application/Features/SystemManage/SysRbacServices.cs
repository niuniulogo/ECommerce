using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Results;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.SystemManage;

// ============ 角色 ============
public class SysRoleService
{
    private readonly ISysRoleDeptRepository _roleDepts;
    private readonly ISysRoleMenuRepository _roleMenus;
    private readonly ISysRoleRepository _roles;

    public SysRoleService(ISysRoleRepository roles, ISysRoleMenuRepository roleMenus, ISysRoleDeptRepository roleDepts)
    {
        _roles = roles;
        _roleMenus = roleMenus;
        _roleDepts = roleDepts;
    }

    public async Task<(List<SysRolePageDto> Items, int Total)> GetPageAsync(string? keywords, int pageNum, int pageSize,
        CancellationToken ct)
    {
        var (items, total) = await _roles.QueryPageAsync(keywords, pageNum, pageSize, ct);
        return (
            items.Select(r => new SysRolePageDto
            {
                Id = r.Id.ToString(), Name = r.Name, Code = r.Code, Sort = r.Sort, Status = r.Status,
                DataScope = r.DataScope, UpdateTime = r.UpdateTime
            }).ToList(), total);
    }
    public async Task CreateAsync(SysRoleFormDto dto, CancellationToken ct)
    {
        if (await _roles.ExistsNameAsync(dto.Name, null, ct)) throw BusinessException.Conflict("角色名称已存在");
        if (await _roles.ExistsCodeAsync(dto.Code, null, ct)) throw BusinessException.Conflict("角色编码已存在");
        await _roles.InsertAsync(
            new SysRole
            {
                Name = dto.Name, Code = dto.Code, Sort = dto.Sort ?? 0, Status = dto.Status ?? 1,
                DataScope = dto.DataScope ?? 1, IsDeleted = 0, CreateTime = DateTime.UtcNow
            }, ct);
    }

    public async Task UpdateAsync(long id, SysRoleFormDto dto, CancellationToken ct)
    {
        var r = await _roles.GetByIdAsync(id, ct) ?? throw new BusinessException("角色不存在", ResultCodes.NotFound);
        if (await _roles.ExistsNameAsync(dto.Name, id, ct))
            throw new BusinessException("角色名称已存在", ResultCodes.Conflict);
        if (await _roles.ExistsCodeAsync(dto.Code, id, ct))
            throw new BusinessException("角色编码已存在", ResultCodes.Conflict);
        r.Name = dto.Name;
        r.Code = dto.Code;
        r.Sort = dto.Sort ?? r.Sort;
        r.Status = dto.Status ?? r.Status;
        r.DataScope = dto.DataScope ?? r.DataScope;
        r.UpdateTime = DateTime.UtcNow;
        await _roles.UpdateAsync(r, ct);
    }

    public async Task DeleteAsync(string ids, CancellationToken ct)
    {
        await _roles.DeleteByIdsAsync(SysUserService.ParseIds(ids), ct);
    }

    public async Task<SysRoleFormDto> GetFormAsync(long id, CancellationToken ct)
    {
        var r = await _roles.GetByIdAsync(id, ct) ?? throw new BusinessException("角色不存在", ResultCodes.NotFound);
        var deptIds = await _roleDepts.GetDeptIdsByRoleIdAsync(id, ct);
        return new SysRoleFormDto
        {
            Id = r.Id.ToString(), Name = r.Name, Code = r.Code,
            Sort = r.Sort, DataScope = r.DataScope, Status = r.Status,
            DeptIds = deptIds.Select(x => x.ToString()).ToList()
        };
    }

    public Task<List<long>> GetMenuIdsAsync(long roleId, CancellationToken ct)
    {
        return _roleMenus.GetMenuIdsByRoleIdAsync(roleId, ct);
    }

    public Task SetMenusAsync(long roleId, List<long> menuIds, CancellationToken ct)
    {
        return _roleMenus.SetMenuIdsAsync(roleId, menuIds, ct);
    }

    public async Task<List<SysOptionDto>> GetOptionsAsync(CancellationToken ct)
    {
        var roles = await _roles.GetAllAsync(ct);
        return roles.Select(r => new SysOptionDto { Value = r.Id.ToString(), Label = r.Name }).ToList();
    }
}

// ============ 菜单 ============
public class SysMenuService
{
    private const string RootRole = "ROOT";
    private readonly ISysMenuRepository _menus;
    private readonly ILogger<SysMenuService> _logger;

    public SysMenuService(ISysMenuRepository menus, ILogger<SysMenuService> logger)
    {
        _menus = menus;
        _logger = logger;
    }

    public async Task<List<SysMenuItemDto>> GetTreeAsync(string? keywords, CancellationToken ct)
    {
        var menus = await _menus.GetAllEnabledAsync(ct);

        // _logger.LogInformation("菜单列表：{Menus}", System.Text.Json.JsonSerializer.Serialize(menus, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));

        var dict = menus.ToDictionary(m => m.Id,
            m => new SysMenuItemDto
            {
                Id = m.Id.ToString(), ParentId = m.ParentId.ToString(), Name = m.Name, Type = m.Type,
                RouteName = m.RouteName, RoutePath = m.RoutePath, Component = m.Component, Icon = m.Icon, Perm = m.Perm,
                Sort = m.Sort, Visible = m.Visible
            });
        var roots = new List<SysMenuItemDto>();
        foreach (var m in menus)
        {
            var item = dict[m.Id];
            if (m.ParentId > 0 && dict.TryGetValue(m.ParentId, out var p)) p.Children.Add(item);
            else roots.Add(item);
        }

        if (!string.IsNullOrWhiteSpace(keywords))
        {
            bool Keep(SysMenuItemDto n)
            {
                n.Children = n.Children.Where(Keep).ToList();
                return n.Children.Count > 0 || n.Name.Contains(keywords, StringComparison.OrdinalIgnoreCase);
            }

            roots = roots.Where(Keep).ToList();
        }

        return roots;
    }

    public async Task<List<SysOptionDto>> GetOptionsAsync(CancellationToken ct)
    {
        var menus = await _menus.GetAllAsync(ct);
        var nodes = menus.Select(m => new SysOptionDto
            { Value = m.Id.ToString(), Label = m.Name, Children = new List<SysOptionDto>() }).ToList();
        var dict = nodes.ToDictionary(n => long.Parse(n.Value!), n => n);
        var roots = new List<SysOptionDto>();
        foreach (var m in menus)
        {
            var node = dict[m.Id];
            if (m.ParentId == 0 || !dict.ContainsKey(m.ParentId))
                roots.Add(node);
            else
                dict[m.ParentId].Children!.Add(node);
        }

        foreach (var n in nodes)
            if (n.Children is { Count: 0 })
                n.Children = null;
        return roots;
    }

    public async Task<object> GetRoutesAsync(long userId, List<string> roles, CancellationToken ct)
    {
        var menus = roles.Contains(RootRole)
            ? (await _menus.GetAllAsync(ct)).Where(m => m.Type != "B").OrderBy(m => m.Sort).ToList()
            : (await _menus.GetMenusByUserIdAsync(userId, ct)).OrderBy(m => m.Sort).ToList();

        var nodes = menus.Select(m => new
        {
            id = m.Id, parentId = m.ParentId, path = m.RoutePath ?? "",
            component = m.Type == "C" ? "Layout" : m.Component,
            name = m.RouteName ?? m.RoutePath ?? "",
            sort = m.Sort,
            meta = new { title = m.Name, icon = m.Icon, hidden = m.Visible == 0, keepAlive = m.KeepAlive == 1 }
        }).Cast<object>().ToList();
        return BuildTree(nodes, 0);
    }

    private static List<object> BuildTree(List<object> all, long parentId)
    {
        return all.Where(n => ((dynamic)n).parentId == parentId)
            .OrderBy(n => ((dynamic)n).sort)
            .Select(n =>
        {
            dynamic d = n;
            var children = BuildTree(all, d.id);
            return (object)new
            {
                d.id, d.parentId, d.path, d.component,
                d.name,
                d.meta, children = children.Count > 0 ? children : null
            };
        }).ToList();
    }

    public async Task<object> GetFormAsync(long id, CancellationToken ct)
    {
        var m = await _menus.GetByIdAsync(id, ct) ?? throw new BusinessException("菜单不存在", ResultCodes.NotFound);
        return new
        {
            id = m.Id.ToString(), parentId = m.ParentId.ToString(), name = m.Name, type = m.Type,
            routeName = m.RouteName, routePath = m.RoutePath, component = m.Component,
            icon = m.Icon, perm = m.Perm, sort = m.Sort, visible = m.Visible
        };
    }

    public async Task CreateAsync(SysMenuFormDto dto, CancellationToken ct)
    {
        var parentId = long.TryParse(dto.ParentId, out var p) ? p : 0;
        var menu = new SysMenu
        {
            Name = dto.Name, Type = dto.Type, RouteName = dto.RouteName, RoutePath = dto.RoutePath,
            Component = dto.Component, Icon = dto.Icon, Perm = dto.Perm, Sort = dto.Sort ?? 0,
            Visible = dto.Visible ?? 1, ParentId = parentId, TreePath = parentId == 0 ? "0" : null,
            CreateTime = DateTime.UtcNow
        };
        await _menus.InsertAsync(menu, ct);
    }

    public async Task UpdateAsync(long id, SysMenuFormDto dto, CancellationToken ct)
    {
        var m = await _menus.GetByIdAsync(id, ct) ?? throw new BusinessException("菜单不存在", ResultCodes.NotFound);
        var parentId = long.TryParse(dto.ParentId, out var p) ? p : 0;
        m.Name = dto.Name;
        m.Type = dto.Type;
        m.RouteName = dto.RouteName;
        m.RoutePath = dto.RoutePath;
        m.Component = dto.Component;
        m.Icon = dto.Icon;
        m.Perm = dto.Perm;
        m.Sort = dto.Sort ?? m.Sort;
        m.Visible = dto.Visible ?? m.Visible;
        m.ParentId = parentId;
        await _menus.UpdateAsync(m, ct);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var old = await _menus.GetByIdAsync(id, ct);
        if (old == null) throw new BusinessException("菜单不存在", ResultCodes.NotFound);
        if (await _menus.HasChildrenAsync(id, ct)) throw new BusinessException("存在子菜单，请先删除子菜单", ResultCodes.Conflict);
        if (await _menus.IsAssignedToRoleAsync(id, ct)) throw new BusinessException("菜单已分配给角色", ResultCodes.Conflict);
        await _menus.DeleteAsync(id, ct);
    }
}

// ============ 部门 ============
public class SysDeptService
{
    private readonly ISysDeptRepository _depts;
    private readonly ISysUserRepository _users;

    public SysDeptService(ISysDeptRepository depts, ISysUserRepository users)
    {
        _depts = depts;
        _users = users;
    }

    public async Task<List<SysDeptItemDto>> GetTreeAsync(string? keywords, int? status, CancellationToken ct)
    {
        var depts = await _depts.GetAllAsync(ct);
        var dict = depts.ToDictionary(d => d.Id,
            d => new SysDeptItemDto
            {
                Id = d.Id.ToString(), ParentId = d.ParentId.ToString(), Name = d.Name, Code = d.Code, Sort = d.Sort,
                Status = d.Status
            });
        var roots = new List<SysDeptItemDto>();
        foreach (var d in depts)
        {
            var item = dict[d.Id];
            if (d.ParentId > 0 && dict.TryGetValue(d.ParentId, out var p)) p.Children.Add(item);
            else roots.Add(item);
        }

        if (!string.IsNullOrWhiteSpace(keywords) || status.HasValue)
        {
            bool Keep(SysDeptItemDto n)
            {
                n.Children = n.Children.Where(Keep).ToList();
                return n.Children.Count > 0 ||
                       ((string.IsNullOrEmpty(keywords) ||
                         n.Name.Contains(keywords, StringComparison.OrdinalIgnoreCase)) &&
                        (!status.HasValue || n.Status == status));
            }

            roots = roots.Where(Keep).ToList();
        }

        return roots;
    }

    public async Task CreateAsync(SysDeptFormDto dto, CancellationToken ct)
    {
        if (await _depts.ExistsCodeAsync(dto.Code, null, ct))
            throw new BusinessException("部门编号已存在", ResultCodes.Conflict);
        var parentId = long.TryParse(dto.ParentId, out var p) ? p : 0;
        await _depts.InsertAsync(
            new SysDept
            {
                Name = dto.Name, Code = dto.Code, ParentId = parentId, Sort = dto.Sort ?? 0, Status = dto.Status ?? 1,
                IsDeleted = 0, CreateTime = DateTime.UtcNow, TreePath = parentId == 0 ? "0" : ""
            }, ct);
    }

    public async Task<object> GetFormAsync(long id, CancellationToken ct)
    {
        var d = await _depts.GetByIdAsync(id, ct) ?? throw new BusinessException("部门不存在", ResultCodes.NotFound);
        return new
        {
            id = d.Id.ToString(), parentId = d.ParentId.ToString(), name = d.Name,
            code = d.Code, sort = d.Sort, status = d.Status
        };
    }

    public async Task UpdateAsync(long id, SysDeptFormDto dto, CancellationToken ct)
    {
        var d = await _depts.GetByIdAsync(id, ct) ?? throw new BusinessException("部门不存在", ResultCodes.NotFound);
        if (await _depts.ExistsCodeAsync(dto.Code, id, ct))
            throw new BusinessException("部门编号已存在", ResultCodes.Conflict);
        d.Name = dto.Name;
        d.Code = dto.Code;
        d.Sort = dto.Sort ?? d.Sort;
        d.Status = dto.Status ?? d.Status;
        d.UpdateTime = DateTime.UtcNow;
        await _depts.UpdateAsync(d, ct);
    }

    public async Task DeleteAsync(string ids, CancellationToken ct)
    {
        var list = SysUserService.ParseIds(ids);
        foreach (var id in list)
        {
            if (await _depts.GetChildIdsAsync(id, ct).ContinueWith(t => t.Result.Count > 0))
                throw new BusinessException("存在子部门，请先删除子部门", ResultCodes.Conflict);
            if (await _users.HasUsersAsync(id, ct)) throw new BusinessException("部门下存在用户，无法删除", ResultCodes.Conflict);
        }

        await _depts.DeleteByIdsAsync(list, ct);
    }

    public async Task<List<SysOptionDto>> GetOptionsAsync(CancellationToken ct)
    {
        var depts = await _depts.GetAllAsync(ct);
        var nodes = depts.Select(d => new SysOptionDto
            { Value = d.Id.ToString(), Label = d.Name, Children = new List<SysOptionDto>() }).ToList();
        // 按 parent_id 组装树
        var dict = nodes.ToDictionary(n => long.Parse(n.Value!), n => n);
        var roots = new List<SysOptionDto>();
        foreach (var d in depts)
        {
            var node = dict[d.Id];
            if (d.ParentId == 0 || !dict.ContainsKey(d.ParentId))
                roots.Add(node);
            else
                dict[d.ParentId].Children!.Add(node);
        }

        // 清理空 children
        foreach (var n in nodes)
            if (n.Children is { Count: 0 })
                n.Children = null;
        return roots;
    }
}