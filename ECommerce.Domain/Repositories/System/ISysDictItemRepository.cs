using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysDictItemRepository
{
    Task<SysDictItem?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<bool> ExistsValueAsync(string dictCode, string value, long? excludeId = null, CancellationToken ct = default);
    Task<long> InsertAsync(SysDictItem item, CancellationToken ct = default);
    Task UpdateAsync(SysDictItem item, CancellationToken ct = default);
    Task DeleteByIdsAsync(string dictCode, List<long> ids, CancellationToken ct = default);

    Task<(List<SysDictItem> Items, long Total)> QueryPageAsync(string dictCode, string? keywords, int pageNum,
        int pageSize, CancellationToken ct = default);

    Task<List<SysDictItem>> GetEnabledOptionsAsync(string dictCode, CancellationToken ct = default);
}