namespace HumioAPI.Entities;

public class Product
{
    public long Id { get; set; }
    public long ModuleId { get; set; }
    public string Name { get; set; } = string.Empty;

    public Module Module { get; set; } = default!;
    public ICollection<Purchase> Purchases { get; } = new List<Purchase>();
    public ICollection<AdminAccessHistory> AdminAccessHistory { get; } = new List<AdminAccessHistory>();
    public ICollection<Promocode> Promocodes { get; } = new List<Promocode>();
}
