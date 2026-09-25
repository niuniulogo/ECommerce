namespace ECommerce.Domain.Repositories.System;

public interface ISysUserRoleRepository
{
    Task<(List<string> Roles, List<string> Perms)> GetRoleCodesByUserIdAsync(long userId,
        CancellationToken ct = default);

    Task SetRoleIdsAsync(long userId, IEnumerable<long> roleIds, CancellationToken ct = default);
    Task<List<long>> GetRoleIdsByUserIdAsync(long userId, CancellationToken ct = default);
}

public interface ISysRoleMenuRepository
{
    Task<List<long>> GetMenuIdsByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task SetMenuIdsAsync(long roleId, IEnumerable<long> menuIds, CancellationToken ct = default);
}

public interface ISysRoleDeptRepository
{
    Task<List<long>> GetDeptIdsByRoleIdAsync(long roleId, CancellationToken ct = default);
    Task SetDeptIdsAsync(long roleId, IEnumerable<long> deptIds, CancellationToken ct = default);
}