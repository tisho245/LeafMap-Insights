using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Controllers;

/// <summary>CRUD за референтната таблица Genera (родове). GET публичен; POST/PUT/DELETE с JWT.</summary>
[ApiController]
[Route("api/[controller]")]
public class GeneraController : ControllerBase
{
    private readonly LeafMapDbContext _context;

    public GeneraController(LeafMapDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Genus>>> GetAll()
        => Ok(await _context.Genera.OrderBy(g => g.Name).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Genus>> GetById(int id)
    {
        var entity = await _context.Genera.FindAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    /// <summary>Добавя нов род.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Genus>> Create([FromBody] Genus genus)
    {
        _context.Genera.Add(genus);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = genus.Id }, genus);
    }

    /// <summary>Обновява род по Id.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Genus genus)
    {
        if (id != genus.Id) return BadRequest();
        _context.Entry(genus).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Genera.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    /// <summary>Изтрива род по Id.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Genera.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Genera.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
