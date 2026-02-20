namespace HumioAPI.Entities;

public class Module
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; } = new List<Product>();
    public ICollection<Lesson> Lessons { get; } = new List<Lesson>();
    public ICollection<ModuleLocalization> Localizations { get; } = new List<ModuleLocalization>();
    public ICollection<UserModuleAccess> UserModuleAccesses { get; } = new List<UserModuleAccess>();
}
