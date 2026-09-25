using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;

namespace ECommerce.Infrastructure.Persistence.Repositories.System;

public class SysRoleRepository : ISysRoleRepository
{
    private readonly IFreeSql _fsql;

    public SysRoleRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<SysRole?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysRole>().Where(x => x.Id == id && x.IsDeleted == 0).FirstAsync(ct)!;
    }

    public Task<bool> ExistsNameAsync(string name, long? excludeId = null, CancellationToken ct = default)
    {
        var s = _fsql.Select<SysRole>().Where(x => x.Name == name && x.IsDeleted == 0);
        if (excludeId.HasValue) s = s.Where(x => x.Id != excludeId);
        return s.AnyAsync(ct);
    }

    public Task<bool> ExistsCodeAsync(string code, long? excludeId = null, CancellationToken ct = default)
    {
        var s = _fsql.Select<SysRole>().Where(x => x.Code == code && x.IsDeleted == 0);
        if (excludeId.HasValue) s = s.Where(x => x.Id != excludeId);
        return s.AnyAsync(ct);
    }

    public async Task<long> InsertAsync(SysRole role, CancellationToken ct = default)
    {
        return role.Id = await _fsql.Insert(role).ExecuteIdentityAsync(ct);
    }

    public Task UpdateAsync(SysRole role, CancellationToken ct = default)
    {
        return _fsql.Update<SysRole>().SetSource(role).ExecuteAffrowsAsync(ct);
    }

    public async Task DeleteByIdsAsync(List<long> ids, CancellationToken ct = default)
    {
        await _fsql.Update<SysRole>().Set(x => x.IsDeleted, 1).Set(x => x.UpdateTime, DateTime.UtcNow)
            .Where(x => ids.Contains(x.Id)).ExecuteAffrowsAsync(ct);
    }

    public async Task<(List<SysRole> Items, int Total)> QueryPageAsync(string? keywords, int pageNum, int pageSize,
        CancellationToken ct = default)
    {
        pageNum = pageNum <= 0 ? 1 : pageNum;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var s = _fsql.Select<SysRole>().Where(x => x.IsDeleted == 0);
        if (!string.IsNullOrWhiteSpace(keywords))
            s = s.Where(x => x.Name.Contains(keywords) || x.Code.Contains(keywords));
        var total = (int)await s.CountAsync(ct);
        var items = await s.OrderBy(x => x.Sort).Page(pageNum, pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<List<SysRole>> GetAllAsync(CancellationToken ct = default)
    {
        return _fsql.Select<SysRole>().Where(x => x.IsDeleted == 0 && x.Status == 1).OrderBy(x => x.Sort)
            .ToListAsync(ct);
    }
}

public class SysRoleMenuRepository : ISysRoleMenuRepository
{
    private readonly IFreeSql _fsql;

    public SysRoleMenuRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<List<long>> GetMenuIdsByRoleIdAsync(long roleId, CancellationToken ct = default)
    {
        return _fsql.Select<SysRoleMenu>().Where(x => x.RoleId == roleId).ToListAsync(x => x.MenuId, ct);
    }

    public async Task SetMenuIdsAsync(long roleId, IEnumerable<long> menuIds, CancellationToken ct = default)
    {
        await _fsql.Delete<SysRoleMenu>().Where(x => x.RoleId == roleId).ExecuteAffrowsAsync(ct);
        var links = menuIds.Select(mid => new SysRoleMenu { RoleId = roleId, MenuId = mid }).ToList();
        if (links.Count > 0) await _fsql.Insert(links).ExecuteAffrowsAsync(ct);
    }
}

public class SysRoleDeptRepository : ISysRoleDeptRepository
{
    private readonly IFreeSql _fsql;

    public SysRoleDeptRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<List<long>> GetDeptIdsByRoleIdAsync(long roleId, CancellationToken ct = default)
    {
        return _fsql.Select<SysRoleDept>().Where(x => x.RoleId == roleId).ToListAsync(x => x.DeptId, ct);
    }

    public async Task SetDeptIdsAsync(long roleId, IEnumerable<long> deptIds, CancellationToken ct = default)
    {
        await _fsql.Delete<SysRoleDept>().Where(x => x.RoleId == roleId).ExecuteAffrowsAsync(ct);
        var links = deptIds.Select(did => new SysRoleDept { RoleId = roleId, DeptId = did }).ToList();
        if (links.Count > 0) await _fsql.Insert(links).ExecuteAffrowsAsync(ct);
    }
}