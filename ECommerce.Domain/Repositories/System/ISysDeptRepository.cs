using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysDeptRepository
{
    Task<SysDept?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<SysDept>> GetAllAsync(CancellationToken ct = default);
    Task<bool> ExistsCodeAsync(string code, long? excludeId = null, CancellationToken ct = default);
    Task<long> InsertAsync(SysDept dept, CancellationToken ct = default);
    Task UpdateAsync(SysDept dept, CancellationToken ct = default);
    Task DeleteByIdsAsync(List<long> ids, CancellationToken ct = default);
    Task<List<long>> GetChildIdsAsync(long id, CancellationToken ct = default);
}