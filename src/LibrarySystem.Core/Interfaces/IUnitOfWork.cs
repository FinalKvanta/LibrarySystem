namespace LibrarySystem.Core.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync();
}
