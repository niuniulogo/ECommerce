using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysConfigRepository
{
    Task<SysConfig?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<bool> ExistsKeyAsync(string key, long? excludeId = null, CancellationToken ct = default);
    Task<long> InsertAsync(SysConfig config, CancellationToken ct = default);
    Task UpdateAsync(SysConfig config, CancellationToken ct = default);

    Task<(List<SysConfig> Items, long Total)> QueryPageAsync(string? keywords, int pageNum, int pageSize,
        CancellationToken ct = default);

    Task<List<SysConfig>> GetAllEnabledAsync(CancellationToken ct = default);
}