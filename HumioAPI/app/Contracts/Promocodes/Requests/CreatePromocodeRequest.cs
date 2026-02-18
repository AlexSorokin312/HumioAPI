using System.ComponentModel.DataAnnotations;

namespace HumioAPI.Contracts.Promocodes;

public sealed record CreatePromocodeRequest(
    [param: Required] string Code,
    [param: Range(1, int.MaxValue)] int MaxUsageCount,
    [param: Range(1, int.MaxValue)] int Days,
    [param: Range(1, long.MaxValue)] long ProductId);
