using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;

namespace ECommerce.Infrastructure.Persistence.Repositories.System;

public class SysNoticeRepository : ISysNoticeRepository
{
    private readonly IFreeSql _fsql;

    public SysNoticeRepository(IFreeSql fsql)
    {
        _fsql = fsql;
    }

    public Task<SysNotice?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        return _fsql.Select<SysNotice>().Where(x => x.Id == id && x.IsDeleted == 0).FirstAsync(ct)!;
    }

    public async Task<long> InsertAsync(SysNotice notice, CancellationToken ct = default)
    {
        return notice.Id = await _fsql.Insert(notice).ExecuteIdentityAsync(ct);
    }

    public Task UpdateAsync(SysNotice notice, CancellationToken ct = default)
    {
        return _fsql.Update<SysNotice>().SetSource(notice).ExecuteAffrowsAsync(ct);
    }

    public async Task<(List<(SysNotice Notice, string? PublisherName)> Items, long Total)> QueryPageAsync(string? title,
        int? publishStatus, int pageNum, int pageSize, CancellationToken ct = default)
    {
        pageNum = pageNum <= 0 ? 1 : pageNum;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var q = _fsql.Select<SysNotice>().Where(x => x.IsDeleted == 0);
        if (!string.IsNullOrWhiteSpace(title)) q = q.Where(x => x.Title!.Contains(title));
        if (publishStatus.HasValue) q = q.Where(x => x.PublishStatus == publishStatus);
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(x => x.Id).Page(pageNum, pageSize).ToListAsync(ct);
        var publisherIds = items.Select(x => x.PublisherId ?? 0).Distinct().ToList();
        var names = await _fsql.Select<SysUser>().Where(x => publisherIds.Contains(x.Id))
            .ToListAsync(x => new { x.Id, x.Nickname }, ct);
        return (items.Select(n => (n, names.FirstOrDefault(x => x.Id == (n.PublisherId ?? 0))?.Nickname)).ToList(),
            total);
    }

    public async Task<(List<(SysNotice Notice, int IsRead)> Items, long Total)> GetMyPageAsync(long userId,
        string? title, int? isRead, int pageNum, int pageSize, CancellationToken ct = default)
    {
        pageNum = pageNum <= 0 ? 1 : pageNum;
        pageSize = pageSize <= 0 ? 10 : pageSize;
        var query = _fsql.Select<SysUserNotice, SysNotice>()
            .LeftJoin((un, n) => un.NoticeId == n.Id && n.IsDeleted == 0 && n.PublishStatus == 1)
            .Where((un, n) => un.UserId == userId && un.IsDeleted == 0);
        if (!string.IsNullOrWhiteSpace(title)) query = query.Where((un, n) => n.Title!.Contains(title));
        if (isRead.HasValue) query = query.Where((un, n) => un.IsRead == isRead);
        var total = await query.CountAsync(ct);
        var list = await query.OrderByDescending((un, n) => n.PublishTime).Page(pageNum, pageSize)
            .ToListAsync((un, n) => new { Notice = n, un.IsRead }, ct);
        return (list.Where(x => x.Notice != null).Select(x => (x.Notice!, x.IsRead)).ToList(), total);
    }

    public async Task MarkReadAsync(long userId, long noticeId, CancellationToken ct = default)
    {
        await _fsql.Update<SysUserNotice>()
            .Set(x => x.IsRead, 1)
            .Set(x => x.ReadTime, DateTime.UtcNow)
            .Where(x => x.UserId == userId && x.NoticeId == noticeId && x.IsRead == 0)
            .ExecuteAffrowsAsync(ct);
    }

    public async Task MarkAllReadAsync(long userId, CancellationToken ct = default)
    {
        await _fsql.Update<SysUserNotice>()
            .Set(x => x.IsRead, 1)
            .Set(x => x.ReadTime, DateTime.UtcNow)
            .Where(x => x.UserId == userId && x.IsRead == 0 && x.IsDeleted == 0)
            .ExecuteAffrowsAsync(ct);
    }

    public async Task ReplaceUserNoticesAsync(long noticeId, IEnumerable<long> userIds, CancellationToken ct = default)
    {
        await _fsql.Delete<SysUserNotice>().Where(x => x.NoticeId == noticeId).ExecuteAffrowsAsync(ct);
        var rows = userIds.Select(uid => new SysUserNotice { NoticeId = noticeId, UserId = uid, IsRead = 0 }).ToList();
        if (rows.Count > 0) await _fsql.Insert(rows).ExecuteAffrowsAsync(ct);
    }

    public async Task DeleteUserNoticesAsync(long noticeId, CancellationToken ct = default)
    {
        await _fsql.Delete<SysUserNotice>().Where(x => x.NoticeId == noticeId).ExecuteAffrowsAsync(ct);
    }

    public Task<List<long>> GetActiveUserIdsAsync(CancellationToken ct = default)
    {
        return _fsql.Select<SysUser>().Where(x => x.IsDeleted == 0 && x.Status == 1).ToListAsync(x => x.Id, ct);
    }

    public async Task<string?> GetUserNicknameAsync(long userId, CancellationToken ct = default)
    {
        return await _fsql.Select<SysUser>().Where(x => x.Id == userId).FirstAsync(x => x.Nickname, ct);
    }
}