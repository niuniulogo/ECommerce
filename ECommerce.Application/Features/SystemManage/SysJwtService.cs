using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Application.Features.SystemManage;

/// <summary>系统管理模块 JWT 签发/校验（access + refresh 双 token）。</summary>
public class SysJwtService
{
    public const string TokenUseClaim = "token_use";
    public const string AccessUse = "access";
    public const string RefreshUse = "refresh";
    private readonly string _audience;
    private readonly string _issuer;
    private readonly int _refreshMinutes;

    private readonly string _secret;

    public SysJwtService(IConfiguration config)
    {
        var section = config.GetSection("Jwt");
        _secret = section["Key"] ?? "ecommerce-dev-secret-key-change-me-in-production-0123456789";
        _issuer = section["Issuer"] ?? "ECommerce.Api";
        _audience = section["Audience"] ?? "ECommerce.Client";
        AccessMinutes = int.Parse(section["ExpireMinutes"] ?? "120");
        _refreshMinutes = 7 * 24 * 60;
    }

    public int AccessMinutes { get; }

    public string Generate(long userId, string userName, string tokenUse = AccessUse)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName),
            new(TokenUseClaim, tokenUse),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var expires = DateTime.UtcNow.AddMinutes(tokenUse == RefreshUse ? _refreshMinutes : AccessMinutes);
        var token = new JwtSecurityToken(_issuer, _audience, claims, expires: expires, signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryParse(string token, out long userId, out string userName)
    {
        userId = 0;
        userName = "";
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var p = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true, IssuerSigningKey = key,
                ValidateIssuer = true, ValidIssuer = _issuer,
                ValidateAudience = true, ValidAudience = _audience,
                ValidateLifetime = true, ClockSkew = TimeSpan.Zero
            };
            var principal = handler.ValidateToken(token, p, out _);
            userId = long.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
            userName = principal.FindFirstValue(ClaimTypes.Name)!;
            return true;
        }
        catch
        {
            return false;
        }
    }
}