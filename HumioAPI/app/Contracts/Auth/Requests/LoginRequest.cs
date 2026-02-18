using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Auth.Requests;

public sealed record LoginRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required] string Password,
    string? DeviceKey,
    string? Platform);
