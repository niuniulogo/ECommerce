using System.IdentityModel.Tokens.Jwt;
using ECommerce.Application.Features.SystemManage;
using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Endpoints;

/// <summary>认证模块端点：/api/v1/auth（验证码、登录、刷新令牌、退出）。</summary>
public static class AuthEndpoints
{
    public static void Map(IEndpointRouteBuilder routes)
    {
        var group = routes.CreateModule("auth", "Auth");

        group.MapGet("captcha", Captcha);              // GET    /api/v1/auth/captcha
        group.MapPost("login", Login);                  // POST   /api/v1/auth/login
        group.MapPost("refresh-token", Refresh);        // POST   /api/v1/auth/refresh-token
        group.MapDelete("logout", Logout).RequireAuthorization(); // DELETE /api/v1/auth/logout
    }

    private static IResult Captcha([FromServices] SysCaptchaService captcha)
    {
        var c = captcha.Generate();
        return TypedResults.Ok(new ApiResult
        {
            Data = new { captchaId = c.CaptchaId, captchaBase64 = c.CaptchaBase64 },
            Msg = "ok",
            Code = ResultCodes.Success,
            Success = true
        });
    }

    private static async Task<ApiResult> Login([FromBody] SysLoginDto dto,
        [FromServices] SysCaptchaService captcha,
        [FromServices] SysAuthService auth,
        CancellationToken ct)
    {
        if (!captcha.Validate(dto.CaptchaId, dto.CaptchaCode))
            return ApiResult.Fail("验证码错误或已过期", ResultCodes.ValidationError);

        var result = await auth.LoginAsync(dto.UserName, dto.Password, ct);
        return ApiResult.Ok(result, "登录成功");
    }

    private static ApiResult Refresh([FromQuery] string refreshToken,
        [FromServices] SysJwtService jwt)
    {
        if (!jwt.TryParse(refreshToken, out var uid, out var uname) || !IsRefreshToken(refreshToken))
            return ApiResult.Fail("refresh token 无效", ResultCodes.Unauthorized);

        var access = jwt.Generate(uid, uname);
        var refresh = jwt.Generate(uid, uname, SysJwtService.RefreshUse);
        return ApiResult.Ok(new SysLoginResultDto
        {
            AccessToken = access,
            RefreshToken = refresh,
            ExpiresIn = jwt.AccessMinutes * 60
        });
    }

    /// <summary>退出登录（JWT 无状态，前端清 token 即可）。</summary>
    private static ApiResult Logout()
    {
        return ApiResult.Ok("退出成功");
    }

    private static bool IsRefreshToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.Claims.FirstOrDefault(c => c.Type == SysJwtService.TokenUseClaim)?.Value ==
                   SysJwtService.RefreshUse;
        }
        catch
        {
            return false;
        }
    }
}
