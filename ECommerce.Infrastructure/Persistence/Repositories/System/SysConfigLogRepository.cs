using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;

namespace ECommerce.Infrastructure.Persistence.Repositories.System;

public class SysConfigRepository : ISysConfigRepository
{
    private readonly IFreeSql _fsql;

    public SysConfigRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<SysConfig?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysConfig>().Where(x => x.Id == id && x.IsDeleted == 0).FirstAsync(ct)!;
    }

    public Task<bool> ExistsKeyAsync(string key, long? excludeId = null, CancellationToken ct = default)
    {
        var s = _fsql.Select<SysConfig>().Where(x => x.ConfigKey == key && x.IsDeleted == 0);
        if (excludeId.HasValue) s = s.Where(x => x.Id != excludeId);
        return s.AnyAsync(ct);
    }

    public async Task<long> InsertAsync(SysConfig config, CancellationToken ct = default)
    {
        return config.Id = await _fsql.Insert(config).ExecuteIdentityAsync(ct);
    }

    public Task UpdateAsync(SysConfig config, CancellationToken ct = default)
    {
        return _fsql.Update<SysConfig>().SetSource(config).ExecuteAffrowsAsync(ct);
    }

    public async Task<(List<SysConfig> Items, long Total)> QueryPageAsync(string? keywords, int pageNum, int pageSize,
        CancellationToken ct = default)
    {
        pageNum = pageNum <= 0 ? 1 : pageNum;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var s = _fsql.Select<SysConfig>().Where(x => x.IsDeleted == 0);
        if (!string.IsNullOrWhiteSpace(keywords))
            s = s.Where(x => x.ConfigName.Contains(keywords) || x.ConfigKey.Contains(keywords));
        var total = await s.CountAsync(ct);
        var items = await s.OrderByDescending(x => x.Id).Page(pageNum, pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task<List<SysConfig>> GetAllEnabledAsync(CancellationToken ct = default)
    {
        return _fsql.Select<SysConfig>().Where(x => x.IsDeleted == 0).ToListAsync(ct);
    }
}

public class SysLogRepository : ISysLogRepository
{
    private readonly IFreeSql _fsql;

    public SysLogRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public async Task<long> InsertAsync(SysLog log, CancellationToken ct = default)
    {
        return log.Id = await _fsql.Insert(log).ExecuteIdentityAsync(ct);
    }

    public async Task<(List<SysLog> Items, long Total)> QueryPageAsync(string? keywords, DateTime? start, DateTime? end,
        int pageNum, int pageSize, CancellationToken ct = default)
    {
        pageNum = pageNum <= 0 ? 1 : pageNum;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var s = _fsql.Select<SysLog>();
        if (!string.IsNullOrWhiteSpace(keywords))
            s = s.Where(x =>
                x.Title.Contains(keywords) || (x.OperatorName != null && x.OperatorName.Contains(keywords)));
        if (start.HasValue) s = s.Where(x => x.CreateTime >= start);
        if (end.HasValue) s = s.Where(x => x.CreateTime < end);
        var total = await s.CountAsync(ct);
        var items = await s.OrderByDescending(x => x.Id).Page(pageNum, pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<List<(DateTime Day, long Pv, long Uv)>> GetDailyStatsAsync(DateTime start, DateTime end,
        CancellationToken ct = default)
    {
        var rows = await _fsql.Select<SysLog>()
            .Where(x => x.CreateTime >= start && x.CreateTime < end)
            .GroupBy(x => x.CreateTime.Date)
            .ToListAsync(g => new { Day = g.Key, Pv = g.Count() }, ct);
        return rows.Select(r => (r.Day, (long)r.Pv, 0L)).ToList();
    }

    public async Task<(long, long, long, long, long, long)> GetOverviewStatsAsync(DateTime todayStart,
        DateTime yesterdayStart, CancellationToken ct = default)
    {
        var todayPv = await _fsql.Select<SysLog>().Where(x => x.CreateTime >= todayStart).CountAsync(ct);
        var yesterdayPv = await _fsql.Select<SysLog>()
            .Where(x => x.CreateTime >= yesterdayStart && x.CreateTime < todayStart).CountAsync(ct);
        var todayUv = await _fsql.Select<SysLog>().Where(x => x.CreateTime >= todayStart && x.Ip != null).Distinct()
            .ToListAsync(x => x.Ip!, ct);
        var totalUv = await _fsql.Select<SysLog>().Where(x => x.Ip != null).Distinct().ToListAsync(x => x.Ip!, ct);
        var yestUv = await _fsql.Select<SysLog>()
            .Where(x => x.CreateTime >= yesterdayStart && x.CreateTime < todayStart && x.Ip != null)
            .Distinct().ToListAsync(x => x.Ip!, ct);
        var totalPv = await _fsql.Select<SysLog>().CountAsync(ct);
        return (todayPv, todayUv.Count, yesterdayPv, yestUv.Count, totalPv, totalUv.Count);
    }
}