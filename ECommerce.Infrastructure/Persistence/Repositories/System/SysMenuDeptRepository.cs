using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;

namespace ECommerce.Infrastructure.Persistence.Repositories.System;

public class SysMenuRepository : ISysMenuRepository
{
    private readonly IFreeSql _fsql;

    public SysMenuRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<SysMenu?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysMenu>().Where(x => x.Id == id).FirstAsync(ct)!;
    }

    public Task<List<SysMenu>> GetAllAsync(CancellationToken ct = default)
    {
        return _fsql.Select<SysMenu>().ToListAsync(ct);
    }

    public Task<List<SysMenu>> GetAllEnabledAsync(CancellationToken ct = default)
    {
        return _fsql.Select<SysMenu>().Where(x => x.Visible == 1).OrderBy(x => x.Sort).ToListAsync(ct);
    }

    public async Task<long> InsertAsync(SysMenu menu, CancellationToken ct = default)
    {
        return menu.Id = await _fsql.Insert(menu).ExecuteIdentityAsync(ct);
    }

    public Task UpdateAsync(SysMenu menu, CancellationToken ct = default)
    {
        return _fsql.Update<SysMenu>().SetSource(menu).ExecuteAffrowsAsync(ct);
    }

    public Task DeleteAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Delete<SysMenu>().Where(x => x.Id == id).ExecuteAffrowsAsync(ct);
    }

    public async Task<bool> HasChildrenAsync(long id, CancellationToken ct = default)
    {
        return await _fsql.Select<SysMenu>().Where(x => x.ParentId == id).AnyAsync(ct);
    }

    public async Task<bool> IsAssignedToRoleAsync(long id, CancellationToken ct = default)
    {
        return await _fsql.Select<SysRoleMenu>().Where(x => x.MenuId == id).AnyAsync(ct);
    }

    public Task<List<SysMenu>> GetMenusByUserIdAsync(long userId, CancellationToken ct = default)
    {
        return _fsql.Select<SysUserRole, SysRoleMenu, SysMenu>()
            .LeftJoin((ur, rm, m) => ur.RoleId == rm.RoleId && rm.MenuId == m.Id)
            .Where((ur, rm, m) => ur.UserId == userId && m.Type != "B")
            .Distinct()
            .ToListAsync((ur, rm, m) => m, ct);
    }
}

public class SysDeptRepository : ISysDeptRepository
{
    private readonly IFreeSql _fsql;

    public SysDeptRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<SysDept?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysDept>().Where(x => x.Id == id && x.IsDeleted == 0).FirstAsync(ct)!;
    }

    public Task<List<SysDept>> GetAllAsync(CancellationToken ct = default)
    {
        return _fsql.Select<SysDept>().Where(x => x.IsDeleted == 0).OrderBy(x => x.Sort).ToListAsync(ct);
    }

    public Task<bool> ExistsCodeAsync(string code, long? excludeId = null, CancellationToken ct = default)
    {
        var s = _fsql.Select<SysDept>().Where(x => x.Code == code && x.IsDeleted == 0);
        if (excludeId.HasValue) s = s.Where(x => x.Id != excludeId);
        return s.AnyAsync(ct);
    }

    public async Task<long> InsertAsync(SysDept dept, CancellationToken ct = default)
    {
        return dept.Id = await _fsql.Insert(dept).ExecuteIdentityAsync(ct);
    }

    public Task UpdateAsync(SysDept dept, CancellationToken ct = default)
    {
        return _fsql.Update<SysDept>().SetSource(dept).ExecuteAffrowsAsync(ct);
    }

    public async Task DeleteByIdsAsync(List<long> ids, CancellationToken ct = default)
    {
        await _fsql.Update<SysDept>().Set(x => x.IsDeleted, 1).Set(x => x.UpdateTime, DateTime.UtcNow)
            .Where(x => ids.Contains(x.Id)).ExecuteAffrowsAsync(ct);
    }

    public Task<List<long>> GetChildIdsAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysDept>().Where(x => x.ParentId == id && x.IsDeleted == 0).ToListAsync(x => x.Id, ct);
    }
}