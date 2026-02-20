namespace HumioAPI.Contracts.Modules;

public sealed record ModuleResponse(
    long Id,
    string Name,
    ProductSetResponse[] Products);
