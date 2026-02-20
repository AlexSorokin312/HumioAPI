namespace HumioAPI.Contracts.Users;

public sealed record UsersImportResponse(
    int Total,
    int Created,
    int Existing,
    int Failed,
    int SubscriptionsApplied,
    int PurchasesApplied,
    long ModuleId);
