using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Linq;
using HumioAPI.Contracts.Users;
using HumioAPI.Entities;
using HumioAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HumioAPI.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var (success, errors, user) = await _usersService.RegisterAsync(
            request.Email,
            request.Password,
            request.Name);

        if (!success || user is null)
        {
            return BadRequest(new { errors });
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        return Created($"/api/users/{user.Id}", new UserCreatedResponse(user.Id, user.Email ?? string.Empty, user.Name, subscriptionEndDate));
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import([FromBody] ImportUsersRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors, result) = await _usersService.ImportFromExportAsync(
            request.Mode,
            request.Count,
            request.CreatedAfterDate,
            request.Email,
            request.ModuleId);
        if (result is null)
        {
            return BadRequest(new { errors });
        }

        var response = new UsersImportResponse(
            result.Total,
            result.Created,
            result.Existing,
            result.Failed,
            result.SubscriptionsApplied,
            result.PurchasesApplied,
            result.ModuleId);

        return success
            ? Ok(response)
            : BadRequest(new { errors, result = response });
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var (success, errors, notFound) = await _usersService.DeleteAsync(id);
        if (notFound)
        {
            return NotFound();
        }

        if (!success)
        {
            return BadRequest(new { errors });
        }

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        var (success, errors, deletedCount) = await _usersService.DeleteAllAsync();
        if (!success)
        {
            return BadRequest(new { errors, deletedCount });
        }

        return Ok(new { deletedCount });
    }

    [HttpPost("{id:long}/reset-password")]
    public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetPasswordRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors, notFound) = await _usersService.ResetPasswordAdminAsync(id, request.NewPassword);
        if (notFound)
        {
            return NotFound();
        }

        if (!success)
        {
            return BadRequest(new { errors });
        }

        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors) = await _usersService.SendResetTokenAsync(request.Email);
        if (!success)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { errors });
        }

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPasswordByToken([FromBody] ResetPasswordByTokenRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var (success, errors, notFound) = await _usersService.ResetPasswordByTokenAsync(
            request.Email,
            request.Token,
            request.NewPassword);

        if (notFound)
        {
            return NotFound();
        }

        if (!success)
        {
            return BadRequest(new { errors });
        }

        return NoContent();
    }

    [HttpPatch("{id:long}")]
    public async Task<IActionResult> Patch(long id, [FromBody] PatchUserRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        if (request.Email is null && request.Name is null && request.PhoneNumber is null)
        {
            return BadRequest(new { errors = new[] { "At least one field must be provided." } });
        }

        var (success, errors, user, notFound) = await _usersService.UpdatePartialAsync(
            id,
            request.Email,
            request.Name,
            request.PhoneNumber);

        if (notFound || user is null)
        {
            return NotFound();
        }

        if (!success)
        {
            return BadRequest(new { errors });
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        var modules = await _usersService.GetUserModulesAsync(user.Id);
        var purchasesInfo = await _usersService.GetUserPurchasesInfoAsync(user.Id);
        var country = await _usersService.GetUserCountryAsync(user.Id);
        return Ok(ToResponse(user, country, subscriptionEndDate, modules, purchasesInfo));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { errors = new[] { "User id claim is missing or invalid." } });
        }

        var user = await _usersService.GetByIdAsync(userId.Value);
        if (user is null)
        {
            return NotFound();
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        var modules = await _usersService.GetUserModulesAsync(user.Id);
        var purchasesInfo = await _usersService.GetUserPurchasesInfoAsync(user.Id);
        var country = await _usersService.GetUserCountryAsync(user.Id);
        return Ok(ToResponse(user, country, subscriptionEndDate, modules, purchasesInfo));
    }

    [Authorize]
    [HttpPatch("me")]
    public async Task<IActionResult> PatchMe([FromBody] PatchUserRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        if (request.Email is null && request.Name is null && request.PhoneNumber is null)
        {
            return BadRequest(new { errors = new[] { "At least one field must be provided." } });
        }

        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            return Unauthorized(new { errors = new[] { "User id claim is missing or invalid." } });
        }

        var (success, errors, user, notFound) = await _usersService.UpdatePartialAsync(
            userId.Value,
            request.Email,
            request.Name,
            request.PhoneNumber);

        if (notFound || user is null)
        {
            return NotFound();
        }

        if (!success)
        {
            return BadRequest(new { errors });
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        var modules = await _usersService.GetUserModulesAsync(user.Id);
        var purchasesInfo = await _usersService.GetUserPurchasesInfoAsync(user.Id);
        var country = await _usersService.GetUserCountryAsync(user.Id);
        return Ok(ToResponse(user, country, subscriptionEndDate, modules, purchasesInfo));
    }

    [HttpGet("{id:long}/roles")]
    public async Task<IActionResult> GetRoles(long id)
    {
        var (success, errors, roles, notFound) = await _usersService.GetRolesAsync(id);
        if (notFound)
        {
            return NotFound();
        }

        if (!success || roles is null)
        {
            return BadRequest(new { errors });
        }

        return Ok(new UserRolesResponse(roles));
    }

    [HttpPost("{id:long}/roles")]
    public async Task<IActionResult> AddRoles(long id, [FromBody] UserRolesRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var roles = NormalizeRoles(request.Roles);
        if (roles.Length == 0)
        {
            return BadRequest(new { errors = new[] { "At least one role must be provided." } });
        }

        var (success, errors, updatedRoles, notFound) = await _usersService.AddRolesAsync(id, roles);
        if (notFound)
        {
            return NotFound();
        }

        if (!success || updatedRoles is null)
        {
            return BadRequest(new { errors });
        }

        return Ok(new UserRolesResponse(updatedRoles));
    }

    [HttpDelete("{id:long}/roles")]
    public async Task<IActionResult> RemoveRoles(long id, [FromBody] UserRolesRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        var roles = NormalizeRoles(request.Roles);
        if (roles.Length == 0)
        {
            return BadRequest(new { errors = new[] { "At least one role must be provided." } });
        }

        var (success, errors, updatedRoles, notFound) = await _usersService.RemoveRolesAsync(id, roles);
        if (notFound)
        {
            return NotFound();
        }

        if (!success || updatedRoles is null)
        {
            return BadRequest(new { errors });
        }

        return Ok(new UserRolesResponse(updatedRoles));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var user = await _usersService.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        var modules = await _usersService.GetUserModulesAsync(user.Id);
        var purchasesInfo = await _usersService.GetUserPurchasesInfoAsync(user.Id);
        var country = await _usersService.GetUserCountryAsync(user.Id);
        return Ok(ToResponse(user, country, subscriptionEndDate, modules, purchasesInfo));
    }

    [HttpGet("by-email")]
    public async Task<IActionResult> GetByEmail([FromQuery][Required][EmailAddress] string email)
    {
        var user = await _usersService.GetByEmailAsync(email);
        if (user is null)
        {
            return NotFound();
        }

        var subscriptionEndDate = await _usersService.GetSubscriptionEndDateAsync(user.Id);
        var modules = await _usersService.GetUserModulesAsync(user.Id);
        var purchasesInfo = await _usersService.GetUserPurchasesInfoAsync(user.Id);
        var country = await _usersService.GetUserCountryAsync(user.Id);
        return Ok(ToResponse(user, country, subscriptionEndDate, modules, purchasesInfo));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? email = null)
    {
        if (skip < 0)
        {
            return BadRequest(new { errors = new[] { "skip must be >= 0." } });
        }

        if (take <= 0)
        {
            return BadRequest(new { errors = new[] { "take must be > 0." } });
        }

        var pageSize = Math.Min(take, 100);
        var (total, users) = await _usersService.ListAsync(skip, pageSize, email);
        var subscriptionDates = await _usersService.GetSubscriptionEndDatesAsync(users.Select(user => user.Id));
        var userCountries = await _usersService.GetUserCountriesByUserIdsAsync(users.Select(user => user.Id));
        var userModules = await _usersService.GetUserModulesByUserIdsAsync(users.Select(user => user.Id));
        var userPurchases = await _usersService.GetUserPurchasesInfoByUserIdsAsync(users.Select(user => user.Id));
        var items = users
            .Select(user => ToResponse(
                user,
                userCountries.TryGetValue(user.Id, out var country) ? country : null,
                subscriptionDates.TryGetValue(user.Id, out var endsAt) ? endsAt : null,
                userModules.TryGetValue(user.Id, out var modules) ? modules : Array.Empty<UserModuleInfo>(),
                userPurchases.TryGetValue(user.Id, out var purchasesInfo) ? purchasesInfo : new UserPurchasesInfo(0, 0)))
            .ToArray();
        return Ok(new UsersPageResponse(total, items));
    }

    private static UserResponse ToResponse(
        ApplicationUser user,
        string? country,
        DateTimeOffset? subscriptionEndDate,
        UserModuleInfo[] modules,
        UserPurchasesInfo purchasesInfo) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.Name,
            country,
            user.CreatedAt,
            user.LastSeen,
            subscriptionEndDate,
            modules.Select(module => new UserModuleResponse(module.Id, module.Name)).ToArray(),
            purchasesInfo.PurchasesCount,
            purchasesInfo.TotalPurchasedAmountCents);

    private static string[] NormalizeRoles(string[]? roles) =>
        roles?
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();

    private long? GetCurrentUserId()
    {
        var raw = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (long.TryParse(raw, out var id))
        {
            return id;
        }

        return null;
    }
}
