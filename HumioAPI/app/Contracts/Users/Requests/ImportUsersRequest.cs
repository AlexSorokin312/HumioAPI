using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record ImportUsersRequest(
    [param: Required] string Mode,
    int? Count,
    DateOnly? CreatedAfterDate,
    [param: EmailAddress] string? Email,
    long? ModuleId);
