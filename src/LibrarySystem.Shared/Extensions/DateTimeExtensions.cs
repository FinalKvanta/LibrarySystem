namespace LibrarySystem.Shared.Extensions;

public static class DateTimeExtensions
{
    public static bool IsOverdue(this DateTime dueDate)
    {
        return dueDate < DateTime.UtcNow;
    }

    public static bool IsOverdue(this DateTime dueDate, DateTime? returnDate)
    {
        if (returnDate.HasValue)
            return false;
        return dueDate < DateTime.UtcNow;
    }

    public static int DaysOverdue(this DateTime dueDate)
    {
        if (!dueDate.IsOverdue())
            return 0;
        return (DateTime.UtcNow - dueDate).Days;
    }
}
