using System.Text;
using ECommerce.Application.Features.SystemManage;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Endpoints;

/// <summary>SSE 实时推送模块端点：/api/v1/sse。</summary>
public static class SseEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("sse", "SSE").RequireAuthorization();

        group.MapGet("connect", Connect); // GET /api/v1/sse/connect
    }

    /// <summary>SSE 连接：注册到连接管理器，先回连接成功事件，再保持长连接定期发心跳。</summary>
    private static async Task Connect(HttpContext context,
        [FromServices] ISseConnectionManager sse, CancellationToken ct)
    {
        var userId = CurrentUser.GetId(context.User);

        var response = context.Response;
        response.Headers.ContentType = "text/event-stream";
        response.Headers.CacheControl = "no-cache";
        response.Headers.Connection = "keep-alive";

        // 写入回调封装为具名对象（每个 SSE 长连接一个，替代捕获 Response 的内联 Lambda）。
        var frameWriter = new SseFrameWriter(response.Body);
        var connId = sse.Connect(userId, frameWriter.WriteAsync, ct);

        try
        {
            // 先回一个连接成功事件。
            var hello = Encoding.UTF8.GetBytes(
                $"event: connected\ndata: {{\"userId\":{userId}}}\n\n");
            await response.Body.WriteAsync(hello, ct);
            await response.Body.FlushAsync(ct);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
            while (await timer.WaitForNextTickAsync(ct))
            {
                var ping = Encoding.UTF8.GetBytes(": ping\n\n");
                await response.Body.WriteAsync(ping, ct);
                await response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException)
        {
            /* 客户端断开 */
        }
        finally
        {
            sse.Disconnect(userId, connId);
        }
    }

    /// <summary>SSE 帧写入器：把一帧文本写入响应流并立即刷新。</summary>
    private sealed class SseFrameWriter(Stream body)
    {
        public async Task WriteAsync(string frame, CancellationToken ct)
        {
            await body.WriteAsync(Encoding.UTF8.GetBytes(frame), ct);
            await body.FlushAsync(ct);
        }
    }
}
