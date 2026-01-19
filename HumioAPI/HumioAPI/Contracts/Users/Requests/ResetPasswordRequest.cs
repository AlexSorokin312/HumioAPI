using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record ResetPasswordRequest(
    [param: Required, MinLength(6)] string NewPassword);
