using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record GrantModuleAccessRequest(
    [param: Range(1, 36500)] int Days);
