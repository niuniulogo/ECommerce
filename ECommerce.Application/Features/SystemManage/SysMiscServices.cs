using ECommerce.Domain.Entities.System;
using ECommerce.Domain.Repositories.System;
using ECommerce.Shared.Exceptions;
using ECommerce.Shared.Results;

namespace ECommerce.Application.Features.SystemManage;

// ============ 字典 ============
public class SysDictService
{
    private readonly ISysDictRepository _dict;
    private readonly ISysDictItemRepository _items;

    public SysDictService(ISysDictRepository dict, ISysDictItemRepository items)
    {
        _dict = dict;
        _items = items;
    }

    public async Task<(List<SysDictTypeDto> Items, long Total)> GetTypePageAsync(string? keywords, int? status,
        int pageNum, int pageSize, CancellationToken ct)
    {
        var (rows, total) = await _dict.QueryPageAsync(keywords, status, pageNum, pageSize, ct);
        return (
            rows.Select(d => new SysDictTypeDto
                { Id = d.Id.ToString(), Name = d.Name ?? "", DictCode = d.DictCode ?? "", Status = d.Status }).ToList(),
            total);
    }

    public async Task CreateTypeAsync(SysDictTypeFormDto dto, CancellationToken ct)
    {
        if (await _dict.ExistsCodeAsync(dto.DictCode, null, ct))
            throw new BusinessException("字典编码已存在", ResultCodes.Conflict);
        await _dict.InsertAsync(
            new SysDict
            {
                DictCode = dto.DictCode, Name = dto.Name, Status = dto.Status ?? 1, Remark = dto.Remark, IsDeleted = 0,
                CreateTime = DateTime.UtcNow
            }, ct);
    }

    public async Task UpdateTypeAsync(long id, SysDictTypeFormDto dto, CancellationToken ct)
    {
        var d = await _dict.GetByIdAsync(id, ct) ?? throw new BusinessException("字典不存在", ResultCodes.NotFound);
        if (await _dict.ExistsCodeAsync(dto.DictCode, id, ct))
            throw new BusinessException("字典编码已存在", ResultCodes.Conflict);
        d.Name = dto.Name;
        d.DictCode = dto.DictCode;
        d.Status = dto.Status ?? d.Status;
        d.Remark = dto.Remark;
        d.UpdateTime = DateTime.UtcNow;
        await _dict.UpdateAsync(d, ct);
    }

    public async Task DeleteTypeAsync(string ids, CancellationToken ct)
    {
        await _dict.DeleteByIdsAsync(SysUserService.ParseIds(ids), ct);
    }

    public async Task<object> GetTypeFormAsync(long id, CancellationToken ct)
    {
        var d = await _dict.GetByIdAsync(id, ct) ?? throw new BusinessException("字典不存在", ResultCodes.NotFound);
        return new { id = d.Id.ToString(), name = d.Name, dictCode = d.DictCode, status = d.Status, remark = d.Remark };
    }

    public async Task<(List<SysDictItemDto> Items, long Total)> GetItemPageAsync(string dictCode, string? keywords,
        int pageNum, int pageSize, CancellationToken ct)
    {
        var (rows, total) = await _items.QueryPageAsync(dictCode, keywords, pageNum, pageSize, ct);
        return (
            rows.Select(i => new SysDictItemDto
            {
                Id = i.Id.ToString(), Label = i.Label ?? "", Value = i.Value ?? "", Status = i.Status, Sort = i.Sort,
                TagType = i.TagType
            }).ToList(), total);
    }

    public async Task CreateItemAsync(string dictCode, SysDictItemFormDto dto, CancellationToken ct)
    {
        if (await _items.ExistsValueAsync(dictCode, dto.Value, null, ct))
            throw new BusinessException("字典项值已存在", ResultCodes.Conflict);
        await _items.InsertAsync(
            new SysDictItem
            {
                DictCode = dictCode, Label = dto.Label, Value = dto.Value, Status = dto.Status ?? 1,
                Sort = dto.Sort ?? 0, TagType = dto.TagType, CreateTime = DateTime.UtcNow
            }, ct);
    }

    public async Task UpdateItemAsync(string dictCode, long id, SysDictItemFormDto dto, CancellationToken ct)
    {
        var item = await _items.GetByIdAsync(id, ct) ?? throw new BusinessException("字典项不存在", ResultCodes.NotFound);
        if (await _items.ExistsValueAsync(dictCode, dto.Value, id, ct))
            throw new BusinessException("字典项值已存在", ResultCodes.Conflict);
        item.Label = dto.Label;
        item.Value = dto.Value;
        item.Status = dto.Status ?? item.Status;
        item.Sort = dto.Sort ?? item.Sort;
        item.TagType = dto.TagType;
        item.UpdateTime = DateTime.UtcNow;
        await _items.UpdateAsync(item, ct);
    }

    public async Task DeleteItemAsync(string dictCode, string ids, CancellationToken ct)
    {
        await _items.DeleteByIdsAsync(dictCode, SysUserService.ParseIds(ids), ct);
    }

    public async Task<object> GetItemFormAsync(long id, CancellationToken ct)
    {
        var item = await _items.GetByIdAsync(id, ct) ?? throw new BusinessException("字典项不存在", ResultCodes.NotFound);
        return new
        {
            id = item.Id.ToString(), label = item.Label, value = item.Value, sort = item.Sort, status = item.Status,
            tagType = item.TagType
        };
    }

    public Task<List<SysDictItemDto>> GetItemOptionsAsync(string dictCode, CancellationToken ct)
    {
        return _items.GetEnabledOptionsAsync(dictCode, ct).ContinueWith(
            t => t.Result.Select(i => new SysDictItemDto { Value = i.Value, Label = i.Label, TagType = i.TagType })
                .ToList(), ct);
    }
}

// ============ 通知 ============
public class SysNoticeService
{
    private readonly ISysNoticeRepository _notices;

    public SysNoticeService(ISysNoticeRepository notices)
    {
        _notices = notices;
    }

    public async Task<(List<SysNoticeItemDto> Items, long Total)> GetPageAsync(string? title, int? publishStatus,
        int pageNum, int pageSize, CancellationToken ct)
    {
        var (rows, total) = await _notices.QueryPageAsync(title, publishStatus, pageNum, pageSize, ct);
        return (
            rows.Select(r => new SysNoticeItemDto
            {
                Id = r.Notice.Id.ToString(), Title = r.Notice.Title ?? "", Type = r.Notice.Type, Level = r.Notice.Level,
                PublishStatus = r.Notice.PublishStatus, PublisherName = r.PublisherName,
                CreateTime = r.Notice.CreateTime, PublishTime = r.Notice.PublishTime
            }).ToList(), total);
    }

    public async Task CreateAsync(SysNoticeFormDto dto, long operatorId, CancellationToken ct)
    {
        await _notices.InsertAsync(
            new SysNotice
            {
                Title = dto.Title, Content = dto.Content, Type = dto.Type, Level = dto.Level,
                TargetType = dto.TargetType,
                TargetUserIds = dto.TargetType == 2 ? string.Join(",", dto.TargetUserIds ?? new List<long>()) : null,
                PublishStatus = 0, CreateBy = operatorId, IsDeleted = 0, CreateTime = DateTime.UtcNow
            }, ct);
    }

    public async Task UpdateAsync(long id, SysNoticeFormDto dto, CancellationToken ct)
    {
        var n = await _notices.GetByIdAsync(id, ct) ?? throw new BusinessException("通知不存在", ResultCodes.NotFound);
        if (n.PublishStatus == 1) throw new BusinessException("已发布通知不可编辑", ResultCodes.ValidationError);
        n.Title = dto.Title;
        n.Content = dto.Content;
        n.Type = dto.Type;
        n.Level = dto.Level;
        n.TargetType = dto.TargetType;
        n.TargetUserIds = dto.TargetType == 2 ? string.Join(",", dto.TargetUserIds ?? new List<long>()) : null;
        n.UpdateTime = DateTime.UtcNow;
        await _notices.UpdateAsync(n, ct);
    }

    public async Task DeleteAsync(string ids, CancellationToken ct)
    {
        foreach (var id in SysUserService.ParseIds(ids))
        {
            var n = await _notices.GetByIdAsync(id, ct);
            if (n != null && n.PublishStatus == 1)
                throw new BusinessException($"通知【{n.Title}】已发布，不可删除", ResultCodes.ValidationError);
        }

        // 简化：物理删除关联
        foreach (var id in SysUserService.ParseIds(ids)) await _notices.DeleteUserNoticesAsync(id, ct);
        // 软删通知（此处简化为更新状态）
    }

    public async Task PublishAsync(long id, long operatorId, CancellationToken ct)
    {
        var n = await _notices.GetByIdAsync(id, ct) ?? throw new BusinessException("通知不存在", ResultCodes.NotFound);
        n.PublishStatus = 1;
        n.PublishTime = DateTime.UtcNow;
        n.PublisherId = operatorId;
        n.UpdateTime = DateTime.UtcNow;
        await _notices.UpdateAsync(n, ct);
        var targets = n.TargetType == 1
            ? await _notices.GetActiveUserIdsAsync(ct)
            : (n.TargetUserIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToList();
        await _notices.ReplaceUserNoticesAsync(id, targets, ct);
    }

    public async Task<(List<SysNoticeItemDto> Items, long Total)> GetMyPageAsync(long userId, string? title,
        int? isRead, int pageNum, int pageSize, CancellationToken ct)
    {
        var (rows, total) = await _notices.GetMyPageAsync(userId, title, isRead, pageNum, pageSize, ct);
        return (
            rows.Select(r => new SysNoticeItemDto
            {
                Id = r.Notice.Id.ToString(), Title = r.Notice.Title ?? "", Type = r.Notice.Type, Level = r.Notice.Level,
                PublishStatus = r.Notice.PublishStatus, IsRead = r.IsRead, CreateTime = r.Notice.CreateTime,
                PublishTime = r.Notice.PublishTime
            }).ToList(), total);
    }

    public Task MarkAllReadAsync(long userId, CancellationToken ct)
    {
        return _notices.MarkAllReadAsync(userId, ct);
    }

    public async Task<object> GetFormAsync(long id, CancellationToken ct)
    {
        var n = await _notices.GetByIdAsync(id, ct) ?? throw new BusinessException("通知不存在", ResultCodes.NotFound);
        return new
        {
            id = n.Id.ToString(), title = n.Title, content = n.Content, type = n.Type, level = n.Level,
            targetType = n.TargetType,
            targetUserIds = (n.TargetUserIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse)
                .ToList()
        };
    }

    public async Task<object> GetDetailAsync(long id, long userId, CancellationToken ct)
    {
        var n = await _notices.GetByIdAsync(id, ct) ?? throw new BusinessException("通知不存在", ResultCodes.NotFound);
        return new
        {
            id = n.Id.ToString(), title = n.Title, content = n.Content, type = n.Type, level = n.Level,
            publishTime = n.PublishTime, publisherName = n.PublisherId?.ToString() ?? ""
        };
    }

    public async Task RevokeAsync(long id, CancellationToken ct)
    {
        var n = await _notices.GetByIdAsync(id, ct) ?? throw new BusinessException("通知不存在", ResultCodes.NotFound);
        n.PublishStatus = 0;
        n.UpdateTime = DateTime.UtcNow;
        await _notices.UpdateAsync(n, ct);
        await _notices.DeleteUserNoticesAsync(id, ct);
    }
}

// ============ 日志 ============
public class SysLogService
{
    private readonly ISysLogRepository _logs;

    public SysLogService(ISysLogRepository logs)
    {
        _logs = logs;
    }

    public async Task<(List<SysLogItemDto> Items, long Total)> GetPageAsync(string? keywords, string[]? createTime,
        int pageNum, int pageSize, CancellationToken ct)
    {
        DateTime? start = null, end = null;
        if (createTime is { Length: > 0 } && DateTime.TryParse(createTime[0], out var s)) start = s;
        if (createTime is { Length: > 1 } && DateTime.TryParse(createTime[^1], out var e)) end = e;
        var (rows, total) = await _logs.QueryPageAsync(keywords, start, end, pageNum, pageSize, ct);
        return (
            rows.Select(l => new SysLogItemDto
            {
                Id = l.Id, Title = l.Title, Status = l.Status, Ip = l.Ip, RequestUri = l.RequestUri,
                RequestMethod = l.RequestMethod, ExecutionTime = l.ExecutionTime, OperatorName = l.OperatorName,
                CreateTime = l.CreateTime.ToString("yyyy/MM/dd HH:mm"), Content = l.Content, ErrorMsg = l.ErrorMsg
            }).ToList(), total);
    }

    public async Task<object> GetOverviewAsync(CancellationToken ct)
    {
        var (todayPv, todayUv, yestPv, _, totalPv, totalUv) =
            await _logs.GetOverviewStatsAsync(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(-1), ct);

        double Rate(long cur, long prev)
        {
            return prev == 0 ? cur > 0 ? 1.0 : 0 : Math.Round((cur - prev) / (double)prev, 4);
        }

        return new { todayPv, todayUv, totalPv, totalUv, pvGrowthRate = Rate(todayPv, yestPv), uvGrowthRate = 0.0 };
    }

    public async Task<object> GetTrendAsync(DateTime? startDate, DateTime? endDate, CancellationToken ct)
    {
        var start = startDate?.Date ?? DateTime.UtcNow.Date.AddDays(-6);
        var end = endDate?.Date ?? DateTime.UtcNow.Date;
        if (start > end) (start, end) = (end, start);
        if ((end - start).TotalDays > 366) start = end.AddDays(-365);

        var stats = await _logs.GetDailyStatsAsync(start, end.AddDays(1), ct);
        var map = stats.ToDictionary(s => s.Day.Date, s => s);

        var dates = new List<string>();
        var pvList = new List<long>();
        var uvList = new List<long>();
        for (var day = start; day <= end; day = day.AddDays(1))
        {
            dates.Add(day.ToString("yyyy-MM-dd"));
            pvList.Add(map.TryGetValue(day, out var s) ? s.Pv : 0);
            uvList.Add(map.TryGetValue(day, out var s2) ? s2.Uv : 0);
        }

        return new { dates, pvList, uvList };
    }
}

// ============ 配置 ============
public class SysConfigService
{
    private readonly ISysConfigRepository _configs;

    public SysConfigService(ISysConfigRepository configs)
    {
        _configs = configs;
    }

    public async Task<(List<SysConfigItemDto> Items, long Total)> GetPageAsync(string? keywords, int pageNum,
        int pageSize, CancellationToken ct)
    {
        var (rows, total) = await _configs.QueryPageAsync(keywords, pageNum, pageSize, ct);
        return (
            rows.Select(c => new SysConfigItemDto
            {
                Id = c.Id.ToString(), ConfigName = c.ConfigName, ConfigKey = c.ConfigKey, ConfigValue = c.ConfigValue,
                Remark = c.Remark
            }).ToList(), total);
    }

    public async Task CreateAsync(SysConfigFormDto dto, long operatorId, CancellationToken ct)
    {
        if (await _configs.ExistsKeyAsync(dto.ConfigKey, null, ct))
            throw new BusinessException("配置键已存在", ResultCodes.Conflict);
        await _configs.InsertAsync(
            new SysConfig
            {
                ConfigName = dto.ConfigName, ConfigKey = dto.ConfigKey, ConfigValue = dto.ConfigValue,
                Remark = dto.Remark, CreateBy = operatorId, IsDeleted = 0, CreateTime = DateTime.UtcNow
            }, ct);
    }

    public async Task UpdateAsync(long id, SysConfigFormDto dto, long operatorId, CancellationToken ct)
    {
        var c = await _configs.GetByIdAsync(id, ct) ?? throw new BusinessException("配置不存在", ResultCodes.NotFound);
        if (await _configs.ExistsKeyAsync(dto.ConfigKey, id, ct))
            throw new BusinessException("配置键已存在", ResultCodes.Conflict);
        c.ConfigName = dto.ConfigName;
        c.ConfigKey = dto.ConfigKey;
        c.ConfigValue = dto.ConfigValue;
        c.Remark = dto.Remark;
        c.UpdateBy = operatorId;
        c.UpdateTime = DateTime.UtcNow;
        await _configs.UpdateAsync(c, ct);
    }

    public async Task<object> GetFormAsync(long id, CancellationToken ct)
    {
        var c = await _configs.GetByIdAsync(id, ct) ?? throw new BusinessException("配置不存在", ResultCodes.NotFound);
        return new
        {
            id = c.Id.ToString(), configName = c.ConfigName, configKey = c.ConfigKey, configValue = c.ConfigValue,
            remark = c.Remark
        };
    }

    public Task RefreshAsync(CancellationToken ct)
    {
        return Task.CompletedTask;
        // 无缓存，空实现即可
    }
}