using ECommerce.Shared.Results;

namespace ECommerce.Api.Endpoints;

/// <summary>文件上传模块端点：/api/v1/upload。</summary>
public static class UploadEndpoints
{
    private static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"];

    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("upload", "Upload").RequireAuthorization();

        // API 场景用 Bearer 令牌鉴权、不依赖 Cookie 表单，关闭默认防伪令牌校验。
        group.MapPost("image", UploadImage).DisableAntiforgery(); // POST /api/v1/upload/image
    }

    /// <summary>
    /// 上传图片
    /// </summary>
    private static async Task<ApiResult> UploadImage(HttpContext context,
        IWebHostEnvironment env, CancellationToken ct)
    {
        // 手动从表单取文件：请求不是 multipart/form-data 或没有文件时给出友好提示，
        // 而不是让参数绑定直接抛异常（与原 Controller 行为一致）。
        IFormFile? file = context.Request.HasFormContentType
            ? context.Request.Form.Files.FirstOrDefault()
            : null;

        if (file is null || file.Length == 0)
            return ApiResult.Fail("请选择要上传的文件");

        if (file.Length > MaxFileSize)
            return ApiResult.Fail("文件大小不能超过 10MB");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return ApiResult.Fail("只支持 jpg、jpeg、png、gif、webp、bmp 格式");

        // 按日期分目录
        var dateDir = DateTime.Now.ToString("yyyyMMdd");
        var uploadDir = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", dateDir);
        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        // 生成唯一文件名
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        // 返回完整可访问的 URL
        var request = context.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var fileUrl = $"{baseUrl}/uploads/{dateDir}/{fileName}";
        int num1 = Random.Shared.Next();
        fileUrl = $"https://picsum.photos/300/300?random={num1}";
        return ApiResult.Ok(new
        {
            url = fileUrl,
            name = file.FileName,
            size = file.Length
        }, "上传成功");
    }
}
