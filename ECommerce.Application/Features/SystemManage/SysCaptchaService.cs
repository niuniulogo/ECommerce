using Microsoft.Extensions.Caching.Memory;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace ECommerce.Application.Features.SystemManage;

public record SysCaptchaResult(string CaptchaId, string CaptchaBase64);

/// <summary>数学验证码：生成算术题 PNG 图片（base64），答案存内存缓存 5 分钟。</summary>
public class SysCaptchaService
{
    private const string Prefix = "sys:captcha:";
    private static readonly Random _rnd = new();
    private static readonly string[] _operators = { "+", "-", "×" };

    // 5x7 点阵字体
    private static readonly Dictionary<char, string[]> Font5x7 = new()
    {
        { '0', new[] { "01110", "10001", "10011", "10101", "11001", "10001", "01110" } },
        { '1', new[] { "00100", "01100", "00100", "00100", "00100", "00100", "01110" } },
        { '2', new[] { "01110", "10001", "00001", "00110", "01000", "10000", "11111" } },
        { '3', new[] { "11111", "00010", "00100", "00010", "00001", "10001", "01110" } },
        { '4', new[] { "00010", "00110", "01010", "10010", "11111", "00010", "00010" } },
        { '5', new[] { "11111", "10000", "11110", "00001", "00001", "10001", "01110" } },
        { '6', new[] { "00110", "01000", "10000", "11110", "10001", "10001", "01110" } },
        { '7', new[] { "11111", "00001", "00010", "00100", "01000", "01000", "01000" } },
        { '8', new[] { "01110", "10001", "10001", "01110", "10001", "10001", "01110" } },
        { '9', new[] { "01110", "10001", "10001", "01111", "00001", "00010", "01100" } },
        { '+', new[] { "00000", "00100", "00100", "11111", "00100", "00100", "00000" } },
        { '-', new[] { "00000", "00000", "00000", "11111", "00000", "00000", "00000" } },
        { '×', new[] { "00000", "10001", "01010", "00100", "01010", "10001", "00000" } },
        { '=', new[] { "00000", "00000", "11111", "00000", "11111", "00000", "00000" } },
        { '?', new[] { "01110", "10001", "00001", "00110", "00100", "00000", "00100" } },
        { ' ', new[] { "00000", "00000", "00000", "00000", "00000", "00000", "00000" } }
    };

    private readonly IMemoryCache _cache;

    public SysCaptchaService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public SysCaptchaResult Generate(int width = 200, int height = 70)
    {
        var (question, answer) = GenerateMathQuestion();
        var base64 = GenerateCaptchaImage(question, width, height);
        var id = Guid.NewGuid().ToString("N");
        _cache.Set(Prefix + id, answer, TimeSpan.FromMinutes(5));
        return new SysCaptchaResult(id, base64);
    }

    public bool Validate(string captchaId, string answer)
    {
        if (string.IsNullOrEmpty(captchaId) || !_cache.TryGetValue(Prefix + captchaId, out int real))
            return false;
        _cache.Remove(Prefix + captchaId);
        return int.TryParse(answer, out var input) && input == real;
    }

    private (string question, int answer) GenerateMathQuestion()
    {
        var op = _operators[_rnd.Next(_operators.Length)];
        int a, b, answer;
        switch (op)
        {
            case "+":
                a = _rnd.Next(0, 20);
                b = _rnd.Next(0, 20);
                answer = a + b;
                return ($"{a} + {b} = ?", answer);
            case "-":
                a = _rnd.Next(0, 20);
                b = _rnd.Next(0, a + 1);
                answer = a - b;
                return ($"{a} - {b} = ?", answer);
            default:
                a = _rnd.Next(1, 10);
                b = _rnd.Next(1, 10);
                answer = a * b;
                return ($"{a} × {b} = ?", answer);
        }
    }

    private static string GenerateCaptchaImage(string question, int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);

        // 背景
        var bg = new Rgba32(240, 248, 255);
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            image[x, y] = bg;

        // 干扰线
        for (var i = 0; i < 4; i++)
        {
            var c = new Rgba32((byte)_rnd.Next(100, 220), (byte)_rnd.Next(100, 220), (byte)_rnd.Next(100, 220));
            DrawLine(image, _rnd.Next(width), _rnd.Next(height), _rnd.Next(width), _rnd.Next(height), c, 2);
        }

        // 文字
        var tc = new Rgba32((byte)_rnd.Next(30, 120), (byte)_rnd.Next(30, 120), (byte)_rnd.Next(30, 120));
        DrawText(image, question, width / 2, height / 2, tc, 3);

        // 噪点
        for (var i = 0; i < 80; i++)
        {
            int x = _rnd.Next(width), y = _rnd.Next(height);
            var c = new Rgba32((byte)_rnd.Next(256), (byte)_rnd.Next(256), (byte)_rnd.Next(256));
            for (var dx = 0; dx < 2 && x + dx < width; dx++)
            for (var dy = 0; dy < 2 && y + dy < height; dy++)
                image[x + dx, y + dy] = c;
        }

        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
    }

    private static void DrawLine(Image<Rgba32> img, int x1, int y1, int x2, int y2, Rgba32 color, int thickness)
    {
        int dx = Math.Abs(x2 - x1), dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1, sy = y1 < y2 ? 1 : -1;
        var err = dx - dy;
        while (true)
        {
            for (var tx = -thickness / 2; tx <= thickness / 2; tx++)
            for (var ty = -thickness / 2; ty <= thickness / 2; ty++)
            {
                int px = x1 + tx, py = y1 + ty;
                if (px >= 0 && px < img.Width && py >= 0 && py < img.Height)
                    img[px, py] = color;
            }

            if (x1 == x2 && y1 == y2) break;
            var e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x1 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y1 += sy;
            }
        }
    }

    private static void DrawText(Image<Rgba32> img, string text, int cx, int cy, Rgba32 color, int scale)
    {
        int cw = 6 * scale, ch = 8 * scale;
        var sx = cx - text.Length * cw / 2;
        var sy = cy - 7 * scale / 2;
        for (var i = 0; i < text.Length; i++)
        {
            if (!Font5x7.TryGetValue(text[i], out var bmp)) continue;
            for (var row = 0; row < 7; row++)
            for (var col = 0; col < 5; col++)
                if (bmp[row][col] == '1')
                    for (var dx = 0; dx < scale; dx++)
                    for (var dy = 0; dy < scale; dy++)
                    {
                        var x = sx + i * cw + col * scale + dx;
                        var y = sy + row * scale + dy;
                        if (x >= 0 && x < img.Width && y >= 0 && y < img.Height)
                            img[x, y] = color;
                    }
        }
    }
}