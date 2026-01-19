using HumioAPI.Contracts.Auth.Requests;
using HumioAPI.Contracts.Auth.Responses;
using HumioAPI.Contracts.Users;
using HumioAPI.Entities;
using HumioAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HumioAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;

    public AuthController(IGoogleAuthService googleAuthService)
    {
        _googleAuthService = googleAuthService;
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

        return Ok(new GoogleAuthResponse(ToResponse(user), isNewUser));
    }

    private static UserResponse ToResponse(ApplicationUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.Name, user.CreatedAt, user.LastSeen);
}
