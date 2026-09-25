using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysRoleRepository
{
    Task<SysRole?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<bool> ExistsNameAsync(string name, long? excludeId = null, CancellationToken ct = default);
    Task<bool> ExistsCodeAsync(string code, long? excludeId = null, CancellationToken ct = default);
    Task<long> InsertAsync(SysRole role, CancellationToken ct = default);
    Task UpdateAsync(SysRole role, CancellationToken ct = default);
    Task DeleteByIdsAsync(List<long> ids, CancellationToken ct = default);

    Task<(List<SysRole> Items, int Total)> QueryPageAsync(string? keywords, int pageNum, int pageSize,
        CancellationToken ct = default);

    Task<List<SysRole>> GetAllAsync(CancellationToken ct = default);
}