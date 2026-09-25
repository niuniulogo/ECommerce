using System.Collections.Concurrent;
using System.Text.Json;

namespace ECommerce.Application.Features.SystemManage;

public interface ISseConnectionManager
{
    string Connect(long userId, Func<string, CancellationToken, Task> writer, CancellationToken ct);
    void Disconnect(long userId, string connectionId);
    Task SendToUsersAsync(IEnumerable<long> userIds, string eventName, object payload);
    Task BroadcastAsync(string eventName, object payload);
}

/// <summary>进程内 SSE 连接管理：内存字典维护 userId -> 写入回调。</summary>
public class SseConnectionManager : ISseConnectionManager
{
    private readonly ConcurrentDictionary<long, ConcurrentDictionary<string, Func<string, CancellationToken, Task>>>
        _connections = new();

    public string Connect(long userId, Func<string, CancellationToken, Task> writer, CancellationToken ct)
    {
        var connId = Guid.NewGuid().ToString("N");
        var set = _connections.GetOrAdd(userId,
            _ => new ConcurrentDictionary<string, Func<string, CancellationToken, Task>>());
        set[connId] = writer;
        return connId;
    }

    public void Disconnect(long userId, string connectionId)
    {
        if (_connections.TryGetValue(userId, out var set))
            set.TryRemove(connectionId, out _);
    }

    public async Task SendToUsersAsync(IEnumerable<long> userIds, string eventName, object payload)
    {
        var frame = $"event: {eventName}\ndata: {JsonSerializer.Serialize(payload)}\n\n";
        foreach (var uid in userIds.Distinct())
        {
            if (!_connections.TryGetValue(uid, out var set)) continue;
            foreach (var writer in set.Values)
                try
                {
                    await writer(frame, CancellationToken.None);
                }
                catch
                {
                    /* 离线忽略 */
                }
        }
    }

    public Task BroadcastAsync(string eventName, object payload)
    {
        return SendToUsersAsync(_connections.Keys, eventName, payload);
    }
}