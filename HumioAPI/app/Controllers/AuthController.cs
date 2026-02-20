using HumioAPI.Contracts.Auth.Requests;
using HumioAPI.Contracts.Auth.Responses;
using HumioAPI.Contracts.Users;
using HumioAPI.Entities;
using HumioAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HumioAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly IAuthService _authService;
    private readonly IUsersService _usersService;
    private readonly GoogleAuthOptions _googleOptions;

    public AuthController(
        IGoogleAuthService googleAuthService,
        IAuthService authService,
        IUsersService usersService,
        IOptions<GoogleAuthOptions> googleOptions)
    {
        _googleAuthService = googleAuthService;
        _authService = authService;
        _usersService = usersService;
        _googleOptions = googleOptions.Value;
    }

    [AllowAnonymous]
    [HttpGet("google/config")]
    public IActionResult GoogleConfig()
    {
        return Ok(new
        {
            clientId = _googleOptions.ClientId,
            redirectUri = _googleOptions.RedirectUri
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors, user) = await _usersService.RegisterAsync(
            request.Email,
            request.Password,
            request.Name);

        if (!success || user is null)
        {
            return BadRequest(new { errors });
        }

        var (tokenSuccess, tokenErrors, tokens) = await _authService.IssueTokensAsync(
            user,
            request.DeviceKey,
            request.Platform);

        if (!tokenSuccess || tokens is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors = tokenErrors });
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        return Ok(new AuthResponse(ToResponse(user, subscriptionEndDate), ToTokensResponse(tokens)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors, tokens) = await _authService.LoginAsync(
            request.Email,
            request.Password,
            request.DeviceKey,
            request.Platform);

        if (!success || tokens is null)
        {
            return BadRequest(new { errors });
        }

        var user = await _usersService.GetByEmailAsync(request.Email);
        if (user is null)
        {
            return BadRequest(new { errors = new[] { "User not found." } });
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        return Ok(new AuthResponse(ToResponse(user, subscriptionEndDate), ToTokensResponse(tokens)));
    }

    [HttpPost("google")]
    public async Task<IActionResult> Google([FromBody] GoogleAuthRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors, user, isNewUser) = await _googleAuthService.AuthenticateByCodeAsync(
            request.Code,
            request.RedirectUri);
        if (!success || user is null)
        {
            return BadRequest(new { errors });
        }

        var (tokenSuccess, tokenErrors, tokens) = await _authService.IssueTokensAsync(
            user,
            request.DeviceKey,
            request.Platform);

        if (!tokenSuccess || tokens is null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors = tokenErrors });
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        return Ok(new GoogleAuthResponse(ToResponse(user, subscriptionEndDate), isNewUser, ToTokensResponse(tokens)));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors, tokens) = await _authService.RefreshAsync(
            request.RefreshToken,
            request.DeviceKey,
            request.Platform);

        if (!success || tokens is null)
        {
            return BadRequest(new { errors });
        }

        return Ok(ToTokensResponse(tokens));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors) = await _authService.LogoutAsync(request.RefreshToken);
        if (!success)
        {
            return BadRequest(new { errors });
        }

        return NoContent();
    }

    private static UserResponse ToResponse(ApplicationUser user, DateTimeOffset? subscriptionEndDate) =>
        new(user.Id, user.Email ?? string.Empty, user.Name, user.CreatedAt, user.LastSeen, subscriptionEndDate);

    private static AuthTokensResponse ToTokensResponse(AuthTokens tokens) =>
        new(tokens.AccessToken, tokens.RefreshToken, tokens.AccessTokenExpiresAt);
}
