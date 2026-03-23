using System.Runtime.Serialization;

namespace LibrarySystem.Application.DTOs;

[DataContract]
public class LibraryStatsDto
{
    [DataMember(Order = 1)]
    public int TotalBooks { get; set; }

    [DataMember(Order = 2)]
    public int TotalReaders { get; set; }

    [DataMember(Order = 3)]
    public int ActiveLoans { get; set; }

    [DataMember(Order = 4)]
    public int OverdueLoans { get; set; }

    [DataMember(Order = 5)]
    public int TotalLoans { get; set; }

    [DataMember(Order = 6)]
    public int AvailableBooks { get; set; }
}
