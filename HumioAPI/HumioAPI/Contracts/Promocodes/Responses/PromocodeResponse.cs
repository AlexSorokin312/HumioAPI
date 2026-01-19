namespace HumioAPI.Contracts.Promocodes;

public sealed record PromocodeResponse(
    long Id,
    string Code,
    int MaxUsageCount,
    int Days,
    long ProductId);
