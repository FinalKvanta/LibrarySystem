using LibrarySystem.Core.Exceptions;
using LibrarySystem.Application.DTOs;

namespace LibrarySystem.Application.Validators;

public static class BookValidator
{
    public static void Validate(BookDto book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
            throw new DomainException("Book title is required.");

        if (string.IsNullOrWhiteSpace(book.Author))
            throw new DomainException("Book author is required.");

        if (string.IsNullOrWhiteSpace(book.ISBN))
            throw new DomainException("Book ISBN is required.");

        if (book.Year < 1000 || book.Year > DateTime.Now.Year + 1)
            throw new DomainException($"Book year must be between 1000 and {DateTime.Now.Year + 1}.");

        if (book.TotalCopies < 0)
            throw new DomainException("Total copies cannot be negative.");
    }
}
