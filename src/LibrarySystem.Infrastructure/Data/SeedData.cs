using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Enums;
using LibrarySystem.Application.Services;

namespace LibrarySystem.Infrastructure.Data;

public static class SeedData
{
    public static void Initialize(LibraryDbContext context)
    {
        if (context.Books.Any())
            return;

        // Seed books
        var books = new List<Book>
        {
            new() { Title = "War and Peace", Author = "Leo Tolstoy", ISBN = "978-0-14-044793-4", Year = 1869, Genre = "Novel", TotalCopies = 3, AvailableCopies = 2, CreatedAt = DateTime.UtcNow },
            new() { Title = "Crime and Punishment", Author = "Fyodor Dostoevsky", ISBN = "978-0-14-044913-6", Year = 1866, Genre = "Novel", TotalCopies = 2, AvailableCopies = 1, CreatedAt = DateTime.UtcNow },
            new() { Title = "The Master and Margarita", Author = "Mikhail Bulgakov", ISBN = "978-0-14-118014-6", Year = 1967, Genre = "Fantasy", TotalCopies = 4, AvailableCopies = 4, CreatedAt = DateTime.UtcNow },
            new() { Title = "Anna Karenina", Author = "Leo Tolstoy", ISBN = "978-0-14-303500-8", Year = 1877, Genre = "Novel", TotalCopies = 2, AvailableCopies = 2, CreatedAt = DateTime.UtcNow },
            new() { Title = "Dead Souls", Author = "Nikolai Gogol", ISBN = "978-0-14-044854-2", Year = 1842, Genre = "Satire", TotalCopies = 1, AvailableCopies = 1, CreatedAt = DateTime.UtcNow }
        };
        context.Books.AddRange(books);
        context.SaveChanges();

        // Seed readers
        var readers = new List<Reader>
        {
            new() { FullName = "Ivan Petrov", Email = "ivan@example.com", Phone = "+7-900-111-2233", RegisteredDate = DateTime.UtcNow, IsActive = true },
            new() { FullName = "Maria Sidorova", Email = "maria@example.com", Phone = "+7-900-444-5566", RegisteredDate = DateTime.UtcNow, IsActive = true },
            new() { FullName = "Alexei Ivanov", Email = "alexei@example.com", Phone = "+7-900-777-8899", RegisteredDate = DateTime.UtcNow, IsActive = true }
        };
        context.Readers.AddRange(readers);
        context.SaveChanges();

        // Seed loans
        var loans = new List<Loan>
        {
            new() { BookId = 1, ReaderId = 1, LoanDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow.AddDays(4) },
            new() { BookId = 2, ReaderId = 2, LoanDate = DateTime.UtcNow.AddDays(-20), DueDate = DateTime.UtcNow.AddDays(-6) }
        };
        context.Loans.AddRange(loans);
        context.SaveChanges();

        // Seed users (password is the same as username for demo)
        var users = new List<User>
        {
            new() { Username = "admin", PasswordHash = AuthService.HashPassword("admin"), Role = UserRole.Admin, CreatedAt = DateTime.UtcNow },
            new() { Username = "librarian", PasswordHash = AuthService.HashPassword("librarian"), Role = UserRole.Librarian, CreatedAt = DateTime.UtcNow },
            new() { Username = "reader", PasswordHash = AuthService.HashPassword("reader"), Role = UserRole.Reader, CreatedAt = DateTime.UtcNow }
        };
        context.Users.AddRange(users);
        context.SaveChanges();
    }
}
