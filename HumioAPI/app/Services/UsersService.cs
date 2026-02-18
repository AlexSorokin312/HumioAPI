using HumioAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HumioAPI.Services;

public sealed class UsersService : IUsersService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IEmailSender _emailSender;

    public UsersService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
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
}
