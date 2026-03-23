using Microsoft.EntityFrameworkCore;
using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Infrastructure.Data;

namespace LibrarySystem.Infrastructure.Repositories;

public class ReaderRepository : IReaderRepository
{
    private readonly LibraryDbContext _context;

    public ReaderRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Reader?> GetByIdAsync(int id)
    {
        return await _context.Readers.FindAsync(id);
    }

    public async Task<IEnumerable<Reader>> GetAllAsync()
    {
        return await _context.Readers.ToListAsync();
    }

    public async Task<Reader> AddAsync(Reader reader)
    {
        var entry = await _context.Readers.AddAsync(reader);
        return entry.Entity;
    }

    public Task UpdateAsync(Reader reader)
    {
        _context.Readers.Update(reader);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var reader = await _context.Readers.FindAsync(id);
        if (reader != null)
            _context.Readers.Remove(reader);
    }
}
