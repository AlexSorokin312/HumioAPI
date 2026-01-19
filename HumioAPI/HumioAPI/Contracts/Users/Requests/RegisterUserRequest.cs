using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record RegisterUserRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required, MinLength(6)] string Password,
    string? Name);
