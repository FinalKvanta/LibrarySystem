using CoreWCF;
using LibrarySystem.Application.Contracts;
using LibrarySystem.Application.DTOs;
using LibrarySystem.Application.Services;
using LibrarySystem.Core.Enums;
using LibrarySystem.Core.Exceptions;

namespace LibrarySystem.API.Services;

public class LibraryServiceImpl : ILibraryService
{
    private readonly AuthService _authService;
    private readonly BookService _bookService;
    private readonly ReaderService _readerService;
    private readonly LoanService _loanService;
    private readonly StatsService _statsService;

    public LibraryServiceImpl(
        AuthService authService,
        BookService bookService,
        ReaderService readerService,
        LoanService loanService,
        StatsService statsService)
    {
        _authService = authService;
        _bookService = bookService;
        _readerService = readerService;
        _loanService = loanService;
        _statsService = statsService;
    }

    public AuthTokenDto Authenticate(string username, string password)
    {
        try
        {
            return _authService.AuthenticateAsync(username, password).GetAwaiter().GetResult();
        }
        catch (DomainException ex)
        {
            throw new FaultException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new FaultException($"Authentication error: {ex.Message}");
        }
    }

    public List<BookDto> GetAllBooks(string token)
    {
        return ExecuteSecured(token, new[] { UserRole.Reader, UserRole.Librarian, UserRole.Admin },
            () => _bookService.GetAllAsync().GetAwaiter().GetResult());
    }

    public BookDto GetBookById(string token, int id)
    {
        return ExecuteSecured(token, new[] { UserRole.Reader, UserRole.Librarian, UserRole.Admin },
            () => _bookService.GetByIdAsync(id).GetAwaiter().GetResult());
    }

    public List<BookDto> SearchBooks(string token, SearchCriteriaDto criteria)
    {
        return ExecuteSecured(token, new[] { UserRole.Reader, UserRole.Librarian, UserRole.Admin },
            () => _bookService.SearchAsync(criteria).GetAwaiter().GetResult());
    }

    public BookDto AddBook(string token, BookDto book)
    {
        return ExecuteSecured(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _bookService.AddAsync(book).GetAwaiter().GetResult());
    }

    public void UpdateBook(string token, BookDto book)
    {
        ExecuteSecuredAction(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _bookService.UpdateAsync(book).GetAwaiter().GetResult());
    }

    public void DeleteBook(string token, int id)
    {
        ExecuteSecuredAction(token, new[] { UserRole.Admin },
            () => _bookService.DeleteAsync(id).GetAwaiter().GetResult());
    }

    public List<ReaderDto> GetAllReaders(string token)
    {
        return ExecuteSecured(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _readerService.GetAllAsync().GetAwaiter().GetResult());
    }

    public ReaderDto GetReaderById(string token, int id)
    {
        return ExecuteSecured(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _readerService.GetByIdAsync(id).GetAwaiter().GetResult());
    }

    public ReaderDto RegisterReader(string token, ReaderDto reader)
    {
        return ExecuteSecured(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _readerService.RegisterAsync(reader).GetAwaiter().GetResult());
    }

    public LoanDto LendBook(string token, int bookId, int readerId)
    {
        return ExecuteSecured(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _loanService.LendBookAsync(bookId, readerId).GetAwaiter().GetResult());
    }

    public LoanDto ReturnBook(string token, int loanId)
    {
        return ExecuteSecured(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _loanService.ReturnBookAsync(loanId).GetAwaiter().GetResult());
    }

    public List<LoanDto> GetLoansByReader(string token, int readerId)
    {
        return ExecuteSecured(token, new[] { UserRole.Reader, UserRole.Librarian, UserRole.Admin },
            () => _loanService.GetByReaderAsync(readerId).GetAwaiter().GetResult());
    }

    public List<LoanDto> GetOverdueLoans(string token)
    {
        return ExecuteSecured(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _loanService.GetOverdueAsync().GetAwaiter().GetResult());
    }

    public LibraryStatsDto GetStatistics(string token)
    {
        return ExecuteSecured(token, new[] { UserRole.Librarian, UserRole.Admin },
            () => _statsService.GetStatisticsAsync().GetAwaiter().GetResult());
    }

    private T ExecuteSecured<T>(string token, UserRole[] allowedRoles, Func<T> action)
    {
        try
        {
            _authService.RequireRole(token, allowedRoles);
            return action();
        }
        catch (AccessDeniedException ex)
        {
            throw new FaultException(ex.Message);
        }
        catch (NotFoundException ex)
        {
            throw new FaultException(ex.Message);
        }
        catch (DomainException ex)
        {
            throw new FaultException(ex.Message);
        }
        catch (FaultException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new FaultException($"Internal error: {ex.Message}");
        }
    }

    private void ExecuteSecuredAction(string token, UserRole[] allowedRoles, Action action)
    {
        ExecuteSecured<object?>(token, allowedRoles, () => { action(); return null; });
    }
}
