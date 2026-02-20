namespace HumioAPI.Contracts.Users;

public sealed record UserResponse(
    long Id,
    string Email,
    string? Name,
    string? Country,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastSeen,
    DateTimeOffset? SubscriptionEndDate,
    UserModuleResponse[] Modules,
    int PurchasesCount,
    int TotalPurchasedAmountCents);

public sealed record UserModuleResponse(
    long Id,
    string Name,
    DateTimeOffset? EndsAt,
    bool GrantedByAdmin);
