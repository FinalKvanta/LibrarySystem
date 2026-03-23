using Microsoft.EntityFrameworkCore;
using LibrarySystem.Application.DTOs;
using LibrarySystem.Application.Services;
using LibrarySystem.Core.Exceptions;
using LibrarySystem.Infrastructure.Data;
using LibrarySystem.Infrastructure.Repositories;

namespace LibrarySystem.Tests;

public class BookServiceTests
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

    private static BookService CreateService(LibraryDbContext context)
    {
        var repo = new BookRepository(context);
        var uow = new UnitOfWork(context);
        return new BookService(repo, uow);
    }

    [Fact]
    public async Task AddBook_ValidBook_ReturnsCreatedBook()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var dto = new BookDto
        {
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "978-0-00-000000-0",
            Year = 2024,
            Genre = "Test",
            TotalCopies = 5
        };

        var result = await service.AddAsync(dto);

        Assert.NotEqual(0, result.Id);
        Assert.Equal("Test Book", result.Title);
        Assert.Equal("Test Author", result.Author);
        Assert.Equal(5, result.AvailableCopies);
    }

    [Fact]
    public async Task GetBook_ExistingId_ReturnsBook()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var result = await service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("War and Peace", result.Title);
        Assert.Equal("Leo Tolstoy", result.Author);
    }

    [Fact]
    public async Task GetBook_NonExistingId_ThrowsNotFoundException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999));
    }

    [Fact]
    public async Task SearchBooks_ByAuthor_ReturnsMatchingBooks()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var criteria = new SearchCriteriaDto { Author = "Tolstoy" };
        var results = await service.SearchAsync(criteria);

        Assert.Equal(2, results.Count);
        Assert.All(results, b => Assert.Contains("Tolstoy", b.Author));
    }

    [Fact]
    public async Task UpdateBook_ValidData_UpdatesSuccessfully()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var book = await service.GetByIdAsync(1);
        book.Title = "Updated Title";

        await service.UpdateAsync(book);

        var updated = await service.GetByIdAsync(1);
        Assert.Equal("Updated Title", updated.Title);
    }

    [Fact]
    public async Task DeleteBook_ExistingId_RemovesBook()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        await service.DeleteAsync(1);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(1));
    }

    [Fact]
    public async Task AddBook_EmptyTitle_ThrowsDomainException()
    {
        using var context = CreateContext();
        var service = CreateService(context);

        var dto = new BookDto
        {
            Title = "",
            Author = "Author",
            ISBN = "978-0-00-000000-0",
            Year = 2024,
            Genre = "Test",
            TotalCopies = 1
        };

        await Assert.ThrowsAsync<DomainException>(() => service.AddAsync(dto));
    }
}
