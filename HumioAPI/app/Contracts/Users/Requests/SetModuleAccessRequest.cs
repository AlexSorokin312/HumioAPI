namespace HumioAPI.Contracts.Users;

public sealed record SetModuleAccessRequest(
    bool Enabled,
    DateTimeOffset? EndsAt);
