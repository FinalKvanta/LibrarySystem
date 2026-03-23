using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Exceptions;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Application.DTOs;
using LibrarySystem.Application.Validators;

namespace LibrarySystem.Application.Services;

public class BookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BookService(IBookRepository bookRepository, IUnitOfWork unitOfWork)
    {
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<BookDto>> GetAllAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(MapToDto).ToList();
    }

    public async Task<BookDto> GetByIdAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Book", id);
        return MapToDto(book);
    }

    public async Task<List<BookDto>> SearchAsync(SearchCriteriaDto criteria)
    {
        var books = await _bookRepository.SearchAsync(criteria.Title, criteria.Author, criteria.Genre);
        return books.Select(MapToDto).ToList();
    }

    public async Task<BookDto> AddAsync(BookDto dto)
    {
        BookValidator.Validate(dto);

        var book = new Book
        {
            Title = dto.Title,
            Author = dto.Author,
            ISBN = dto.ISBN,
            Year = dto.Year,
            Genre = dto.Genre,
            TotalCopies = dto.TotalCopies,
            AvailableCopies = dto.TotalCopies,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _bookRepository.AddAsync(book);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(created);
    }

    public async Task UpdateAsync(BookDto dto)
    {
        BookValidator.Validate(dto);

        var book = await _bookRepository.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("Book", dto.Id);

        book.Title = dto.Title;
        book.Author = dto.Author;
        book.ISBN = dto.ISBN;
        book.Year = dto.Year;
        book.Genre = dto.Genre;
        book.TotalCopies = dto.TotalCopies;
        book.AvailableCopies = dto.AvailableCopies;
        book.UpdatedAt = DateTime.UtcNow;

        await _bookRepository.UpdateAsync(book);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var book = await _bookRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Book", id);

        await _bookRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    private static BookDto MapToDto(Book book) => new()
    {
        Id = book.Id,
        Title = book.Title,
        Author = book.Author,
        ISBN = book.ISBN,
        Year = book.Year,
        Genre = book.Genre,
        TotalCopies = book.TotalCopies,
        AvailableCopies = book.AvailableCopies,
        CreatedAt = book.CreatedAt,
        UpdatedAt = book.UpdatedAt
    };
}
