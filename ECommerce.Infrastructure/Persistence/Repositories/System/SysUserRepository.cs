using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;

namespace ECommerce.Infrastructure.Persistence.Repositories.System;

public class SysUserRepository : ISysUserRepository
{
    private readonly IFreeSql _fsql;

    public SysUserRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<SysUser?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysUser>().Where(x => x.Id == id && x.IsDeleted == 0).FirstAsync(ct)!;
    }

    public Task<SysUser?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return _fsql.Select<SysUser>().Where(x => x.Username == username && x.IsDeleted == 0).FirstAsync(ct)!;
    }

    public Task<bool> ExistsUsernameAsync(string username, long? excludeId = null, CancellationToken ct = default)
    {
        var s = _fsql.Select<SysUser>().Where(x => x.Username == username && x.IsDeleted == 0);
        if (excludeId.HasValue) s = s.Where(x => x.Id != excludeId);
        return s.AnyAsync(ct);
    }

    public async Task<long> InsertAsync(SysUser user, CancellationToken ct = default)
    {
        return user.Id = await _fsql.Insert(user).ExecuteIdentityAsync(ct);
    }

    public Task UpdateAsync(SysUser user, CancellationToken ct = default)
    {
        return _fsql.Update<SysUser>().SetSource(user).ExecuteAffrowsAsync(ct);
    }

    public async Task<bool> UpdatePasswordAsync(long id, string hashedPassword, CancellationToken ct = default)
    {
        return await _fsql.Update<SysUser>().Set(x => x.Password, hashedPassword)
            .Set(x => x.UpdateTime, DateTime.UtcNow).Where(x => x.Id == id && x.IsDeleted == 0)
            .ExecuteAffrowsAsync(ct) > 0;
    }

    public async Task DeleteByIdsAsync(List<long> ids, CancellationToken ct = default)
    {
        await _fsql.Update<SysUser>().Set(x => x.IsDeleted, 1).Set(x => x.UpdateTime, DateTime.UtcNow)
            .Where(x => ids.Contains(x.Id)).ExecuteAffrowsAsync(ct);
    }

    public async Task CreateWithRolesAsync(SysUser user, IEnumerable<long> roleIds, CancellationToken ct = default)
    {
        user.Id = await _fsql.Insert(user).ExecuteIdentityAsync(ct);
        var links = roleIds.Select(rid => new SysUserRole { UserId = user.Id, RoleId = rid }).ToList();
        if (links.Count > 0) await _fsql.Insert(links).ExecuteAffrowsAsync(ct);
    }

    public async Task UpdateWithRolesAsync(SysUser user, IEnumerable<long> roleIds, CancellationToken ct = default)
    {
        await _fsql.Update<SysUser>().SetSource(user).ExecuteAffrowsAsync(ct);
        await _fsql.Delete<SysUserRole>().Where(x => x.UserId == user.Id).ExecuteAffrowsAsync(ct);
        var links = roleIds.Select(rid => new SysUserRole { UserId = user.Id, RoleId = rid }).ToList();
        if (links.Count > 0) await _fsql.Insert(links).ExecuteAffrowsAsync(ct);
    }

    public async Task<(List<SysUser> Items, long Total)> QueryPageAsync(string? keywords, long? deptId, DateTime? begin,
        DateTime? end, int pageNum, int pageSize, CancellationToken ct = default)
    {
        pageNum = pageNum <= 0 ? 1 : pageNum;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var s = _fsql.Select<SysUser>().Where(x => x.IsDeleted == 0);
        if (!string.IsNullOrWhiteSpace(keywords))
            s = s.Where(x =>
                x.Username.Contains(keywords) || (x.Nickname != null && x.Nickname.Contains(keywords)) ||
                (x.Mobile != null && x.Mobile.Contains(keywords)));
        if (deptId.HasValue) s = s.Where(x => x.DeptId == deptId);
        if (begin.HasValue) s = s.Where(x => x.CreateTime >= begin);
        if (end.HasValue) s = s.Where(x => x.CreateTime < end);
        var total = await s.CountAsync(ct);
        var items = await s.OrderByDescending(x => x.Id).Page(pageNum, pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<List<SysUser>> GetActiveUsersAsync(CancellationToken ct = default)
    {
        return _fsql.Select<SysUser>().Where(x => x.IsDeleted == 0 && x.Status == 1).ToListAsync(ct);
    }

    public async Task<bool> HasUsersAsync(long deptId, CancellationToken ct = default)
    {
        return await _fsql.Select<SysUser>().Where(x => x.DeptId == deptId && x.IsDeleted == 0).AnyAsync(ct);
    }
}

public class SysUserRoleRepository : ISysUserRoleRepository
{
    private readonly IFreeSql _fsql;

    public SysUserRoleRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public async Task<(List<string> Roles, List<string> Perms)> GetRoleCodesByUserIdAsync(long userId,
        CancellationToken ct = default)
    {
        var roles = await _fsql.Select<SysUserRole, SysRole>()
            .LeftJoin((ur, r) => ur.RoleId == r.Id && r.IsDeleted == 0 && r.Status == 1)
            .Where((ur, r) => ur.UserId == userId)
            .ToListAsync((ur, r) => r.Code, ct);
        var perms = await _fsql.Select<SysUserRole, SysRoleMenu, SysMenu>()
            .LeftJoin((ur, rm, m) => ur.RoleId == rm.RoleId && rm.MenuId == m.Id && m.Visible == 1)
            .Where((ur, rm, m) => ur.UserId == userId && m.Perm != null)
            .ToListAsync((ur, rm, m) => m.Perm!, ct);
        return (roles.Distinct().ToList(), perms.Where(p => !string.IsNullOrEmpty(p)).Distinct().ToList());
    }

    public async Task SetRoleIdsAsync(long userId, IEnumerable<long> roleIds, CancellationToken ct = default)
    {
        await _fsql.Delete<SysUserRole>().Where(x => x.UserId == userId).ExecuteAffrowsAsync(ct);
        var links = roleIds.Select(rid => new SysUserRole { UserId = userId, RoleId = rid }).ToList();
        if (links.Count > 0) await _fsql.Insert(links).ExecuteAffrowsAsync(ct);
    }

    public Task<List<long>> GetRoleIdsByUserIdAsync(long userId, CancellationToken ct = default)
    {
        return _fsql.Select<SysUserRole>().Where(x => x.UserId == userId).ToListAsync(x => x.RoleId, ct);
    }
}