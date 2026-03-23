using System.ServiceModel;
using LibrarySystem.Application.Contracts;
using LibrarySystem.Application.DTOs;

Console.WriteLine("=== LibrarySystem WCF Client ===\n");

// --- HTTP Binding ---
Console.WriteLine(">>> Testing via BasicHttpBinding (HTTP) <<<\n");
await TestViaBinding(
    new BasicHttpBinding(),
    new EndpointAddress("http://localhost:5000/LibraryService.svc"));

// --- TCP Binding ---
Console.WriteLine("\n>>> Testing via NetTcpBinding (TCP) <<<\n");
await TestViaBinding(
    new NetTcpBinding(SecurityMode.None),
    new EndpointAddress("net.tcp://localhost:8090/LibraryService.svc"));

Console.WriteLine("\n=== All tests completed ===");

static Task TestViaBinding(System.ServiceModel.Channels.Binding binding, EndpointAddress address)
{
    var factory = new ChannelFactory<ILibraryService>(binding, address);
    var client = factory.CreateChannel();

    try
    {
        // 1. Authentication
        Console.WriteLine("--- Authentication ---");

        Console.Write("Authenticating as admin... ");
        var adminToken = client.Authenticate("admin", "admin");
        Console.WriteLine($"OK. Token received, role: {adminToken.Role}");

        Console.Write("Authenticating as librarian... ");
        var librarianToken = client.Authenticate("librarian", "librarian");
        Console.WriteLine($"OK. Token received, role: {librarianToken.Role}");

        Console.Write("Authenticating as reader... ");
        var readerToken = client.Authenticate("reader", "reader");
        Console.WriteLine($"OK. Token received, role: {readerToken.Role}");

        // Test invalid credentials
        Console.Write("Testing invalid credentials... ");
        try
        {
            client.Authenticate("admin", "wrongpassword");
            Console.WriteLine("FAIL - should have thrown");
        }
        catch (FaultException ex)
        {
            Console.WriteLine($"OK. Got expected error: {ex.Message}");
        }

        // 2. Books CRUD
        Console.WriteLine("\n--- Books ---");

        Console.Write("Getting all books... ");
        var books = client.GetAllBooks(adminToken.Token);
        Console.WriteLine($"OK. Found {books.Count} books.");

        Console.Write("Getting book by ID (1)... ");
        var book = client.GetBookById(adminToken.Token, 1);
        Console.WriteLine($"OK. Title: {book.Title}, Author: {book.Author}");

        Console.Write("Searching books by author 'Tolstoy'... ");
        var searchResults = client.SearchBooks(adminToken.Token, new SearchCriteriaDto { Author = "Tolstoy" });
        Console.WriteLine($"OK. Found {searchResults.Count} books by Tolstoy.");

        Console.Write("Adding a new book... ");
        var newBook = client.AddBook(librarianToken.Token, new BookDto
        {
            Title = "Eugene Onegin",
            Author = "Alexander Pushkin",
            ISBN = "978-0-14-044894-8",
            Year = 1833,
            Genre = "Poetry",
            TotalCopies = 3
        });
        Console.WriteLine($"OK. Added book ID: {newBook.Id}, Title: {newBook.Title}");

        Console.Write("Updating book... ");
        newBook.TotalCopies = 5;
        newBook.AvailableCopies = 5;
        client.UpdateBook(librarianToken.Token, newBook);
        Console.WriteLine("OK. Updated total copies to 5.");

        Console.Write("Deleting book (admin only)... ");
        client.DeleteBook(adminToken.Token, newBook.Id);
        Console.WriteLine("OK. Book deleted.");

        // 3. Readers
        Console.WriteLine("\n--- Readers ---");

        Console.Write("Getting all readers... ");
        var readers = client.GetAllReaders(librarianToken.Token);
        Console.WriteLine($"OK. Found {readers.Count} readers.");

        Console.Write("Getting reader by ID (1)... ");
        var reader = client.GetReaderById(librarianToken.Token, 1);
        Console.WriteLine($"OK. Name: {reader.FullName}, Email: {reader.Email}");

        Console.Write("Registering new reader... ");
        var newReader = client.RegisterReader(librarianToken.Token, new ReaderDto
        {
            FullName = "Natalia Romanova",
            Email = "natalia@example.com",
            Phone = "+7-900-000-1234"
        });
        Console.WriteLine($"OK. Registered reader ID: {newReader.Id}, Name: {newReader.FullName}");

        // 4. Loans
        Console.WriteLine("\n--- Loans ---");

        Console.Write("Lending book (ID=3) to reader (ID=1)... ");
        var loan = client.LendBook(librarianToken.Token, 3, 1);
        Console.WriteLine($"OK. Loan ID: {loan.Id}, Book: {loan.BookTitle}, Due: {loan.DueDate:yyyy-MM-dd}");

        Console.Write("Getting loans by reader (ID=1)... ");
        var readerLoans = client.GetLoansByReader(librarianToken.Token, 1);
        Console.WriteLine($"OK. Found {readerLoans.Count} loans for reader.");

        Console.Write("Getting overdue loans... ");
        var overdueLoans = client.GetOverdueLoans(librarianToken.Token);
        Console.WriteLine($"OK. Found {overdueLoans.Count} overdue loans.");

        Console.Write("Returning book (loan ID={0})... ", loan.Id);
        var returnedLoan = client.ReturnBook(librarianToken.Token, loan.Id);
        Console.WriteLine($"OK. Returned on: {returnedLoan.ReturnDate:yyyy-MM-dd HH:mm}");

        // 5. Statistics
        Console.WriteLine("\n--- Statistics ---");

        Console.Write("Getting library statistics... ");
        var stats = client.GetStatistics(adminToken.Token);
        Console.WriteLine($"OK.");
        Console.WriteLine($"  Total books: {stats.TotalBooks}");
        Console.WriteLine($"  Total readers: {stats.TotalReaders}");
        Console.WriteLine($"  Total loans: {stats.TotalLoans}");
        Console.WriteLine($"  Active loans: {stats.ActiveLoans}");
        Console.WriteLine($"  Overdue loans: {stats.OverdueLoans}");
        Console.WriteLine($"  Available books: {stats.AvailableBooks}");

        // 6. Role-based access control tests
        Console.WriteLine("\n--- Access Control ---");

        Console.Write("Reader trying to add a book (should fail)... ");
        try
        {
            client.AddBook(readerToken.Token, new BookDto
            {
                Title = "Test", Author = "Test", ISBN = "000", Year = 2020, Genre = "Test", TotalCopies = 1
            });
            Console.WriteLine("FAIL - should have thrown");
        }
        catch (FaultException ex)
        {
            Console.WriteLine($"OK. Access denied: {ex.Message}");
        }

        Console.Write("Reader trying to get readers (should fail)... ");
        try
        {
            client.GetAllReaders(readerToken.Token);
            Console.WriteLine("FAIL - should have thrown");
        }
        catch (FaultException ex)
        {
            Console.WriteLine($"OK. Access denied: {ex.Message}");
        }

        Console.Write("Librarian trying to delete a book (should fail)... ");
        try
        {
            client.DeleteBook(librarianToken.Token, 1);
            Console.WriteLine("FAIL - should have thrown");
        }
        catch (FaultException ex)
        {
            Console.WriteLine($"OK. Access denied: {ex.Message}");
        }

        Console.Write("Reader can view books (should succeed)... ");
        var readerBooks = client.GetAllBooks(readerToken.Token);
        Console.WriteLine($"OK. Reader sees {readerBooks.Count} books.");

        Console.WriteLine("\n--- Binding test complete ---");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nERROR: {ex.Message}");
        if (ex.InnerException != null)
            Console.WriteLine($"  Inner: {ex.InnerException.Message}");
    }
    finally
    {
        if (client is IDisposable disposable)
            disposable.Dispose();
    }

    return Task.CompletedTask;
}
