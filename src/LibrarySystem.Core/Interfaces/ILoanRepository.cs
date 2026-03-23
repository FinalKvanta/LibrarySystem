using LibrarySystem.Core.Entities;

namespace LibrarySystem.Core.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id);
    Task<IEnumerable<Loan>> GetAllAsync();
    Task<IEnumerable<Loan>> GetByReaderIdAsync(int readerId);
    Task<IEnumerable<Loan>> GetOverdueAsync();
    Task<Loan> AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
}
