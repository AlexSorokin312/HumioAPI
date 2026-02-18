namespace HumioAPI.Entities;

public class Device
{
    public long Id { get; set; }
    public string DeviceKey { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<UserDevice> UserDevices { get; } = new List<UserDevice>();
    public ICollection<RefreshToken> RefreshTokens { get; } = new List<RefreshToken>();
}
