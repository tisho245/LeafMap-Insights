using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Controllers;

/// <summary>CRUD за референтната таблица TaxonomyClasses (класове). GET без токен; POST/PUT/DELETE с [Authorize].</summary>
[ApiController]
[Route("api/[controller]")]
public class TaxonomyClassesController : ControllerBase
{
    private readonly LeafMapDbContext _context;

    public TaxonomyClassesController(LeafMapDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaxonomyClass>>> GetAll()
        => Ok(await _context.TaxonomyClasses.OrderBy(c => c.Name).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaxonomyClass>> GetById(int id)
    {
        var entity = await _context.TaxonomyClasses.FindAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    /// <summary>Създава нов клас в таксономията.</summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<TaxonomyClass>> Create([FromBody] TaxonomyClass taxonomyClass)
    {
        _context.TaxonomyClasses.Add(taxonomyClass);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = taxonomyClass.Id }, taxonomyClass);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaxonomyClass taxonomyClass)  // Обновяване по Id
    {
        if (id != taxonomyClass.Id) return BadRequest();
        _context.Entry(taxonomyClass).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.TaxonomyClasses.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    /// <summary>Изтрива запис от TaxonomyClasses.</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.TaxonomyClasses.FindAsync(id);
        if (entity == null) return NotFound();
        _context.TaxonomyClasses.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
