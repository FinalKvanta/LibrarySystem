using System.Security.Cryptography;
using System.Text;

namespace LibrarySystem.Shared.Helpers;

public static class PasswordHelper
{
    public static string HashSha256(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static bool Verify(string input, string hash)
    {
        return HashSha256(input) == hash;
    }
}
