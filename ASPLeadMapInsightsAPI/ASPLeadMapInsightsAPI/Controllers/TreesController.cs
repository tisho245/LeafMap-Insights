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

    /// <summary>
    /// Всички дървета; includeLookups=true зарежда Division, TaxonomyClass, Genus, Family, Species.
    /// За оптимизация PhotoURL изобщо не се селектира/връща тук – снимката се взима само при заявка по Id.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tree>>> GetAll(
        [FromQuery] bool includeLookups = false)
    {
        var baseQuery = _context.Trees.AsQueryable();

        // За списъка нямаме нужда от снимка – проектираме към нови Tree обекти без PhotoURL,
        // за да не се дърпа голямата колона от базата и да не се праща към клиента.
        if (includeLookups)
        {
            var withLookups = await baseQuery
                .OrderBy(t => t.Name)
                .Select(t => new Tree
                {
                    Id = t.Id,
                    Name = t.Name,
                    PhotoURL = null,
                    Description = t.Description,
                    Latitude = t.Latitude,
                    Longitude = t.Longitude,
                    DivisionId = t.DivisionId,
                    TaxonomyClassId = t.TaxonomyClassId,
                    GenusId = t.GenusId,
                    FamilyId = t.FamilyId,
                    SpeciesId = t.SpeciesId,
                    KlasId = t.KlasId,
                    Division = t.Division,
                    TaxonomyClass = t.TaxonomyClass,
                    Genus = t.Genus,
                    Family = t.Family,
                    Species = t.Species
                })
                .ToListAsync();

            return Ok(withLookups);
        }

        var withoutLookups = await baseQuery
            .OrderBy(t => t.Name)
            .Select(t => new Tree
            {
                Id = t.Id,
                Name = t.Name,
                PhotoURL = null,
                Description = t.Description,
                Latitude = t.Latitude,
                Longitude = t.Longitude,
                DivisionId = t.DivisionId,
                TaxonomyClassId = t.TaxonomyClassId,
                GenusId = t.GenusId,
                FamilyId = t.FamilyId,
                SpeciesId = t.SpeciesId,
                KlasId = t.KlasId
            })
            .ToListAsync();

        return Ok(withoutLookups);
    }

    /// <summary>Пагиниран списък – ?skip=0&amp;take=24. Връща { items: [...], total: N } за по-лесно зареждане без замръзване.</summary>
    [HttpGet("Paged")]
    public async Task<ActionResult<PagedTreesResult>> GetPaged(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 24,
        [FromQuery] bool includeLookups = true)
    {
        take = Math.Clamp(take, 1, 100);
        skip = Math.Max(0, skip);
        var query = _context.Trees.AsQueryable();
        var total = await query.CountAsync();
        if (includeLookups)
            query = query
                .Include(t => t.Division)
                .Include(t => t.TaxonomyClass)
                .Include(t => t.Genus)
                .Include(t => t.Family)
                .Include(t => t.Species);
        var items = await query.OrderBy(t => t.Name).Skip(skip).Take(take).ToListAsync();
        return Ok(new PagedTreesResult { Items = items, Total = total });
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

    /// <summary>Добавя ново дърво. Изисква валиден JWT. Приема CreateTreeDto – само FK id-та, без навигации, за да не вмъква EF в Divisions/Families/Genera/Species/TaxonomyClasses.</summary>
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Tree>> Create([FromBody] CreateTreeDto dto)
    {
        var tree = new Tree
        {
            Name = dto.Name?.Trim() ?? string.Empty,
            PhotoURL = dto.PhotoURL,
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            DivisionId = dto.DivisionId,
            TaxonomyClassId = dto.TaxonomyClassId,
            GenusId = dto.GenusId,
            FamilyId = dto.FamilyId,
            SpeciesId = dto.SpeciesId,
            KlasId = dto.KlasId
        };
        _context.Trees.Add(tree);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tree.Id }, tree);
    }

    /// <summary>
    /// Обновява дърво по Id. Приема DTO само със скаларни полета (без навигационни свойства),
    /// за да не се задейства автоматична валидация за Division/Family/Genus/Species навигациите.
    /// </summary>
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateTreeDto dto)
    {
        if (id <= 0 || id != dto.Id) return BadRequest("Invalid id.");

        var tree = await _context.Trees.FindAsync(id);
        if (tree == null) return NotFound();

        tree.Name = dto.Name?.Trim() ?? string.Empty;
        tree.PhotoURL = dto.PhotoURL;
        tree.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        tree.Latitude = dto.Latitude;
        tree.Longitude = dto.Longitude;
        tree.DivisionId = dto.DivisionId;
        tree.TaxonomyClassId = dto.TaxonomyClassId;
        tree.GenusId = dto.GenusId;
        tree.FamilyId = dto.FamilyId;
        tree.SpeciesId = dto.SpeciesId;
        tree.KlasId = dto.KlasId;

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

public class PagedTreesResult
{
    public List<Tree> Items { get; set; } = new();
    public int Total { get; set; }
}
