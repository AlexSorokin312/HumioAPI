using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Auth.Requests;

public sealed record GoogleAuthRequest(
    [param: Required] string Code,
    string? RedirectUri);
