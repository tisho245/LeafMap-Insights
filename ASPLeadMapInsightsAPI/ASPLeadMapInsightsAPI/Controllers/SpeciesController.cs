using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Controllers;

/// <summary>CRUD за референтната таблица Species (видове). GET публичен; POST/PUT/DELETE с JWT.</summary>
[ApiController]
[Route("api/[controller]")]
public class SpeciesController : ControllerBase
{
    private readonly LeafMapDbContext _context;

    public SpeciesController(LeafMapDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Species>>> GetAll()
        => Ok(await _context.Species.OrderBy(s => s.Name).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Species>> GetById(int id)
    {
        var entity = await _context.Species.FindAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    /// <summary>Добавя нов вид.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Species>> Create([FromBody] Species species)
    {
        _context.Species.Add(species);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = species.Id }, species);
    }

    /// <summary>Обновява вид по Id.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Species species)
    {
        if (id != species.Id) return BadRequest();
        _context.Entry(species).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Species.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    /// <summary>Изтрива вид по Id.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Species.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Species.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
