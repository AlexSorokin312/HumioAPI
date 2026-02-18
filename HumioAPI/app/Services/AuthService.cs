using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HumioAPI.Data;
using HumioAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HumioAPI.Services;

public sealed class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext,
        IOptions<JwtOptions> jwtOptions)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<(bool Success, string[] Errors, AuthTokens? Tokens)> LoginAsync(
        string email,
        string password,
        string? deviceKey,
        string? platform,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return (false, new[] { "Invalid credentials." }, null);
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, password);
        if (!validPassword)
        {
            return (false, new[] { "Invalid credentials." }, null);
        }

        return await IssueTokensAsync(user, deviceKey, platform, cancellationToken);
    }

    public async Task<(bool Success, string[] Errors, AuthTokens? Tokens)> IssueTokensAsync(
        ApplicationUser user,
        string? deviceKey,
        string? platform,
        CancellationToken cancellationToken = default)
    {
        var deviceId = await ResolveDeviceIdAsync(user.Id, deviceKey, platform, cancellationToken);
        var (accessToken, accessExpiresAt) = await CreateAccessTokenAsync(user, cancellationToken);
        var (refreshEntity, refreshToken) = await CreateRefreshTokenAsync(user.Id, deviceId, cancellationToken);

        return (true, Array.Empty<string>(), new AuthTokens(accessToken, refreshToken, accessExpiresAt));
    }

    public async Task<(bool Success, string[] Errors, AuthTokens? Tokens)> RefreshAsync(
        string refreshToken,
        string? deviceKey,
        string? platform,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return (false, new[] { "Refresh token is required." }, null);
        }

        var now = DateTimeOffset.UtcNow;
        var tokenHash = HashToken(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return (false, new[] { "Refresh token not found." }, null);
        }

        if (storedToken.RevokedAt is not null)
        {
            return (false, new[] { "Refresh token revoked." }, null);
        }

        if (storedToken.ExpiresAt <= now)
        {
            return (false, new[] { "Refresh token expired." }, null);
        }

        if (!string.IsNullOrWhiteSpace(deviceKey) && !string.IsNullOrWhiteSpace(platform))
        {
            var device = await _dbContext.Devices
                .FirstOrDefaultAsync(d => d.DeviceKey == deviceKey, cancellationToken);

            if (device is null || storedToken.DeviceId != device.Id)
            {
                return (false, new[] { "Refresh token device mismatch." }, null);
            }
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
        if (user is null)
        {
            return (false, new[] { "User not found." }, null);
        }

        var (accessToken, accessExpiresAt) = await CreateAccessTokenAsync(user, cancellationToken);
        var (newRefreshEntity, newRefreshToken) = await CreateRefreshTokenAsync(user.Id, storedToken.DeviceId, cancellationToken);

        storedToken.RevokedAt = now;
        storedToken.ReplacedByTokenId = newRefreshEntity.Id;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (true, Array.Empty<string>(), new AuthTokens(accessToken, newRefreshToken, accessExpiresAt));
    }

    public async Task<(bool Success, string[] Errors)> LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return (false, new[] { "Refresh token is required." });
        }

        var tokenHash = HashToken(refreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null)
        {
            return (true, Array.Empty<string>());
        }

        if (storedToken.RevokedAt is null)
        {
            storedToken.RevokedAt = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return (true, Array.Empty<string>());
    }

    private async Task<long?> ResolveDeviceIdAsync(
        long userId,
        string? deviceKey,
        string? platform,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceKey) || string.IsNullOrWhiteSpace(platform))
        {
            return null;
        }

        var trimmedKey = deviceKey.Trim();
        var trimmedPlatform = platform.Trim();

        var device = await _dbContext.Devices
            .FirstOrDefaultAsync(d => d.DeviceKey == trimmedKey, cancellationToken);

        if (device is null)
        {
            device = new Device
            {
                DeviceKey = trimmedKey,
                Platform = trimmedPlatform,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _dbContext.Devices.Add(device);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else if (!string.Equals(device.Platform, trimmedPlatform, StringComparison.OrdinalIgnoreCase))
        {
            device.Platform = trimmedPlatform;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var linkExists = await _dbContext.UsersDevices
            .AnyAsync(ud => ud.UserId == userId && ud.DeviceId == device.Id && ud.RevokedAt == null, cancellationToken);

        if (!linkExists)
        {
            _dbContext.UsersDevices.Add(new UserDevice
            {
                UserId = userId,
                DeviceId = device.Id,
                LinkedAt = DateTimeOffset.UtcNow
            });
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return device.Id;
    }

    private async Task<(string Token, DateTimeOffset ExpiresAt)> CreateAccessTokenAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_jwtOptions.Key))
        {
            throw new InvalidOperationException("JWT key is not configured.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        if (!string.IsNullOrWhiteSpace(user.Name))
        {
            claims.Add(new Claim("name", user.Name));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(Math.Max(1, _jwtOptions.AccessTokenMinutes));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var encoded = new JwtSecurityTokenHandler().WriteToken(token);
        return (encoded, expiresAt);
    }

    private async Task<(RefreshToken Entity, string Token)> CreateRefreshTokenAsync(
        long userId,
        long? deviceId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var tokenValue = GenerateTokenValue();
        var tokenHash = HashToken(tokenValue);

        var refreshToken = new RefreshToken
        {
            UserId = userId,
            DeviceId = deviceId,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = now.AddDays(Math.Max(1, _jwtOptions.RefreshTokenDays))
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (refreshToken, tokenValue);
    }

    private static string GenerateTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes);
        return token.Replace("+", "-").Replace("/", "_").Replace("=", string.Empty);
    }

    private static string HashToken(string token)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
