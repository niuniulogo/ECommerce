using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysLogRepository
{
    Task<long> InsertAsync(SysLog log, CancellationToken ct = default);

    Task<(List<SysLog> Items, long Total)> QueryPageAsync(string? keywords, DateTime? start, DateTime? end, int pageNum,
        int pageSize, CancellationToken ct = default);

    Task<List<(DateTime Day, long Pv, long Uv)>> GetDailyStatsAsync(DateTime start, DateTime end,
        CancellationToken ct = default);

    Task<(long TodayPv, long TodayUv, long YesterdayPv, long YesterdayUv, long TotalPv, long TotalUv)>
        GetOverviewStatsAsync(DateTime todayStart, DateTime yesterdayStart, CancellationToken ct = default);
}