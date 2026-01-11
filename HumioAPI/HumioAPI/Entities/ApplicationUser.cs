using Microsoft.AspNetCore.Identity;

namespace HumioAPI.Entities;

public class ApplicationUser : IdentityUser<long>
{
    public string? Name { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastSeen { get; set; }

    public ICollection<UserDevice> UserDevices { get; } = new List<UserDevice>();
    public ICollection<Purchase> Purchases { get; } = new List<Purchase>();
    public ICollection<PromocodeUsage> PromocodeUsages { get; } = new List<PromocodeUsage>();
    public ICollection<UserModuleAccess> ModuleAccesses { get; } = new List<UserModuleAccess>();

    public ICollection<AdminAccessHistory> AdminAccessEntries { get; } = new List<AdminAccessHistory>();
    public ICollection<AdminAccessHistory> TargetAccessEntries { get; } = new List<AdminAccessHistory>();
}
