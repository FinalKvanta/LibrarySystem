namespace LibrarySystem.Core.Entities;

public class Loan
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public DateTime LoanDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public Book Book { get; set; } = null!;
    public Reader Reader { get; set; } = null!;
}
