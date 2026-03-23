using Microsoft.EntityFrameworkCore;
using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Infrastructure.Data;

namespace LibrarySystem.Infrastructure.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly LibraryDbContext _context;

    public LoanRepository(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Reader)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Loan>> GetAllAsync()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Reader)
            .ToListAsync();
    }

    public async Task<IEnumerable<Loan>> GetByReaderIdAsync(int readerId)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Reader)
            .Where(l => l.ReaderId == readerId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Loan>> GetOverdueAsync()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Reader)
            .Where(l => !l.ReturnDate.HasValue && l.DueDate < DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<Loan> AddAsync(Loan loan)
    {
        var entry = await _context.Loans.AddAsync(loan);
        return entry.Entity;
    }

    public Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        return Task.CompletedTask;
    }
}
