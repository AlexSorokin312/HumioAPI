namespace HumioAPI.Contracts.Users;

public sealed record UserCreatedResponse(long Id, string Email, string? Name);
