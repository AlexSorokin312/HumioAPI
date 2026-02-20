namespace HumioAPI.Entities;

public class ProductSetLocalization
{
    public long Id { get; set; }
    public long ProductSetId { get; set; }
    public string LanguageCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Product ProductSet { get; set; } = default!;
}
