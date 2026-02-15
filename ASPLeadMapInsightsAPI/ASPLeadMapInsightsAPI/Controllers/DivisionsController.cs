using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Controllers;

/// <summary>CRUD за референтната таблица Divisions (отдели в таксономията). GET е публичен; POST/PUT/DELETE изискват JWT.</summary>
[ApiController]
[Route("api/[controller]")]
public class DivisionsController : ControllerBase
{
    private readonly LeafMapDbContext _context;

    public DivisionsController(LeafMapDbContext context) => _context = context;

    /// <summary>Списък всички отдели, подредени по име.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Division>>> GetAll()
        => Ok(await _context.Divisions.OrderBy(d => d.Name).ToListAsync());

    /// <summary>Един отдел по Id; 404 ако няма такъв.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Division>> GetById(int id)
    {
        var entity = await _context.Divisions.FindAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    /// <summary>Добавя нов отдел. Изисква Authorization: Bearer &lt;token&gt;.</summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Division>> Create([FromBody] Division division)
    {
        _context.Divisions.Add(division);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = division.Id }, division);
    }

    /// <summary>Обновява съществуващ отдел по Id.</summary>
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Division division)
    {
        if (id != division.Id) return BadRequest();
        _context.Entry(division).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Divisions.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    /// <summary>Изтрива отдел по Id. Restrict в DbContext – ако има дървета с този DivisionId, може да има грешка.</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Divisions.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Divisions.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
