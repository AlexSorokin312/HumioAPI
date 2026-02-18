using HumioAPI.Contracts.Users;

namespace HumioAPI.Contracts.Auth.Responses;

public sealed record AuthResponse(UserResponse User, AuthTokensResponse Tokens);
