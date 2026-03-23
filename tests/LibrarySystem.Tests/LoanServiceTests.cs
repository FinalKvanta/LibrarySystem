using Microsoft.EntityFrameworkCore;
using LibrarySystem.Application.Services;
using LibrarySystem.Core.Exceptions;
using LibrarySystem.Infrastructure.Data;
using LibrarySystem.Infrastructure.Repositories;

namespace LibrarySystem.Tests;

public class LoanServiceTests
{
    private static LibraryDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new LibraryDbContext(options);
        SeedData.Initialize(context);
        return context;
    }

    private static LoanService CreateService(LibraryDbContext context)
    {
        var bookRepo = new BookRepository(context);
        var readerRepo = new ReaderRepository(context);
        var loanRepo = new LoanRepository(context);
        var uow = new UnitOfWork(context);
        return new LoanService(loanRepo, bookRepo, readerRepo, uow);
    }

    [Fact]
    public async Task LendBook_ValidRequest_CreatesLoan()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        // Book 3 (The Master and Margarita) has 4 available copies, Reader 1 exists
        var result = await service.LendBookAsync(3, 1);

        Assert.NotEqual(0, result.Id);
        Assert.Equal(3, result.BookId);
        Assert.Equal(1, result.ReaderId);
        Assert.Equal("The Master and Margarita", result.BookTitle);
        Assert.Equal("Ivan Petrov", result.ReaderName);
        Assert.False(result.IsOverdue);
    }

    [Fact]
    public async Task LendBook_NonExistingBook_ThrowsNotFoundException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.LendBookAsync(999, 1));
    }

    [Fact]
    public async Task LendBook_NoCopiesAvailable_ThrowsDomainException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        // Book 5 (Dead Souls) has 1 copy, lend it first
        await service.LendBookAsync(5, 1);

        // Now try to lend again - no copies left
        await Assert.ThrowsAsync<DomainException>(() => service.LendBookAsync(5, 2));
    }

    [Fact]
    public async Task ReturnBook_ValidLoan_ReturnsSuccessfully()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        // Loan 1 exists from seed data
        var result = await service.ReturnBookAsync(1);

        Assert.NotNull(result.ReturnDate);
        Assert.False(result.IsOverdue);
    }

    [Fact]
    public async Task ReturnBook_AlreadyReturned_ThrowsDomainException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        await service.ReturnBookAsync(1);

        await Assert.ThrowsAsync<DomainException>(() => service.ReturnBookAsync(1));
    }

    [Fact]
    public async Task GetOverdueLoans_ReturnsOverdueOnly()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        // Loan 2 from seed data is overdue (DueDate = UtcNow - 6 days)
        var overdueLoans = await service.GetOverdueAsync();

        Assert.NotEmpty(overdueLoans);
        Assert.All(overdueLoans, l => Assert.True(l.IsOverdue));
    }

    [Fact]
    public async Task GetByReader_ExistingReader_ReturnsLoans()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        // Reader 1 has loan 1 from seed data
        var loans = await service.GetByReaderAsync(1);

        Assert.NotEmpty(loans);
        Assert.All(loans, l => Assert.Equal(1, l.ReaderId));
    }
}
