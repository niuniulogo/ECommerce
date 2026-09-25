using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysMenuRepository
{
    Task<SysMenu?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<List<SysMenu>> GetAllAsync(CancellationToken ct = default);
    Task<List<SysMenu>> GetAllEnabledAsync(CancellationToken ct = default);
    Task<long> InsertAsync(SysMenu menu, CancellationToken ct = default);
    Task UpdateAsync(SysMenu menu, CancellationToken ct = default);
    Task DeleteAsync(long id, CancellationToken ct = default);
    Task<bool> HasChildrenAsync(long id, CancellationToken ct = default);
    Task<bool> IsAssignedToRoleAsync(long id, CancellationToken ct = default);
    Task<List<SysMenu>> GetMenusByUserIdAsync(long userId, CancellationToken ct = default);
}