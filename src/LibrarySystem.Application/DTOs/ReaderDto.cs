using System.Runtime.Serialization;

namespace LibrarySystem.Application.DTOs;

[DataContract]
public class ReaderDto
{
    [DataMember(Order = 1)]
    public int Id { get; set; }

    [DataMember(Order = 2)]
    public string FullName { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string Email { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public string Phone { get; set; } = string.Empty;

    [DataMember(Order = 5)]
    public DateTime RegisteredDate { get; set; }

    [DataMember(Order = 6)]
    public bool IsActive { get; set; }
}
