using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Controllers;

/// <summary>CRUD за референтната таблица Families (семейства). GET без авторизация; промените с [Authorize].</summary>
[ApiController]
[Route("api/[controller]")]
public class FamiliesController : ControllerBase
{
    private readonly LeafMapDbContext _context;

    public FamiliesController(LeafMapDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Family>>> GetAll()
        => Ok(await _context.Families.OrderBy(f => f.Name).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Family>> GetById(int id)
    {
        var entity = await _context.Families.FindAsync(id);
        return entity == null ? NotFound() : Ok(entity);
    }

    /// <summary>Добавя ново семейство.</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Family>> Create([FromBody] Family family)
    {
        _context.Families.Add(family);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = family.Id }, family);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Family family)
    {
        if (id != family.Id) return BadRequest();
        _context.Entry(family).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Families.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    /// <summary>Изтрива семейство по Id.</summary>
    /// <summary>Изтрива семейство по Id.</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Families.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Families.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
