namespace HumioAPI.Entities;

public class Promocode
{
    public long Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public int MaxUsageCount { get; set; }
    public int Days { get; set; }
    public long ProductId { get; set; }

    public Product Product { get; set; } = default!;
    public ICollection<PromocodeUsage> Usages { get; } = new List<PromocodeUsage>();
}
