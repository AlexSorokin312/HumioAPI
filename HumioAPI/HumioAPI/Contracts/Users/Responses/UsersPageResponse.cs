namespace HumioAPI.Contracts.Users;

public sealed record UsersPageResponse(int Total, UserResponse[] Items);
