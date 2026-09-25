using System.Security.Cryptography;

namespace ECommerce.Shared.Utils;

/// <summary>
///     密码哈希/校验工具。新哈希用 PBKDF2-SHA256；兼容 MesMiniApi 的 BCrypt 哈希（$2a$/$2b$/$2y$ 开头）。
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public static string Hash(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("密码不能为空", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public static bool Verify(string password, string stored)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored))
            return false;

        // BCrypt 哈希（MesMiniApi 数据）：$2a$/$2b$/$2y$ 开头
        if (stored.StartsWith("$2a$") || stored.StartsWith("$2b$") || stored.StartsWith("$2y$"))
            return BCrypt.Net.BCrypt.Verify(password, stored);

        // PBKDF2 哈希：{salt}.{hash}
        var segments = stored.Split('.', 2);
        if (segments.Length != 2)
            return false;

        try
        {
            var salt = Convert.FromBase64String(segments[0]);
            var expected = Convert.FromBase64String(segments[1]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}