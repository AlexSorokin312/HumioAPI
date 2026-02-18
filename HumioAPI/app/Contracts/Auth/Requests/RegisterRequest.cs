using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Auth.Requests;

public sealed record RegisterRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required, MinLength(6)] string Password,
    string? Name,
    string? DeviceKey,
    string? Platform);
