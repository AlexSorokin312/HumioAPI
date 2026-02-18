using HumioAPI.Entities;

namespace HumioAPI.Services;

public interface IGoogleAuthService
{
    Task<(bool Success, string[] Errors, ApplicationUser? User, bool IsNewUser)> AuthenticateByCodeAsync(
        string code,
        string? redirectUri,
        CancellationToken cancellationToken = default);
}
