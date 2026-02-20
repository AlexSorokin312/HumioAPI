using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Modules;

public sealed record UpsertModuleLocalizationRequest(
    [param: Required] string LanguageCode,
    [param: Required] string Name,
    string? Description);
