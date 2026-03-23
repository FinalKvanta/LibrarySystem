using LibrarySystem.Core.Interfaces;
using LibrarySystem.Application.DTOs;

namespace LibrarySystem.Application.Services;

public class StatsService
{
    private readonly IBookRepository _bookRepository;
    private readonly IReaderRepository _readerRepository;
    private readonly ILoanRepository _loanRepository;

    public StatsService(
        IBookRepository bookRepository,
        IReaderRepository readerRepository,
        ILoanRepository loanRepository)
    {
        _bookRepository = bookRepository;
        _readerRepository = readerRepository;
        _loanRepository = loanRepository;
    }

    public async Task<LibraryStatsDto> GetStatisticsAsync()
    {
        var books = (await _bookRepository.GetAllAsync()).ToList();
        var readers = (await _readerRepository.GetAllAsync()).ToList();
        var loans = (await _loanRepository.GetAllAsync()).ToList();
        var overdue = (await _loanRepository.GetOverdueAsync()).ToList();

        return new LibraryStatsDto
        {
            TotalBooks = books.Count,
            TotalReaders = readers.Count,
            TotalLoans = loans.Count,
            ActiveLoans = loans.Count(l => !l.ReturnDate.HasValue),
            OverdueLoans = overdue.Count,
            AvailableBooks = books.Sum(b => b.AvailableCopies)
        };
    }
}
