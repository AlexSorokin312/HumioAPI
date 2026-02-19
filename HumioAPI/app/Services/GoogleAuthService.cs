using Google.Apis.Auth;
using HumioAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HumioAPI.Services;

public sealed class GoogleAuthService : IGoogleAuthService
{
    private const string ProviderName = "Google";
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GoogleAuthOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;

    public GoogleAuthService(
        UserManager<ApplicationUser> userManager,
        IOptions<GoogleAuthOptions> options,
        IHttpClientFactory httpClientFactory)
    {
        _userManager = userManager;
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(bool Success, string[] Errors, ApplicationUser? User, bool IsNewUser)> AuthenticateByCodeAsync(
        string code,
        string? redirectUri,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId) ||
            string.IsNullOrWhiteSpace(_options.ClientSecret) ||
            string.IsNullOrWhiteSpace(_options.TokenUri))
        {
            return (false, new[] { "Google auth is not configured." }, null, false);
        }

        var resolvedRedirectUri = string.IsNullOrWhiteSpace(redirectUri)
            ? _options.RedirectUri
            : redirectUri;

        if (string.IsNullOrWhiteSpace(resolvedRedirectUri))
        {
            return (false, new[] { "Redirect URI is required." }, null, false);
        }

        var (idToken, exchangeError) = await ExchangeCodeForIdTokenAsync(code, resolvedRedirectUri, cancellationToken);
        if (string.IsNullOrWhiteSpace(idToken))
        {
            var message = string.IsNullOrWhiteSpace(exchangeError)
                ? "Failed to exchange authorization code."
                : $"Failed to exchange authorization code. {exchangeError}";

            return (false, new[] { message }, null, false);
        }

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _options.ClientId }
            });
        }
        catch (Exception ex)
        {
            return (false, new[] { $"Invalid Google token: {ex.Message}" }, null, false);
        }

        if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Email))
        {
            return (false, new[] { "Google token does not contain required claims." }, null, false);
        }

        var existingByLogin = await _userManager.FindByLoginAsync(ProviderName, payload.Subject);
        if (existingByLogin is not null)
        {
            return (true, Array.Empty<string>(), existingByLogin, false);
        }

        var user = await _userManager.FindByEmailAsync(payload.Email);
        var isNewUser = false;

        if (user is null)
        {
            user = new ApplicationUser
            {
                Email = payload.Email,
                UserName = payload.Email,
                Name = payload.Name,
                CreatedAt = DateTimeOffset.UtcNow,
                EmailConfirmed = payload.EmailVerified
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var createErrors = createResult.Errors.Select(error => error.Description).ToArray();
                return (false, createErrors, null, false);
            }

            isNewUser = true;
        }
        else if (string.IsNullOrWhiteSpace(user.Name) && !string.IsNullOrWhiteSpace(payload.Name))
        {
            user.Name = payload.Name;
            await _userManager.UpdateAsync(user);
        }

        var loginInfo = new UserLoginInfo(ProviderName, payload.Subject, ProviderName);
        var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
        if (!addLoginResult.Succeeded)
        {
            var loginErrors = addLoginResult.Errors.Select(error => error.Description).ToArray();
            return (false, loginErrors, null, false);
        }

        return (true, Array.Empty<string>(), user, isNewUser);
    }

    private async Task<(string? IdToken, string? Error)> ExchangeCodeForIdTokenAsync(
        string code,
        string redirectUri,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient();
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        });

        using var response = await client.PostAsync(_options.TokenUri, content, cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return (null, $"Google token endpoint returned {(int)response.StatusCode}: {payload}");
        }

        try
        {
            using var json = JsonDocument.Parse(payload);
            if (json.RootElement.TryGetProperty("id_token", out var idTokenElement))
            {
                return (idTokenElement.GetString(), null);
            }
        }
        catch (JsonException)
        {
            return (null, "Google token endpoint returned invalid JSON.");
        }

        return (null, "Google token endpoint response does not contain id_token.");
    }
}
