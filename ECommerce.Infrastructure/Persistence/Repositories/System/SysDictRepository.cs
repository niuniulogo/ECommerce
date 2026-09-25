using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;

namespace ECommerce.Infrastructure.Persistence.Repositories.System;

public class SysDictRepository : ISysDictRepository
{
    private readonly IFreeSql _fsql;

    public SysDictRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<SysDict?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysDict>().Where(x => x.Id == id && x.IsDeleted == 0).FirstAsync(ct)!;
    }

    public Task<bool> ExistsCodeAsync(string code, long? excludeId = null, CancellationToken ct = default)
    {
        var s = _fsql.Select<SysDict>().Where(x => x.DictCode == code && x.IsDeleted == 0);
        if (excludeId.HasValue) s = s.Where(x => x.Id != excludeId);
        return s.AnyAsync(ct);
    }

    public async Task<long> InsertAsync(SysDict dict, CancellationToken ct = default)
    {
        return dict.Id = await _fsql.Insert(dict).ExecuteIdentityAsync(ct);
    }

    public Task UpdateAsync(SysDict dict, CancellationToken ct = default)
    {
        return _fsql.Update<SysDict>().SetSource(dict).ExecuteAffrowsAsync(ct);
    }

    public async Task DeleteByIdsAsync(List<long> ids, CancellationToken ct = default)
    {
        await _fsql.Update<SysDict>().Set(x => x.IsDeleted, 1).Where(x => ids.Contains(x.Id))
            .ExecuteAffrowsAsync(ct);
    }

    public async Task<(List<SysDict> Items, long Total)> QueryPageAsync(string? keywords, int? status, int pageNum,
        int pageSize, CancellationToken ct = default)
    {
        pageNum = pageNum <= 0 ? 1 : pageNum;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var s = _fsql.Select<SysDict>().Where(x => x.IsDeleted == 0);
        if (!string.IsNullOrWhiteSpace(keywords))
            s = s.Where(x => x.Name!.Contains(keywords) || x.DictCode!.Contains(keywords));
        if (status.HasValue) s = s.Where(x => x.Status == status);
        var total = await s.CountAsync(ct);
        var items = await s.OrderByDescending(x => x.Id).Page(pageNum, pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<List<SysDict>> GetEnabledAsync(CancellationToken ct = default)
    {
        return _fsql.Select<SysDict>().Where(x => x.IsDeleted == 0 && x.Status == 1).ToListAsync(ct);
    }
}

public class SysDictItemRepository : ISysDictItemRepository
{
    private readonly IFreeSql _fsql;

    public SysDictItemRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<SysDictItem?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysDictItem>().Where(x => x.Id == id).FirstAsync(ct)!;
    }

    public Task<bool> ExistsValueAsync(string dictCode, string value, long? excludeId = null,
        CancellationToken ct = default)
    {
        var s = _fsql.Select<SysDictItem>().Where(x => x.DictCode == dictCode && x.Value == value);
        if (excludeId.HasValue) s = s.Where(x => x.Id != excludeId);
        return s.AnyAsync(ct);
    }

    public async Task<long> InsertAsync(SysDictItem item, CancellationToken ct = default)
    {
        return item.Id = await _fsql.Insert(item).ExecuteIdentityAsync(ct);
    }

    public Task UpdateAsync(SysDictItem item, CancellationToken ct = default)
    {
        return _fsql.Update<SysDictItem>().SetSource(item).ExecuteAffrowsAsync(ct);
    }

    public async Task DeleteByIdsAsync(string dictCode, List<long> ids, CancellationToken ct = default)
    {
        await _fsql.Delete<SysDictItem>().Where(x => x.DictCode == dictCode && ids.Contains(x.Id))
            .ExecuteAffrowsAsync(ct);
    }

    public async Task<(List<SysDictItem> Items, long Total)> QueryPageAsync(string dictCode, string? keywords,
        int pageNum, int pageSize, CancellationToken ct = default)
    {
        pageNum = pageNum <= 0 ? 1 : pageNum;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var s = _fsql.Select<SysDictItem>().Where(x => x.DictCode == dictCode);
        if (!string.IsNullOrWhiteSpace(keywords)) s = s.Where(x => x.Label != null && x.Label.Contains(keywords));
        var total = await s.CountAsync(ct);
        var items = await s.OrderBy(x => x.Sort).Page(pageNum, pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<List<SysDictItem>> GetEnabledOptionsAsync(string dictCode, CancellationToken ct = default)
    {
        return _fsql.Select<SysDictItem>().Where(x => x.DictCode == dictCode && x.Status == 1).OrderBy(x => x.Sort)
            .ToListAsync(ct);
    }
}