using HumioAPI.Entities;

namespace HumioAPI.Services;

public interface IPromocodesService
{
    Task<(bool Success, string[] Errors, Promocode? Promocode)> CreateAsync(
        string code,
        int maxUsageCount,
        int days,
        long productId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, Promocode? Promocode, bool NotFound)> UpdateAsync(
        long id,
        string? code,
        int? maxUsageCount,
        int? days,
        long? productId,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string[] Errors, bool NotFound)> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<Promocode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<(int Total, Promocode[] Items)> ListAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default);
}
