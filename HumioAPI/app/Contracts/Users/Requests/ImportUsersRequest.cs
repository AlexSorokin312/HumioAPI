using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record ImportUsersRequest(
    [param: Required] string FilePath,
    long? ModuleId);
