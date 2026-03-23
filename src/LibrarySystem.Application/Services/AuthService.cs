using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Enums;
using LibrarySystem.Core.Exceptions;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Application.DTOs;

namespace LibrarySystem.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private static readonly string SecretKey = "LibrarySystem-Secret-Key-2024-KT4-SuperSecure!";
    private static readonly int TokenExpirationHours = 24;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthTokenDto> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        if (user == null)
            throw new DomainException("Invalid username or password.");

        var passwordHash = HashPassword(password);
        if (user.PasswordHash != passwordHash)
            throw new DomainException("Invalid username or password.");

        var token = GenerateToken(user);

        return new AuthTokenDto
        {
            Token = token,
            Username = user.Username,
            Role = user.Role.ToString(),
            ExpiresAt = DateTime.UtcNow.AddHours(TokenExpirationHours)
        };
    }

    public (string Username, UserRole Role) ValidateToken(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3)
                throw new AccessDeniedException("Invalid token format.");

            var header = parts[0];
            var payload = parts[1];
            var signature = parts[2];

            // Verify signature
            var expectedSignature = ComputeHmac($"{header}.{payload}");
            if (signature != expectedSignature)
                throw new AccessDeniedException("Invalid token signature.");

            // Decode payload
            var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
            var claims = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson)!;

            // Check expiration
            var exp = claims["exp"].GetInt64();
            var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
            if (DateTime.UtcNow > expDate)
                throw new AccessDeniedException("Token has expired.");

            var username = claims["sub"].GetString()!;
            var role = Enum.Parse<UserRole>(claims["role"].GetString()!);

            return (username, role);
        }
        catch (AccessDeniedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AccessDeniedException($"Token validation failed: {ex.Message}");
        }
    }

    public void RequireRole(string token, params UserRole[] allowedRoles)
    {
        var (_, role) = ValidateToken(token);
        if (!allowedRoles.Contains(role))
            throw new AccessDeniedException($"Role '{role}' does not have permission for this operation.");
    }

    private string GenerateToken(User user)
    {
        var header = new { alg = "HS256", typ = "JWT" };
        var headerJson = JsonSerializer.Serialize(header);
        var headerBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));

        var payload = new Dictionary<string, object>
        {
            ["sub"] = user.Username,
            ["role"] = user.Role.ToString(),
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["exp"] = DateTimeOffset.UtcNow.AddHours(TokenExpirationHours).ToUnixTimeSeconds()
        };
        var payloadJson = JsonSerializer.Serialize(payload);
        var payloadBase64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        var signature = ComputeHmac($"{headerBase64}.{payloadBase64}");

        return $"{headerBase64}.{payloadBase64}.{signature}";
    }

    private static string ComputeHmac(string input)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
