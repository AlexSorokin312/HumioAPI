namespace HumioAPI.Entities;

public class Module
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int IntervalCount { get; set; }

    public ICollection<Product> Products { get; } = new List<Product>();
    public ICollection<UserModuleAccess> UserModuleAccesses { get; } = new List<UserModuleAccess>();
}
