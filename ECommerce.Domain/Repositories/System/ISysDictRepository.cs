using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysDictRepository
{
    Task<SysDict?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<bool> ExistsCodeAsync(string code, long? excludeId = null, CancellationToken ct = default);
    Task<long> InsertAsync(SysDict dict, CancellationToken ct = default);
    Task UpdateAsync(SysDict dict, CancellationToken ct = default);
    Task DeleteByIdsAsync(List<long> ids, CancellationToken ct = default);

    Task<(List<SysDict> Items, long Total)> QueryPageAsync(string? keywords, int? status, int pageNum, int pageSize,
        CancellationToken ct = default);

    Task<List<SysDict>> GetEnabledAsync(CancellationToken ct = default);
}