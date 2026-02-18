using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record ResetPasswordByTokenRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required] string Token,
    [param: Required, MinLength(6)] string NewPassword);
