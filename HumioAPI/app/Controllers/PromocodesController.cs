using System.ComponentModel.DataAnnotations;
using System.Linq;
using HumioAPI.Contracts.Promocodes;
using HumioAPI.Entities;
using HumioAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HumioAPI.Controllers;

[ApiController]
[Route("api/promocodes")]
public class PromocodesController : ControllerBase
{
    private readonly IPromocodesService _promocodesService;

    public PromocodesController(IPromocodesService promocodesService)
    {
        _promocodesService = promocodesService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromocodeRequest request)
    {
        var (success, errors, promocode) = await _promocodesService.CreateAsync(
            request.Code,
            request.MaxUsageCount,
            request.Days,
            request.ProductId);

        if (!success || promocode is null)
        {
            return BadRequest(new { errors });
        }

        return Created($"/api/promocodes/{promocode.Id}", ToResponse(promocode));
    }

    [HttpPatch("{id:long}")]
    public async Task<IActionResult> Patch(long id, [FromBody] UpdatePromocodeRequest request)
    {
        if (request is null)
        {
            return BadRequest(new { errors = new[] { "Request body is required." } });
        }

        if (request.Code is null && request.MaxUsageCount is null && request.Days is null && request.ProductId is null)
        {
            return BadRequest(new { errors = new[] { "At least one field must be provided." } });
        }

        var (success, errors, promocode, notFound) = await _promocodesService.UpdateAsync(
            id,
            request.Code,
            request.MaxUsageCount,
            request.Days,
            request.ProductId);

        if (notFound || promocode is null)
        {
            return NotFound();
        }

        if (!success)
        {
            return BadRequest(new { errors });
        }

        return Ok(ToResponse(promocode));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var (success, errors, notFound) = await _promocodesService.DeleteAsync(id);
        if (notFound)
        {
            return NotFound();
        }

        if (!success)
        {
            return BadRequest(new { errors });
        }

        return NoContent();
    }

    [HttpGet("by-code")]
    public async Task<IActionResult> GetByCode([FromQuery][Required] string code)
    {
        var promocode = await _promocodesService.GetByCodeAsync(code);
        if (promocode is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(promocode));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        if (skip < 0)
        {
            return BadRequest(new { errors = new[] { "skip must be >= 0." } });
        }

        if (take <= 0)
        {
            return BadRequest(new { errors = new[] { "take must be > 0." } });
        }

        var pageSize = Math.Min(take, 100);
        var (total, items) = await _promocodesService.ListAsync(skip, pageSize);
        var responseItems = items.Select(ToResponse).ToArray();
        return Ok(new PromocodesPageResponse(total, responseItems));
    }

    private static PromocodeResponse ToResponse(Promocode promocode) =>
        new(promocode.Id, promocode.Code, promocode.MaxUsageCount, promocode.Days, promocode.ProductId);
}
