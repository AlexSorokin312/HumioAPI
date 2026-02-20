using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Modules;

public sealed record UpdateModuleRequest(
    [param: Required] string Name);
