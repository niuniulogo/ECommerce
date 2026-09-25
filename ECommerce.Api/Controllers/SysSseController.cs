using System.Security.Claims;
using System.Text;
using ECommerce.Application.Features.SystemManage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

/// <summary>SSE 实时推送：客户端连接后注册到内存连接管理器，服务端可向指定用户推送事件。</summary>
[ApiController]
[Route("api/v1/sse")]
[Authorize]
public class SysSseController : ControllerBase
{
    private readonly ISseConnectionManager _sse;

    public SysSseController(ISseConnectionManager sse)
    {
        _sse = sse;
    }

    [HttpGet("connect")]
    public async Task Connect(CancellationToken ct)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        Response.Headers.ContentType = "text/event-stream";
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Connection = "keep-alive";

        var connId = _sse.Connect(userId, async (frame, innerCt) =>
        {
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(frame), innerCt);
            await Response.Body.FlushAsync(innerCt);
        }, ct);

        try
        {
            // 先回一个连接成功事件，再保持长连接，定期发心跳。
            var hello = $"event: connected\ndata: {{\"userId\":{userId}}}\n\n";
            await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(hello), ct);
            await Response.Body.FlushAsync(ct);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            while (await timer.WaitForNextTickAsync(ct))
            {
                var ping = ": ping\n\n";
                await Response.Body.WriteAsync(Encoding.UTF8.GetBytes(ping), ct);
                await Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            /* 客户端断开 */
        }
        finally
        {
            _sse.Disconnect(userId, connId);
        }
    }
}