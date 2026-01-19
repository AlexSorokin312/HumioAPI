using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record PatchUserRequest(
    [param: EmailAddress] string? Email,
    string? Name,
    string? PhoneNumber);
