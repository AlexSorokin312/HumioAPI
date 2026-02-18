namespace HumioAPI.Contracts.Users;

public sealed record UserResponse(
    long Id,
    string Email,
    string? Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastSeen);
