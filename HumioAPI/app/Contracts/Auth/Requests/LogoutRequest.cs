using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Auth.Requests;

public sealed record LogoutRequest(
    [param: Required] string RefreshToken);
