namespace HumioAPI.Entities;

public class Product
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Module> Modules { get; } = new List<Module>();
    public ICollection<Purchase> Purchases { get; } = new List<Purchase>();
    public ICollection<Promocode> Promocodes { get; } = new List<Promocode>();
    public ICollection<ProductSetLocalization> Localizations { get; } = new List<ProductSetLocalization>();
}
