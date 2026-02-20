using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Modules;

public sealed record CreateProductRequest(
    [param: Required] string Name,
    [param: MinLength(1)] long[] ModuleIds);
