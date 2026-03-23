using System.Runtime.Serialization;

namespace LibrarySystem.Application.DTOs;

[DataContract]
public class AuthTokenDto
{
    [DataMember(Order = 1)]
    public string Token { get; set; } = string.Empty;

    [DataMember(Order = 2)]
    public string Username { get; set; } = string.Empty;

    [DataMember(Order = 3)]
    public string Role { get; set; } = string.Empty;

    [DataMember(Order = 4)]
    public DateTime ExpiresAt { get; set; }
}
