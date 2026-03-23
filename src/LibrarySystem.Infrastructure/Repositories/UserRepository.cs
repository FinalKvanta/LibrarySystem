using Microsoft.EntityFrameworkCore;
using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Infrastructure.Data;

namespace LibrarySystem.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly LibraryDbContext _context;

    public UserRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> AddAsync(User user)
    {
        var entry = await _context.Users.AddAsync(user);
        return entry.Entity;
    }
}
