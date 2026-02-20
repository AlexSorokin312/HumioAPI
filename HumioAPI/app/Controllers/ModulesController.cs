using HumioAPI.Contracts.Modules;
using HumioAPI.Data;
using HumioAPI.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HumioAPI.Controllers;

[ApiController]
[Route("api/modules")]
public class ModulesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public ModulesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var modules = await _dbContext.Modules
            .AsNoTracking()
            .OrderBy(m => m.Id)
            .Select(m => new ModuleResponse(
                m.Id,
                m.Name,
                m.Description,
                m.Products
                    .OrderBy(p => p.Id)
                    .Select(p => new ProductSetResponse(
                        p.Id,
                        p.Name,
                        p.Modules
                            .OrderBy(pm => pm.Id)
                            .Select(pm => pm.Id)
                            .ToArray()))
                    .ToArray()))
            .ToArrayAsync();

        return Ok(modules);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateModuleRequest request)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            return BadRequest(new { errors = new[] { "Name is required." } });
        }

        var module = new Module
        {
            Name = name,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
        };

        _dbContext.Modules.Add(module);
        await _dbContext.SaveChangesAsync();

        var response = new ModuleResponse(
            module.Id,
            module.Name,
            module.Description,
            Array.Empty<ProductSetResponse>());

        return Created($"/api/modules/{module.Id}", response);
    }

    [HttpPatch("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateModuleRequest request)
    {
        var module = await _dbContext.Modules.FirstOrDefaultAsync(m => m.Id == id);
        if (module is null)
        {
            return NotFound();
        }

        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            return BadRequest(new { errors = new[] { "Name is required." } });
        }

        module.Name = name;
        module.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        await _dbContext.SaveChangesAsync();

        return Ok(new ModuleResponse(module.Id, module.Name, module.Description, Array.Empty<ProductSetResponse>()));
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var module = await _dbContext.Modules.FirstOrDefaultAsync(m => m.Id == id);
        if (module is null)
        {
            return NotFound();
        }

        _dbContext.Modules.Remove(module);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:long}/localizations")]
    public async Task<IActionResult> GetLocalization(
        long id,
        [FromQuery] string languageCode)
    {
        var normalizedLanguageCode = languageCode.Trim().ToLowerInvariant();
        if (normalizedLanguageCode.Length == 0)
        {
            return BadRequest(new { errors = new[] { "languageCode is required." } });
        }

        var moduleExists = await _dbContext.Modules.AnyAsync(m => m.Id == id);
        if (!moduleExists)
        {
            return NotFound();
        }

        var localization = await _dbContext.ModuleLocalizations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                l => l.ModuleId == id && l.LanguageCode.ToLower() == normalizedLanguageCode);

        if (localization is null)
        {
            return NotFound();
        }

        return Ok(new ModuleLocalizationResponse(
            localization.Id,
            localization.ModuleId,
            localization.LanguageCode,
            localization.Name,
            localization.Description));
    }

    [HttpPut("{id:long}/localizations")]
    public async Task<IActionResult> UpsertLocalization(
        long id,
        [FromBody] UpsertModuleLocalizationRequest request)
    {
        var moduleExists = await _dbContext.Modules.AnyAsync(m => m.Id == id);
        if (!moduleExists)
        {
            return NotFound(new { errors = new[] { "Module not found." } });
        }

        var normalizedLanguageCode = request.LanguageCode.Trim().ToLowerInvariant();
        if (normalizedLanguageCode.Length == 0)
        {
            return BadRequest(new { errors = new[] { "LanguageCode is required." } });
        }

        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            return BadRequest(new { errors = new[] { "Name is required." } });
        }

        var localization = await _dbContext.ModuleLocalizations
            .FirstOrDefaultAsync(
                l => l.ModuleId == id && l.LanguageCode.ToLower() == normalizedLanguageCode);

        if (localization is null)
        {
            localization = new ModuleLocalization
            {
                ModuleId = id,
                LanguageCode = normalizedLanguageCode,
                Name = name,
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim()
            };

            _dbContext.ModuleLocalizations.Add(localization);
        }
        else
        {
            localization.Name = name;
            localization.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        await _dbContext.SaveChangesAsync();

        return Ok(new ModuleLocalizationResponse(
            localization.Id,
            localization.ModuleId,
            localization.LanguageCode,
            localization.Name,
            localization.Description));
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var name = request.Name.Trim();
        if (name.Length == 0)
        {
            return BadRequest(new { errors = new[] { "Name is required." } });
        }

        if (request.ModuleIds is null || request.ModuleIds.Length == 0)
        {
            return BadRequest(new { errors = new[] { "At least one moduleId is required." } });
        }

        var distinctModuleIds = request.ModuleIds.Distinct().ToArray();
        var modules = await _dbContext.Modules
            .Where(m => distinctModuleIds.Contains(m.Id))
            .ToListAsync();

        if (modules.Count != distinctModuleIds.Length)
        {
            return BadRequest(new { errors = new[] { "One or more moduleIds were not found." } });
        }

        var product = new Product { Name = name };
        foreach (var module in modules)
        {
            product.Modules.Add(module);
        }

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return Created(
            $"/api/modules/products/{product.Id}",
            new ProductSetResponse(product.Id, product.Name, distinctModuleIds));
    }
}
