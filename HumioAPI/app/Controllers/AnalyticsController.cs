using HumioAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumioAPI.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public AnalyticsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("promocodes-usage")]
    public async Task<IActionResult> GetPromocodesUsage([FromQuery] int take = 8)
    {
        if (take <= 0)
        {
            return BadRequest(new { errors = new[] { "take must be > 0." } });
        }

        var pageSize = Math.Min(take, 50);
        var items = await _dbContext.Promocodes
            .AsNoTracking()
            .Select(promocode => new
            {
                promocode.Id,
                promocode.Code,
                promocode.MaxUsageCount,
                UsedCount = promocode.Usages.Count()
            })
            .OrderByDescending(item => item.UsedCount)
            .ThenBy(item => item.Id)
            .Take(pageSize)
            .ToArrayAsync();

        var totalUsed = await _dbContext.PromocodeUsages.AsNoTracking().CountAsync();

        return Ok(new
        {
            totalUsed,
            items
        });
    }
}
