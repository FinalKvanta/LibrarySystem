using Microsoft.EntityFrameworkCore;
using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Infrastructure.Data;

namespace LibrarySystem.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly LibraryDbContext _context;

    public BookRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> GetByIdAsync(int id)
    {
        return await _context.Books.FindAsync(id);
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _context.Books.ToListAsync();
    }

    public async Task<IEnumerable<Book>> SearchAsync(string? title, string? author, string? genre)
    {
        var query = _context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
            query = query.Where(b => b.Title.Contains(title));

        if (!string.IsNullOrWhiteSpace(author))
            query = query.Where(b => b.Author.Contains(author));

        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(b => b.Genre.Contains(genre));

        return await query.ToListAsync();
    }

    public async Task<Book> AddAsync(Book book)
    {
        var entry = await _context.Books.AddAsync(book);
        return entry.Entity;
    }

    public Task UpdateAsync(Book book)
    {
        _context.Books.Update(book);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
            _context.Books.Remove(book);
    }
}
