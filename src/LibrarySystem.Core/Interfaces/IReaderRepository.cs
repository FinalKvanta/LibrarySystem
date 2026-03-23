using LibrarySystem.Core.Entities;

namespace LibrarySystem.Core.Interfaces;

public interface IReaderRepository
{
    Task<Reader?> GetByIdAsync(int id);
    Task<IEnumerable<Reader>> GetAllAsync();
    Task<Reader> AddAsync(Reader reader);
    Task UpdateAsync(Reader reader);
    Task DeleteAsync(int id);
}
