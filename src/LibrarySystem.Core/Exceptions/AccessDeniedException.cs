namespace LibrarySystem.Core.Exceptions;

public class AccessDeniedException : DomainException
{
    public AccessDeniedException(string message = "Access denied. Insufficient permissions.")
        : base(message) { }
}
