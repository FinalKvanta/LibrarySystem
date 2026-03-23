using System.Runtime.Serialization;

namespace LibrarySystem.Application.DTOs;

[DataContract]
public class BookDto
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public string Title { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string Author { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string ISBN { get; set; } = string.Empty;

    [DataMember(Order = 5)]
    public int Year { get; set; }

    [DataMember(Order = 6)]
    public string Genre { get; set; } = string.Empty;

    [DataMember(Order = 7)]
    public int TotalCopies { get; set; }

    [DataMember(Order = 8)]
    public int AvailableCopies { get; set; }

    [DataMember(Order = 9)]
    public DateTime CreatedAt { get; set; }

    [DataMember(Order = 10)]
    public DateTime? UpdatedAt { get; set; }
}
