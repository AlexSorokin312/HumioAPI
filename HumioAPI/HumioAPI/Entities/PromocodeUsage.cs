namespace HumioAPI.Entities;

public class PromocodeUsage
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long PromocodeId { get; set; }
    public DateTimeOffset UsedAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
    public Promocode Promocode { get; set; } = default!;
}
