namespace HumioAPI.Contracts.Auth.Responses;

public sealed record AuthTokensResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt);
