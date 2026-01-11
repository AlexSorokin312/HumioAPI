namespace HumioAPI.Entities;

public class Purchase
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ProductId { get; set; }
    public int AmountCents { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderPaymentId { get; set; } = string.Empty;
    public string? Receipt { get; set; }
    public PaymentStatus Status { get; set; }
    public int Days { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PurchasedAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
    public Product Product { get; set; } = default!;
}
