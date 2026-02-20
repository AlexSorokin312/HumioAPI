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

    Task<(bool Success, string[] Errors, string? Country, bool NotFound)> UpdateUserCountryAsync(
        long id,
        string? country,
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

    Task<(bool Success, string[] Errors, int DeletedCount)> DeleteAllAsync(
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, DateTimeOffset? EndsAt, bool UserNotFound, bool ModuleNotFound)> GrantModuleAccessByAdminAsync(
        long adminId,
        long targetUserId,
        long moduleId,
        int days,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, DateTimeOffset? EndsAt, bool Enabled, bool UserNotFound, bool ModuleNotFound)> SetModuleAccessByAdminAsync(
        long adminId,
        long targetUserId,
        long moduleId,
        bool enabled,
        DateTimeOffset? endsAt,
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

    Task<DateTimeOffset?> GetSubscriptionEndDateAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<long, DateTimeOffset?>> GetSubscriptionEndDatesAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken = default);

    Task<string?> GetUserCountryAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<long, string?>> GetUserCountriesByUserIdsAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken = default);

    Task<UserModuleInfo[]> GetUserModulesAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<long, UserModuleInfo[]>> GetUserModulesByUserIdsAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken = default);

    Task<UserPurchasesInfo> GetUserPurchasesInfoAsync(
        long userId,
        CancellationToken cancellationToken = default);

    Task<Dictionary<long, UserPurchasesInfo>> GetUserPurchasesInfoByUserIdsAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, UsersImportResult? Result)> ImportFromExportAsync(
        string mode,
        int? count,
        DateOnly? createdAfterDate,
        string? email,
        long? moduleId,
        CancellationToken cancellationToken = default);
}

public sealed record UsersImportResult(
    int Total,
    int Created,
    int Existing,
    int Failed,
    int SubscriptionsApplied,
    int PurchasesApplied,
    long ModuleId);

public sealed record UserModuleInfo(
    long Id,
    string Name,
    DateTimeOffset? EndsAt,
    bool GrantedByAdmin);

public sealed record UserPurchasesInfo(
    int PurchasesCount,
    int TotalPurchasedAmountCents);
