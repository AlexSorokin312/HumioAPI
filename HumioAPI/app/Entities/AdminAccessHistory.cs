namespace HumioAPI.Entities;

public class AdminAccessHistory
{
    public long Id { get; set; }
    public long AdminId { get; set; }
    public long TargetUserId { get; set; }
    public long ProductId { get; set; }
    public int Days { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ApplicationUser Admin { get; set; } = default!;
    public ApplicationUser TargetUser { get; set; } = default!;
    public Product Product { get; set; } = default!;
}
