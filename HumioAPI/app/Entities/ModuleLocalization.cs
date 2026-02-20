namespace HumioAPI.Entities;

public class ModuleLocalization
{
    public long Id { get; set; }
    public long ModuleId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Module Module { get; set; } = default!;
}
