using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record ResetPasswordByTokenRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required, RegularExpression(@"^\d{4}$", ErrorMessage = "Token must be a 4-digit code.")] string Token,
    [param: Required, MinLength(6)] string NewPassword);
