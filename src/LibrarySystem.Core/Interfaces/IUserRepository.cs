using LibrarySystem.Core.Entities;

namespace LibrarySystem.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User> AddAsync(User user);
}
