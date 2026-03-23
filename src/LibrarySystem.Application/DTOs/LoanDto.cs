using System.Runtime.Serialization;

namespace LibrarySystem.Application.DTOs;

[DataContract]
public class LoanDto
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public int BookId { get; set; }

    [DataMember(Order = 3)]
    public int ReaderId { get; set; }

    [DataMember(Order = 4)]
    public string BookTitle { get; set; } = string.Empty;

    [DataMember(Order = 5)]
    public string ReaderName { get; set; } = string.Empty;

    [DataMember(Order = 6)]
    public DateTime LoanDate { get; set; }

    [DataMember(Order = 7)]
    public DateTime DueDate { get; set; }

    [DataMember(Order = 8)]
    public DateTime? ReturnDate { get; set; }

    [DataMember(Order = 9)]
    public bool IsOverdue { get; set; }
}
