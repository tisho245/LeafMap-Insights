using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASPLeadMapInsightsAPI.Controllers;

/// <summary>
/// CRUD за централната таблица Trees. GET и WithinBounds са без токен; създаване/редакция/изтриване изискват [Authorize].
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TreesController : ControllerBase
{
    private readonly LeafMapDbContext _context;

    public TreesController(LeafMapDbContext context) => _context = context;

    /// <summary>
    /// Дървета в зададен правоъгълник по координати (minLat, maxLat, minLng, maxLng).
    /// Клиентът изпраща видимите граници на картата (напр. от Leaflet getBounds()). includeLookups включва Division, Species и др.
    /// </summary>
    [HttpGet("WithinBounds")]
    public async Task<ActionResult<IEnumerable<Tree>>> WithinBounds(
        [FromQuery] double minLat,
        [FromQuery] double minLng,
        [FromQuery] double maxLat,
        [FromQuery] double maxLng,
        [FromQuery] bool includeLookups = false)
    {
        var query = _context.Trees
            .Where(t => t.Latitude >= minLat && t.Latitude <= maxLat
                     && t.Longitude >= minLng && t.Longitude <= maxLng);
        // По избор зареждаме навигационните свойства за показ в UI.
        if (includeLookups)
            query = query
                .Include(t => t.Division)
                .Include(t => t.TaxonomyClass)
                .Include(t => t.Genus)
                .Include(t => t.Family)
                .Include(t => t.Species);
        var list = await query.OrderBy(t => t.Name).ToListAsync();
        return Ok(list);
    }

    /// <summary>Всички дървета; includeLookups=true зарежда Division, TaxonomyClass, Genus, Family, Species.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tree>>> GetAll(
        [FromQuery] bool includeLookups = false)
    {
        var query = _context.Trees.AsQueryable();
        if (includeLookups)
            query = query
                .Include(t => t.Division)
                .Include(t => t.TaxonomyClass)
                .Include(t => t.Genus)
                .Include(t => t.Family)
                .Include(t => t.Species);
        return Ok(await query.OrderBy(t => t.Name).ToListAsync());
    }

    /// <summary>Едно дърво по Id; 404 ако няма. includeLookups за пълни данни за таксономия.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Tree>> GetById(int id, [FromQuery] bool includeLookups = false)
    {
        var query = _context.Trees.AsQueryable();
        if (includeLookups)
            query = query
                .Include(t => t.Division)
                .Include(t => t.TaxonomyClass)
                .Include(t => t.Genus)
                .Include(t => t.Family)
                .Include(t => t.Species);
        var entity = await query.FirstOrDefaultAsync(t => t.Id == id);
        return entity == null ? NotFound() : Ok(entity);
    }

    /// <summary>Добавя ново дърво. Изисква валиден JWT в Authorization.</summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Tree>> Create([FromBody] Tree tree)
    {
        _context.Trees.Add(tree);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tree.Id }, tree);
    }

    /// <summary>Обновява дърво по Id. Id в URL и в тялото трябва да съвпадат.</summary>
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Tree tree)
    {
        if (id != tree.Id) return BadRequest();
        _context.Entry(tree).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Trees.AnyAsync(e => e.Id == id)) return NotFound();
            throw;
        }
        return NoContent();
    }

    /// <summary>Изтрива дърво по Id.</summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Trees.FindAsync(id);
        if (entity == null) return NotFound();
        _context.Trees.Remove(entity);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
