using HumioAPI.Data;
using HumioAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace HumioAPI.Services;

public sealed class PromocodesService : IPromocodesService
{
    private readonly AppDbContext _dbContext;

    public PromocodesService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(bool Success, string[] Errors, Promocode? Promocode)> CreateAsync(
        string code,
        int maxUsageCount,
        int days,
        long productId,
        CancellationToken cancellationToken = default)
    {
        var trimmedCode = code.Trim();
        if (trimmedCode.Length == 0)
        {
            return (false, new[] { "Code is required." }, null);
        }

        var productExists = await _dbContext.Products.AnyAsync(p => p.Id == productId, cancellationToken);
        if (!productExists)
        {
            return (false, new[] { "Product not found." }, null);
        }

        var codeExists = await _dbContext.Promocodes
            .AnyAsync(p => p.Code == trimmedCode, cancellationToken);
        if (codeExists)
        {
            return (false, new[] { "Promocode with this code already exists." }, null);
        }

        var promocode = new Promocode
        {
            Code = trimmedCode,
            MaxUsageCount = maxUsageCount,
            Days = days,
            ProductId = productId
        };

        _dbContext.Promocodes.Add(promocode);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (true, Array.Empty<string>(), promocode);
    }

    public async Task<(bool Success, string[] Errors, Promocode? Promocode, bool NotFound)> UpdateAsync(
        long id,
        string? code,
        int? maxUsageCount,
        int? days,
        long? productId,
        CancellationToken cancellationToken = default)
    {
        var promocode = await _dbContext.Promocodes
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (promocode is null)
        {
            return (false, Array.Empty<string>(), null, true);
        }

        var errors = new List<string>();

        if (code is not null)
        {
            var trimmedCode = code.Trim();
            if (trimmedCode.Length == 0)
            {
                errors.Add("Code cannot be empty.");
            }
            else
            {
                var codeExists = await _dbContext.Promocodes
                    .AnyAsync(p => p.Code == trimmedCode && p.Id != id, cancellationToken);
                if (codeExists)
                {
                    errors.Add("Promocode with this code already exists.");
                }
                else
                {
                    promocode.Code = trimmedCode;
                }
            }
        }

        if (productId.HasValue)
        {
            var productExists = await _dbContext.Products
                .AnyAsync(p => p.Id == productId.Value, cancellationToken);
            if (!productExists)
            {
                errors.Add("Product not found.");
            }
            else
            {
                promocode.ProductId = productId.Value;
            }
        }

        if (maxUsageCount.HasValue)
        {
            promocode.MaxUsageCount = maxUsageCount.Value;
        }

        if (days.HasValue)
        {
            promocode.Days = days.Value;
        }

        if (errors.Count > 0)
        {
            return (false, errors.ToArray(), promocode, false);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, Array.Empty<string>(), promocode, false);
    }

    public async Task<(bool Success, string[] Errors, bool NotFound)> DeleteAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        var promocode = await _dbContext.Promocodes
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (promocode is null)
        {
            return (false, Array.Empty<string>(), true);
        }

        _dbContext.Promocodes.Remove(promocode);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return (true, Array.Empty<string>(), false);
    }

    public Task<Promocode?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var trimmedCode = code.Trim();
        if (trimmedCode.Length == 0)
        {
            return Task.FromResult<Promocode?>(null);
        }

        return _dbContext.Promocodes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == trimmedCode, cancellationToken);
    }

    public async Task<(int Total, Promocode[] Items)> ListAsync(
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Promocodes.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (total, items.ToArray());
    }
}
