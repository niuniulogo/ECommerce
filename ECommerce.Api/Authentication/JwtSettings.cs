namespace ECommerce.Api.Authentication;

/// <summary>JWT 配置选项（绑定 appsettings.json 的 Jwt 节）。</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "ECommerce.Api";

    public string Audience { get; set; } = "ECommerce.Client";

    /// <summary>对称签名密钥，长度需不少于 32 个字符（HMAC-SHA256）。</summary>
    public string Key { get; set; } = string.Empty;

    public int ExpireMinutes { get; set; } = 120;
}