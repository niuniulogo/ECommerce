using ECommerce.Domain.Entities.System;

namespace ECommerce.Domain.Repositories.System;

public interface ISysNoticeRepository
{
    Task<SysNotice?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<long> InsertAsync(SysNotice notice, CancellationToken ct = default);
    Task UpdateAsync(SysNotice notice, CancellationToken ct = default);

    Task<(List<(SysNotice Notice, string? PublisherName)> Items, long Total)> QueryPageAsync(string? title,
        int? publishStatus, int pageNum, int pageSize, CancellationToken ct = default);

    Task<(List<(SysNotice Notice, int IsRead)> Items, long Total)> GetMyPageAsync(long userId, string? title,
        int? isRead, int pageNum, int pageSize, CancellationToken ct = default);

    Task MarkReadAsync(long userId, long noticeId, CancellationToken ct = default);
    Task MarkAllReadAsync(long userId, CancellationToken ct = default);
    Task ReplaceUserNoticesAsync(long noticeId, IEnumerable<long> userIds, CancellationToken ct = default);
    Task DeleteUserNoticesAsync(long noticeId, CancellationToken ct = default);
    Task<List<long>> GetActiveUserIdsAsync(CancellationToken ct = default);
    Task<string?> GetUserNicknameAsync(long userId, CancellationToken ct = default);
}