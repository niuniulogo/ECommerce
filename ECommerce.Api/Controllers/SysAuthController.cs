using System.IdentityModel.Tokens.Jwt;
using ECommerce.Application.Features.SystemManage;
using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

/// <summary>认证：验证码、登录、刷新令牌。</summary>
[ApiController]
[Route("api/v1/auth")]
public class SysAuthController : ControllerBase
{
    private readonly SysAuthService _auth;
    private readonly SysCaptchaService _captcha;
    private readonly SysJwtService _jwt;

    public SysAuthController(SysAuthService auth, SysCaptchaService captcha, SysJwtService jwt)
    {
        _auth = auth;
        _captcha = captcha;
        _jwt = jwt;
    }

    [HttpGet("captcha")]
    [AllowAnonymous]
    public IActionResult Captcha()
    {
        var c = _captcha.Generate();
        return Ok(new ApiResult
        {
            Data = new { captchaId = c.CaptchaId, captchaBase64 = c.CaptchaBase64 }, Msg = "ok",
            Code = ResultCodes.Success, Success = true
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ApiResult> Login([FromBody] SysLoginDto dto, CancellationToken ct)
    {
        if (!_captcha.Validate(dto.CaptchaId, dto.CaptchaCode))
            return ApiResult.Fail("验证码错误或已过期", ResultCodes.ValidationError);
        var result = await _auth.LoginAsync(dto.UserName, dto.Password, ct);
        return ApiResult.Ok(result, "登录成功");
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public ApiResult Refresh([FromQuery] string refreshToken)
    {
        if (!_jwt.TryParse(refreshToken, out var uid, out var uname) || !IsRefreshToken(refreshToken))
            return ApiResult.Fail("refresh token 无效", ResultCodes.Unauthorized);
        var access = _jwt.Generate(uid, uname);
        var refresh = _jwt.Generate(uid, uname, SysJwtService.RefreshUse);
        return ApiResult.Ok(new SysLoginResultDto
            { AccessToken = access, RefreshToken = refresh, ExpiresIn = _jwt.AccessMinutes * 60 });
    }

    /// <summary>退出登录（JWT 无状态，前端清 token 即可）。</summary>
    [HttpDelete("logout")]
    [Authorize]
    public ApiResult Logout()
    {
        return ApiResult.Ok("退出成功");
    }

    private bool IsRefreshToken(string token)
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