using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Modules;

public sealed record CreateModuleRequest(
    [param: Required] string Name);
