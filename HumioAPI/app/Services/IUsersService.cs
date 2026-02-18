using HumioAPI.Entities;

namespace HumioAPI.Services;

public interface IUsersService
{
    Task<(bool Success, string[] Errors, ApplicationUser? User)> RegisterAsync(
        string email,
        string password,
        string? name,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, ApplicationUser? User, bool NotFound)> UpdatePartialAsync(
        long id,
        string? email,
        string? name,
        string? phoneNumber,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, bool NotFound)> ResetPasswordAdminAsync(
        long id,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors)> SendResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, bool NotFound)> ResetPasswordByTokenAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    Task<ApplicationUser?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<(int Total, ApplicationUser[] Items)> ListAsync(
        int skip,
        int take,
        string? email,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, bool NotFound)> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, string[]? Roles, bool NotFound)> GetRolesAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, string[]? Roles, bool NotFound)> AddRolesAsync(
        long id,
        string[] roles,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, string[]? Roles, bool NotFound)> RemoveRolesAsync(
        long id,
        string[] roles,
        CancellationToken cancellationToken = default);
}
