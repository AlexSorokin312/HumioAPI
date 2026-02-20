using System.Text.Json;
using HumioAPI.Data;
using HumioAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HumioAPI.Services;

public sealed class UsersService : IUsersService
{
    private const long LegacyPurchasedProductId = 1;
    private const string LegacyImportProvider = "legacy-export";
    private const string LegacyExportBaseUrl = "https://humio.space/api/UserData/export";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEmailSender _emailSender;
    private readonly AppDbContext _dbContext;
    private readonly IHttpClientFactory _httpClientFactory;

    public UsersService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEmailSender emailSender,
        AppDbContext dbContext,
        IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _dbContext = dbContext;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(bool Success, string[] Errors, ApplicationUser? User)> RegisterAsync(
        string email,
        string password,
        string? name,
        CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Email = email,
            UserName = email,
            Name = name,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => error.Description).ToArray();
            return (false, errors, null);
        }

        return (true, Array.Empty<string>(), user);
    }

    public async Task<(bool Success, string[] Errors, ApplicationUser? User, bool NotFound)> UpdatePartialAsync(
        long id,
        string? email,
        string? name,
        string? phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return (false, Array.Empty<string>(), null, true);
        }

        var errors = new List<string>();

        if (email is not null)
        {
            var emailResult = await _userManager.SetEmailAsync(user, email);
            if (!emailResult.Succeeded)
            {
                errors.AddRange(emailResult.Errors.Select(error => error.Description));
            }

            var userNameResult = await _userManager.SetUserNameAsync(user, email);
            if (!userNameResult.Succeeded)
            {
                errors.AddRange(userNameResult.Errors.Select(error => error.Description));
            }
        }

        if (phoneNumber is not null)
        {
            var phoneResult = await _userManager.SetPhoneNumberAsync(user, phoneNumber);
            if (!phoneResult.Succeeded)
            {
                errors.AddRange(phoneResult.Errors.Select(error => error.Description));
            }
        }

        if (name is not null)
        {
            user.Name = name;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                errors.AddRange(updateResult.Errors.Select(error => error.Description));
            }
        }

        if (errors.Count > 0)
        {
            return (false, errors.ToArray(), user, false);
        }

        return (true, Array.Empty<string>(), user, false);
    }

    public async Task<(bool Success, string[] Errors, bool NotFound)> ResetPasswordAdminAsync(
        long id,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return (false, Array.Empty<string>(), true);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => error.Description).ToArray();
            return (false, errors, false);
        }

        return (true, Array.Empty<string>(), false);
    }

    public async Task<(bool Success, string[] Errors)> SendResetTokenAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return (true, Array.Empty<string>());
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var subject = "Password reset code";
        var body = $"Your reset code: {token}";

        try
        {
            await _emailSender.SendAsync(email, subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, new[] { ex.Message });
        }

        return (true, Array.Empty<string>());
    }

    public async Task<(bool Success, string[] Errors, bool NotFound)> ResetPasswordByTokenAsync(
        string email,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return (false, Array.Empty<string>(), true);
        }

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => error.Description).ToArray();
            return (false, errors, false);
        }

        return (true, Array.Empty<string>(), false);
    }

    public Task<ApplicationUser?> GetByIdAsync(long id, CancellationToken cancellationToken = default) =>
        _userManager.FindByIdAsync(id.ToString());

    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        _userManager.FindByEmailAsync(email);

    public async Task<(int Total, ApplicationUser[] Items)> ListAsync(
        int skip,
        int take,
        string? email,
        CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(u => u.Email != null && u.Email.Contains(email));
        }

        var total = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (total, users.ToArray());
    }

    public async Task<(bool Success, string[] Errors, bool NotFound)> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return (false, Array.Empty<string>(), true);
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => error.Description).ToArray();
            return (false, errors, false);
        }

        return (true, Array.Empty<string>(), false);
    }

    public async Task<(bool Success, string[] Errors, int DeletedCount)> DeleteAllAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        if (users.Count == 0)
        {
            return (true, Array.Empty<string>(), 0);
        }

        var errors = new List<string>();
        var deletedCount = 0;

        foreach (var user in users)
        {
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(error => $"[{user.Email ?? user.Id.ToString()}] {error.Description}"));
                continue;
            }

            deletedCount++;
        }

        return errors.Count > 0
            ? (false, errors.Distinct().ToArray(), deletedCount)
            : (true, Array.Empty<string>(), deletedCount);
    }

    public async Task<(bool Success, string[] Errors, string[]? Roles, bool NotFound)> GetRolesAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return (false, Array.Empty<string>(), null, true);
        }

        var roles = await _userManager.GetRolesAsync(user);
        return (true, Array.Empty<string>(), roles.ToArray(), false);
    }

    public async Task<(bool Success, string[] Errors, string[]? Roles, bool NotFound)> AddRolesAsync(
        long id,
        string[] roles,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return (false, Array.Empty<string>(), null, true);
        }

        var missingRoles = await GetMissingRolesAsync(roles);
        if (missingRoles.Length > 0)
        {
            return (false, new[] { $"Roles not found: {string.Join(", ", missingRoles)}" }, null, false);
        }

        var result = await _userManager.AddToRolesAsync(user, roles);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => error.Description).ToArray();
            return (false, errors, null, false);
        }

        var updatedRoles = await _userManager.GetRolesAsync(user);
        return (true, Array.Empty<string>(), updatedRoles.ToArray(), false);
    }

    public async Task<(bool Success, string[] Errors, string[]? Roles, bool NotFound)> RemoveRolesAsync(
        long id,
        string[] roles,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return (false, Array.Empty<string>(), null, true);
        }

        var missingRoles = await GetMissingRolesAsync(roles);
        if (missingRoles.Length > 0)
        {
            return (false, new[] { $"Roles not found: {string.Join(", ", missingRoles)}" }, null, false);
        }

        var result = await _userManager.RemoveFromRolesAsync(user, roles);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(error => error.Description).ToArray();
            return (false, errors, null, false);
        }

        var updatedRoles = await _userManager.GetRolesAsync(user);
        return (true, Array.Empty<string>(), updatedRoles.ToArray(), false);
    }

    public async Task<DateTimeOffset?> GetSubscriptionEndDateAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserModuleAccess
            .Where(access => access.UserId == userId)
            .Select(access => (DateTimeOffset?)access.EndsAt)
            .OrderByDescending(endsAt => endsAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<long, DateTimeOffset?>> GetSubscriptionEndDatesAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<long, DateTimeOffset?>();
        }

        var values = await _dbContext.UserModuleAccess
            .Where(access => ids.Contains(access.UserId))
            .GroupBy(access => access.UserId)
            .Select(group => new
            {
                group.Key,
                EndsAt = group.Max(item => item.EndsAt)
            })
            .ToListAsync(cancellationToken);

        return values.ToDictionary(item => item.Key, item => (DateTimeOffset?)item.EndsAt);
    }

    public async Task<string?> GetUserCountryAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .Where(profile => profile.UserId == userId)
            .Select(profile => profile.Country)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<long, string?>> GetUserCountriesByUserIdsAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<long, string?>();
        }

        var values = await _dbContext.UserProfiles
            .Where(profile => ids.Contains(profile.UserId))
            .Select(profile => new { profile.UserId, profile.Country })
            .ToListAsync(cancellationToken);

        return values.ToDictionary(item => item.UserId, item => item.Country);
    }

    public async Task<UserModuleInfo[]> GetUserModulesAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var modules = await _dbContext.UserModuleAccess
            .Where(access => access.UserId == userId)
            .Select(access => new { access.ModuleId, access.Module.Name })
            .Distinct()
            .OrderBy(module => module.ModuleId)
            .ToListAsync(cancellationToken);

        return modules
            .Select(module => new UserModuleInfo(module.ModuleId, module.Name))
            .ToArray();
    }

    public async Task<Dictionary<long, UserModuleInfo[]>> GetUserModulesByUserIdsAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<long, UserModuleInfo[]>();
        }

        var values = await _dbContext.UserModuleAccess
            .Where(access => ids.Contains(access.UserId))
            .Select(access => new
            {
                access.UserId,
                Module = new UserModuleInfo(access.ModuleId, access.Module.Name)
            })
            .ToListAsync(cancellationToken);

        return values
            .GroupBy(item => item.UserId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(item => item.Module)
                    .Distinct()
                    .OrderBy(module => module.Id)
                    .ToArray());
    }

    public async Task<UserPurchasesInfo> GetUserPurchasesInfoAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        var paidPurchases = _dbContext.Purchases
            .Where(purchase => purchase.UserId == userId && purchase.Status == PaymentStatus.Paid);

        var purchasesCount = await paidPurchases.CountAsync(cancellationToken);
        var totalPurchasedAmountCents = await paidPurchases
            .Select(purchase => (int?)purchase.AmountCents)
            .SumAsync(cancellationToken) ?? 0;

        return new UserPurchasesInfo(purchasesCount, totalPurchasedAmountCents);
    }

    public async Task<Dictionary<long, UserPurchasesInfo>> GetUserPurchasesInfoByUserIdsAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<long, UserPurchasesInfo>();
        }

        var values = await _dbContext.Purchases
            .Where(purchase => ids.Contains(purchase.UserId) && purchase.Status == PaymentStatus.Paid)
            .GroupBy(purchase => purchase.UserId)
            .Select(group => new
            {
                group.Key,
                PurchasesCount = group.Count(),
                TotalPurchasedAmountCents = group.Sum(purchase => purchase.AmountCents)
            })
            .ToListAsync(cancellationToken);

        return values.ToDictionary(
            item => item.Key,
            item => new UserPurchasesInfo(item.PurchasesCount, item.TotalPurchasedAmountCents));
    }

    public async Task<(bool Success, string[] Errors, UsersImportResult? Result)> ImportFromExportAsync(
        string mode,
        int? count,
        DateOnly? createdAfterDate,
        string? email,
        long? moduleId,
        CancellationToken cancellationToken = default)
    {
        var (exportUri, validationError) = BuildExportUri(mode, count, createdAfterDate, email);
        if (validationError is not null)
        {
            return (false, new[] { validationError }, null);
        }

        var targetModuleId = await ResolveTargetModuleIdAsync(moduleId, cancellationToken);
        if (!targetModuleId.HasValue)
        {
            return (false, new[] { "Module not found. Pass moduleId or create at least one module." }, null);
        }

        var productExists = await _dbContext.Products.AnyAsync(
            product => product.Id == LegacyPurchasedProductId,
            cancellationToken);
        if (!productExists)
        {
            return (false, new[] { $"Product with id={LegacyPurchasedProductId} not found." }, null);
        }

        ExportPayload payload;
        try
        {
            payload = await ReadExportPayloadAsync(exportUri!, cancellationToken);
        }
        catch (Exception ex)
        {
            return (false, new[] { $"Cannot parse users export JSON from '{exportUri}': {ex.Message}" }, null);
        }

        var users = payload.Users ?? Array.Empty<ExportUserItem>();

        var errors = new List<string>();
        var total = users.Length;
        var created = 0;
        var existing = 0;
        var failed = 0;
        var subscriptionsApplied = 0;
        var purchasesApplied = 0;

        foreach (var item in users)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (item.AspNetUser is null || string.IsNullOrWhiteSpace(item.AspNetUser.Email))
            {
                failed++;
                errors.Add("Skipped user without email.");
                continue;
            }

            var userEmail = item.AspNetUser.Email.Trim();
            var user = await _userManager.FindByEmailAsync(userEmail);
            var createdNow = false;

            if (user is null)
            {
                user = new ApplicationUser
                {
                    Email = userEmail,
                    UserName = string.IsNullOrWhiteSpace(item.AspNetUser.UserName) ? userEmail : item.AspNetUser.UserName.Trim(),
                    Name = item.AspNetUser.Name,
                    CreatedAt = item.AspNetUser.RegistrationDateUtc ?? DateTimeOffset.UtcNow,
                    PasswordHash = string.IsNullOrWhiteSpace(item.AspNetUser.PasswordHash) ? null : item.AspNetUser.PasswordHash,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString("N"),
                    ConcurrencyStamp = Guid.NewGuid().ToString("N")
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    failed++;
                    errors.AddRange(createResult.Errors.Select(error => $"[{userEmail}] {error.Description}"));
                    continue;
                }

                created++;
                createdNow = true;
            }
            else
            {
                existing++;
            }

            var roles = NormalizeImportRoles(item.Roles);
            if (roles.Length > 0)
            {
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        var roleResult = await _roleManager.CreateAsync(new ApplicationRole { Name = role });
                        if (!roleResult.Succeeded)
                        {
                            errors.AddRange(roleResult.Errors.Select(error => $"[role:{role}] {error.Description}"));
                        }
                    }
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                var missingRoles = roles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();
                if (missingRoles.Length > 0)
                {
                    var addRolesResult = await _userManager.AddToRolesAsync(user, missingRoles);
                    if (!addRolesResult.Succeeded)
                    {
                        errors.AddRange(addRolesResult.Errors.Select(error => $"[{userEmail}] {error.Description}"));
                    }
                }
            }

            var subscriptionEndDate = item.UserData?.SubscriptionEndDate;
            if (subscriptionEndDate.HasValue && subscriptionEndDate.Value > DateTimeOffset.MinValue)
            {
                var access = await _dbContext.UserModuleAccess
                    .FirstOrDefaultAsync(
                        value => value.UserId == user.Id && value.ModuleId == targetModuleId.Value,
                        cancellationToken);

                if (access is null)
                {
                    _dbContext.UserModuleAccess.Add(new UserModuleAccess
                    {
                        UserId = user.Id,
                        ModuleId = targetModuleId.Value,
                        EndsAt = subscriptionEndDate.Value
                    });
                }
                else
                {
                    access.EndsAt = subscriptionEndDate.Value;
                }

                subscriptionsApplied++;
            }

            var country = item.UserData?.Country;
            if (!string.IsNullOrWhiteSpace(country))
            {
                var normalizedCountry = country.Trim().ToUpperInvariant();
                var profile = await _dbContext.UserProfiles
                    .FirstOrDefaultAsync(value => value.UserId == user.Id, cancellationToken);

                if (profile is null)
                {
                    _dbContext.UserProfiles.Add(new UserProfile
                    {
                        UserId = user.Id,
                        Country = normalizedCountry
                    });
                }
                else
                {
                    profile.Country = normalizedCountry;
                }
            }

            if (HasPurchases(item.Purchases))
            {
                var purchaseWasAdded = await EnsureLegacyPurchaseAsync(user, item, cancellationToken);
                if (purchaseWasAdded)
                {
                    purchasesApplied++;
                }
            }

            if (createdNow)
            {
                // Identity operations already persist user; only save extra linked data below.
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var importResult = new UsersImportResult(
            total,
            created,
            existing,
            failed,
            subscriptionsApplied,
            purchasesApplied,
            targetModuleId.Value);

        var distinctErrors = errors.Where(error => !string.IsNullOrWhiteSpace(error)).Distinct().Take(100).ToArray();
        return distinctErrors.Length > 0
            ? (false, distinctErrors, importResult)
            : (true, Array.Empty<string>(), importResult);
    }

    private static (Uri? Uri, string? Error) BuildExportUri(
        string mode,
        int? count,
        DateOnly? createdAfterDate,
        string? email)
    {
        if (string.IsNullOrWhiteSpace(mode))
        {
            return (null, "Mode is required. Allowed: all, count, after-date, by-email.");
        }

        var normalizedMode = mode.Trim().ToLowerInvariant();
        return normalizedMode switch
        {
            "all" => (new Uri($"{LegacyExportBaseUrl}/all"), null),
            "count" => BuildCountUri(count),
            "after-date" => BuildAfterDateUri(createdAfterDate),
            "by-email" => BuildByEmailUri(email),
            _ => (null, $"Unsupported mode '{mode}'. Allowed: all, count, after-date, by-email.")
        };
    }

    private static (Uri? Uri, string? Error) BuildCountUri(int? count)
    {
        if (!count.HasValue || count.Value <= 0)
        {
            return (null, "For mode 'count', pass a positive 'count'.");
        }

        return (new Uri($"{LegacyExportBaseUrl}/count?count={count.Value}"), null);
    }

    private static (Uri? Uri, string? Error) BuildAfterDateUri(DateOnly? createdAfterDate)
    {
        if (!createdAfterDate.HasValue)
        {
            return (null, "For mode 'after-date', pass 'createdAfterDate' in format YYYY-MM-DD.");
        }

        return (new Uri($"{LegacyExportBaseUrl}/after-date?createdAfterDate={createdAfterDate.Value:yyyy-MM-dd}"), null);
    }

    private static (Uri? Uri, string? Error) BuildByEmailUri(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return (null, "For mode 'by-email', pass 'email'.");
        }

        return (new Uri($"{LegacyExportBaseUrl}/by-email?email={Uri.EscapeDataString(email.Trim())}"), null);
    }

    private async Task<ExportPayload> ReadExportPayloadAsync(Uri sourceUri, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        using var response = await client.GetAsync(sourceUri, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ExportPayload>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        return payload ?? new ExportPayload();
    }

    private async Task<bool> EnsureLegacyPurchaseAsync(
        ApplicationUser user,
        ExportUserItem item,
        CancellationToken cancellationToken)
    {
        var alreadyHasPaidPurchase = await _dbContext.Purchases.AnyAsync(
            purchase => purchase.UserId == user.Id
                        && purchase.ProductId == LegacyPurchasedProductId
                        && purchase.Status == PaymentStatus.Paid,
            cancellationToken);
        if (alreadyHasPaidPurchase)
        {
            return false;
        }

        var latest = item.Purchases?
            .Where(purchase => purchase is not null)
            .OrderByDescending(purchase => purchase!.PurchaseDate ?? DateTimeOffset.MinValue)
            .FirstOrDefault();

        var paidAt = latest?.PurchaseDate ?? DateTimeOffset.UtcNow;
        var amountCents = latest?.Price is { } price
            ? Math.Max(0, (int)Math.Round(price * 100m, MidpointRounding.AwayFromZero))
            : 0;
        var days = CalculateDays(latest);

        var externalUserId = item.AspNetUser?.Id;
        var externalPurchaseId = latest?.Id is { } id
            ? id.ToString()
            : "no-purchase-id";
        var userKey = !string.IsNullOrWhiteSpace(externalUserId)
            ? externalUserId
            : (user.Email ?? user.Id.ToString());
        var providerPaymentId = $"legacy:{userKey}:{externalPurchaseId}:p1";

        var duplicate = await _dbContext.Purchases.AnyAsync(
            purchase => purchase.Provider == LegacyImportProvider
                        && purchase.ProviderPaymentId == providerPaymentId,
            cancellationToken);
        if (duplicate)
        {
            return false;
        }

        _dbContext.Purchases.Add(new Purchase
        {
            UserId = user.Id,
            ProductId = LegacyPurchasedProductId,
            AmountCents = amountCents,
            Currency = "RUB",
            Provider = LegacyImportProvider,
            ProviderPaymentId = providerPaymentId,
            Receipt = null,
            Status = PaymentStatus.Paid,
            Days = days,
            CreatedAt = paidAt,
            PurchasedAt = paidAt
        });

        return true;
    }

    private static bool HasPurchases(ExportPurchase[]? purchases) =>
        purchases is { Length: > 0 };

    private static int CalculateDays(ExportPurchase? purchase)
    {
        if (purchase?.PurchaseDate is null || purchase.SubscriptionEndDate is null)
        {
            return 30;
        }

        var days = (int)Math.Ceiling((purchase.SubscriptionEndDate.Value - purchase.PurchaseDate.Value).TotalDays);
        return Math.Max(1, days);
    }

    private async Task<string[]> GetMissingRolesAsync(IEnumerable<string> roles)
    {
        var missing = new List<string>();

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                missing.Add(role);
            }
        }

        return missing.ToArray();
    }

    private async Task<long?> ResolveTargetModuleIdAsync(long? requestedModuleId, CancellationToken cancellationToken)
    {
        if (requestedModuleId.HasValue)
        {
            var exists = await _dbContext.Modules.AnyAsync(module => module.Id == requestedModuleId.Value, cancellationToken);
            return exists ? requestedModuleId.Value : null;
        }

        var firstModuleId = await _dbContext.Modules
            .OrderBy(module => module.Id)
            .Select(module => (long?)module.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return firstModuleId;
    }

    private static string[] NormalizeImportRoles(JsonElement? rolesElement)
    {
        if (!rolesElement.HasValue)
        {
            return Array.Empty<string>();
        }

        var roles = new List<string>();
        var element = rolesElement.Value;

        if (element.ValueKind == JsonValueKind.String)
        {
            var singleRole = element.GetString();
            if (!string.IsNullOrWhiteSpace(singleRole))
            {
                roles.Add(singleRole.Trim());
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var role = item.GetString();
                if (!string.IsNullOrWhiteSpace(role))
                {
                    roles.Add(role.Trim());
                }
            }
        }

        return roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private sealed class ExportPayload
    {
        public ExportUserItem[]? Users { get; init; }
    }

    private sealed class ExportUserItem
    {
        public ExportAspNetUser? AspNetUser { get; init; }
        public JsonElement? Roles { get; init; }
        public ExportUserData? UserData { get; init; }
        public ExportPurchase[]? Purchases { get; init; }
    }

    private sealed class ExportAspNetUser
    {
        public string? Id { get; init; }
        public string? Name { get; init; }
        public string? UserName { get; init; }
        public string? Email { get; init; }
        public DateTimeOffset? RegistrationDateUtc { get; init; }
        public string? PasswordHash { get; init; }
    }

    private sealed class ExportUserData
    {
        public string? Country { get; init; }
        public DateTimeOffset? SubscriptionEndDate { get; init; }
    }

    private sealed class ExportPurchase
    {
        public long? Id { get; init; }
        public decimal? Price { get; init; }
        public DateTimeOffset? PurchaseDate { get; init; }
        public DateTimeOffset? SubscriptionEndDate { get; init; }
    }
}
