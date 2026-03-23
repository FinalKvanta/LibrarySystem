using System.ServiceModel;
using LibrarySystem.Application.DTOs;

namespace LibrarySystem.Application.Contracts;

[ServiceContract(Namespace = "http://library.example.com/services")]
public interface ILibraryService
{
    // Authentication
    [OperationContract]
    AuthTokenDto Authenticate(string username, string password);

    // Books
    [OperationContract]
    List<BookDto> GetAllBooks(string token);

    [OperationContract]
    BookDto GetBookById(string token, int id);

    [OperationContract]
    List<BookDto> SearchBooks(string token, SearchCriteriaDto criteria);

    [OperationContract]
    BookDto AddBook(string token, BookDto book);

    [OperationContract]
    void UpdateBook(string token, BookDto book);

    [OperationContract]
    void DeleteBook(string token, int id);

    // Readers
    [OperationContract]
    List<ReaderDto> GetAllReaders(string token);

    [OperationContract]
    ReaderDto GetReaderById(string token, int id);

    [OperationContract]
    ReaderDto RegisterReader(string token, ReaderDto reader);

    // Loans
    [OperationContract]
    LoanDto LendBook(string token, int bookId, int readerId);

    [OperationContract]
    LoanDto ReturnBook(string token, int loanId);

    [OperationContract]
    List<LoanDto> GetLoansByReader(string token, int readerId);

    [OperationContract]
    List<LoanDto> GetOverdueLoans(string token);

    // Statistics
    [OperationContract]
    LibraryStatsDto GetStatistics(string token);
}
