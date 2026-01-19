using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Users;

public sealed record UserRolesRequest([param: Required] string[] Roles);
