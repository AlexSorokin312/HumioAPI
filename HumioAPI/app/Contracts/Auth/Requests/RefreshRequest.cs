using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Auth.Requests;

public sealed record RefreshRequest(
    [param: Required] string RefreshToken,
    string? DeviceKey,
    string? Platform);
