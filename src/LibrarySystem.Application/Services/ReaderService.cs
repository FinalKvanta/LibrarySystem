using LibrarySystem.Core.Entities;
using LibrarySystem.Core.Exceptions;
using LibrarySystem.Core.Interfaces;
using LibrarySystem.Application.DTOs;

namespace LibrarySystem.Application.Services;

public class ReaderService
{
    private readonly IReaderRepository _readerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReaderService(IReaderRepository readerRepository, IUnitOfWork unitOfWork)
    {
        _readerRepository = readerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ReaderDto>> GetAllAsync()
    {
        var readers = await _readerRepository.GetAllAsync();
        return readers.Select(MapToDto).ToList();
    }

    public async Task<ReaderDto> GetByIdAsync(int id)
    {
        var reader = await _readerRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Reader", id);
        return MapToDto(reader);
    }

    public async Task<ReaderDto> RegisterAsync(ReaderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FullName))
            throw new DomainException("Reader full name is required.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new DomainException("Reader email is required.");

        var reader = new Reader
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            RegisteredDate = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _readerRepository.AddAsync(reader);
        await _unitOfWork.SaveChangesAsync();
        return MapToDto(created);
    }

    public async Task UpdateAsync(ReaderDto dto)
    {
        var reader = await _readerRepository.GetByIdAsync(dto.Id)
            ?? throw new NotFoundException("Reader", dto.Id);

        reader.FullName = dto.FullName;
        reader.Email = dto.Email;
        reader.Phone = dto.Phone;
        reader.IsActive = dto.IsActive;

        await _readerRepository.UpdateAsync(reader);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var reader = await _readerRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Reader", id);

        await _readerRepository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();
    }

    private static ReaderDto MapToDto(Reader reader) => new()
    {
        Id = reader.Id,
        FullName = reader.FullName,
        Email = reader.Email,
        Phone = reader.Phone,
        RegisteredDate = reader.RegisteredDate,
        IsActive = reader.IsActive
    };
}
