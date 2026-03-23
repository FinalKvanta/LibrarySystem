using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Exceptions;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Application.DTOs;

namespace LibrarySystem.Application.Services;

public class LoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IReaderRepository _readerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoanService(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        IReaderRepository readerRepository,
        IUnitOfWork unitOfWork)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _readerRepository = readerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoanDto> LendBookAsync(int bookId, int readerId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId)
            ?? throw new NotFoundException("Book", bookId);
        var reader = await _readerRepository.GetByIdAsync(readerId)
            ?? throw new NotFoundException("Reader", readerId);

        if (!reader.IsActive)
            throw new DomainException("Reader account is not active.");

        if (book.AvailableCopies <= 0)
            throw new DomainException($"No available copies of '{book.Title}'.");

        book.AvailableCopies--;
        await _bookRepository.UpdateAsync(book);

        var loan = new Loan
        {
            BookId = bookId,
            ReaderId = readerId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14)
        };

        var created = await _loanRepository.AddAsync(loan);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(created, book.Title, reader.FullName);
    }

    public async Task<LoanDto> ReturnBookAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId)
            ?? throw new NotFoundException("Loan", loanId);

        if (loan.ReturnDate.HasValue)
            throw new DomainException("This book has already been returned.");

        loan.ReturnDate = DateTime.UtcNow;
        await _loanRepository.UpdateAsync(loan);

        var book = await _bookRepository.GetByIdAsync(loan.BookId)
            ?? throw new NotFoundException("Book", loan.BookId);
        book.AvailableCopies++;
        await _bookRepository.UpdateAsync(book);

        var reader = await _readerRepository.GetByIdAsync(loan.ReaderId);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(loan, book.Title, reader?.FullName ?? "Unknown");
    }

    public async Task<List<LoanDto>> GetByReaderAsync(int readerId)
    {
        var reader = await _readerRepository.GetByIdAsync(readerId)
            ?? throw new NotFoundException("Reader", readerId);

        var loans = await _loanRepository.GetByReaderIdAsync(readerId);
        var result = new List<LoanDto>();

        foreach (var loan in loans)
        {
            var book = await _bookRepository.GetByIdAsync(loan.BookId);
            result.Add(MapToDto(loan, book?.Title ?? "Unknown", reader.FullName));
        }

        return result;
    }

    public async Task<List<LoanDto>> GetOverdueAsync()
    {
        var loans = await _loanRepository.GetOverdueAsync();
        var result = new List<LoanDto>();

        foreach (var loan in loans)
        {
            var book = await _bookRepository.GetByIdAsync(loan.BookId);
            var reader = await _readerRepository.GetByIdAsync(loan.ReaderId);
            result.Add(MapToDto(loan, book?.Title ?? "Unknown", reader?.FullName ?? "Unknown"));
        }

        return result;
    }

    private static LoanDto MapToDto(Loan loan, string bookTitle, string readerName) => new()
    {
        Id = loan.Id,
        BookId = loan.BookId,
        ReaderId = loan.ReaderId,
        BookTitle = bookTitle,
        ReaderName = readerName,
        LoanDate = loan.LoanDate,
        DueDate = loan.DueDate,
        ReturnDate = loan.ReturnDate,
        IsOverdue = !loan.ReturnDate.HasValue && loan.DueDate < DateTime.UtcNow
    };
}
