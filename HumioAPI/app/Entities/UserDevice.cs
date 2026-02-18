namespace HumioAPI.Entities;

public class UserDevice
{
    public long UserId { get; set; }
    public long DeviceId { get; set; }
    public DateTimeOffset LinkedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public ApplicationUser User { get; set; } = default!;
    public Device Device { get; set; } = default!;
}
