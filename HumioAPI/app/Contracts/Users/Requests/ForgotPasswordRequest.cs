using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record ForgotPasswordRequest(
    [param: Required, EmailAddress] string Email);
