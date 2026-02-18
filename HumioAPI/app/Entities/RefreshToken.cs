using System;

namespace HumioAPI.Entities;

public class RefreshToken
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? DeviceId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public long? ReplacedByTokenId { get; set; }

    public ApplicationUser User { get; set; } = default!;
    public Device? Device { get; set; }
    public RefreshToken? ReplacedByToken { get; set; }
}
