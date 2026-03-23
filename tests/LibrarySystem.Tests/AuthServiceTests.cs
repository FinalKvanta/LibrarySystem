using Microsoft.EntityFrameworkCore;
using LibrarySystem.Application.Services;
using LibrarySystem.Core.Enums;
using LibrarySystem.Core.Exceptions;
using LibrarySystem.Infrastructure.Data;
using LibrarySystem.Infrastructure.Repositories;

namespace LibrarySystem.Tests;

public class AuthServiceTests
{
    private static LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new LibraryDbContext(options);
        SeedData.Initialize(context);
        return context;
    }

    private static AuthService CreateService(LibraryDbContext context)
    {
        var userRepo = new UserRepository(context);
        return new AuthService(userRepo);
    }

    [Fact]
    public async Task Authenticate_ValidCredentials_ReturnsToken()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.AuthenticateAsync("admin", "admin");

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.Equal("admin", result.Username);
        Assert.Equal("Admin", result.Role);
        Assert.True(result.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Authenticate_InvalidPassword_ThrowsDomainException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.AuthenticateAsync("admin", "wrongpassword"));
    }

    [Fact]
    public async Task Authenticate_NonExistingUser_ThrowsDomainException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        await Assert.ThrowsAsync<DomainException>(() =>
            service.AuthenticateAsync("nonexistent", "password"));
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsUserInfo()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var authResult = await service.AuthenticateAsync("admin", "admin");
        var (username, role) = service.ValidateToken(authResult.Token);

        Assert.Equal("admin", username);
        Assert.Equal(UserRole.Admin, role);
    }

    [Fact]
    public void ValidateToken_InvalidToken_ThrowsAccessDeniedException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        Assert.Throws<AccessDeniedException>(() => service.ValidateToken("invalid.token.here"));
    }

    [Fact]
    public async Task RequireRole_CorrectRole_DoesNotThrow()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var authResult = await service.AuthenticateAsync("admin", "admin");

        // Should not throw
        service.RequireRole(authResult.Token, UserRole.Admin);
    }

    [Fact]
    public async Task RequireRole_WrongRole_ThrowsAccessDeniedException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var authResult = await service.AuthenticateAsync("reader", "reader");

        Assert.Throws<AccessDeniedException>(() =>
            service.RequireRole(authResult.Token, UserRole.Admin, UserRole.Librarian));
    }

    [Fact]
    public async Task Authenticate_AllRoles_ReturnCorrectRoles()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var admin = await service.AuthenticateAsync("admin", "admin");
        Assert.Equal("Admin", admin.Role);

        var librarian = await service.AuthenticateAsync("librarian", "librarian");
        Assert.Equal("Librarian", librarian.Role);

        var reader = await service.AuthenticateAsync("reader", "reader");
        Assert.Equal("Reader", reader.Role);
    }
}
