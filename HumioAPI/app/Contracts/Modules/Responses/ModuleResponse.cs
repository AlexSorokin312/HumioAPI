namespace HumioAPI.Contracts.Modules;

public sealed record ModuleResponse(
    long Id,
    string Name,
    string? Description,
    ProductSetResponse[] Products);
