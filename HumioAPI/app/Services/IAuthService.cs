using HumioAPI.Entities;

namespace HumioAPI.Services;

public interface IAuthService
{
    Task<(bool Success, string[] Errors, AuthTokens? Tokens)> LoginAsync(
        string email,
        string password,
        string? deviceKey,
        string? platform,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, AuthTokens? Tokens)> IssueTokensAsync(
        ApplicationUser user,
        string? deviceKey,
        string? platform,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, AuthTokens? Tokens)> RefreshAsync(
        string refreshToken,
        string? deviceKey,
        string? platform,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors)> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
