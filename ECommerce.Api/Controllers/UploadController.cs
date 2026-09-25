using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

/// <summary>
/// 文件上传接口
/// </summary>
[ApiController]
[Route("api/v1/upload")]
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// 上传图片
    /// </summary>
    [HttpPost("image")]
    public async Task<ApiResult> UploadImage(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return ApiResult.Fail("请选择要上传的文件");

        if (file.Length > MaxFileSize)
            return ApiResult.Fail("文件大小不能超过 10MB");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            return ApiResult.Fail("只支持 jpg、jpeg、png、gif、webp、bmp 格式");

        // 按日期分目录
        var dateDir = DateTime.Now.ToString("yyyyMMdd");
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", dateDir);
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
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
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
