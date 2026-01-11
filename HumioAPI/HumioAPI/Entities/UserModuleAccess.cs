namespace HumioAPI.Entities;

public class UserModuleAccess
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long ModuleId { get; set; }
    public DateTimeOffset EndsAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
    public Module Module { get; set; } = default!;
}
