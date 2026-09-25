using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysUserRepository
{
    Task<SysUser?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<SysUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> ExistsUsernameAsync(string username, long? excludeId = null, CancellationToken ct = default);
    Task<long> InsertAsync(SysUser user, CancellationToken ct = default);
    Task UpdateAsync(SysUser user, CancellationToken ct = default);
    Task<bool> UpdatePasswordAsync(long id, string hashedPassword, CancellationToken ct = default);
    Task DeleteByIdsAsync(List<long> ids, CancellationToken ct = default);
    Task CreateWithRolesAsync(SysUser user, IEnumerable<long> roleIds, CancellationToken ct = default);
    Task UpdateWithRolesAsync(SysUser user, IEnumerable<long> roleIds, CancellationToken ct = default);

    Task<(List<SysUser> Items, long Total)> QueryPageAsync(string? keywords, long? deptId, DateTime? begin,
        DateTime? end, int pageNum, int pageSize, CancellationToken ct = default);

    Task<List<SysUser>> GetActiveUsersAsync(CancellationToken ct = default);
    Task<bool> HasUsersAsync(long deptId, CancellationToken ct = default);
}