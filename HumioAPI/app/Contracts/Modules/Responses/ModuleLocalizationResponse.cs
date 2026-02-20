namespace HumioAPI.Contracts.Modules;

public sealed record ModuleLocalizationResponse(
    long Id,
    long ModuleId,
    string LanguageCode,
    string Name,
    string? Description);
