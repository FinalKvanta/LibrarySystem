using System.Runtime.Serialization;

namespace LibrarySystem.Application.DTOs;

[DataContract]
public class SearchCriteriaDto
{
    [DataMember(Order = 1)]
    public string? Title { get; set; }

    [DataMember(Order = 2)]
    public string? Author { get; set; }

    [DataMember(Order = 3)]
    public string? Genre { get; set; }
}
