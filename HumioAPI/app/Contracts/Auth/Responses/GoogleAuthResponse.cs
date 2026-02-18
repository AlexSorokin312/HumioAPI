using HumioAPI.Contracts.Users;

namespace HumioAPI.Contracts.Auth.Responses;

public sealed record GoogleAuthResponse(UserResponse User, bool IsNewUser, AuthTokensResponse Tokens);
