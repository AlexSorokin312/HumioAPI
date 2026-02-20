namespace HumioAPI.Contracts.Modules;

public sealed record ProductSetResponse(
    long Id,
    string Name,
    long[] ModuleIds);
