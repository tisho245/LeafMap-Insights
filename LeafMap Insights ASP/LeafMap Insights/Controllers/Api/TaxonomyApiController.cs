using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeafMap_Insights.Data;

namespace LeafMap_Insights.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxonomyApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TaxonomyApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TaxonomyApi/Families
        [HttpGet("Families")]
        public async Task<ActionResult<IEnumerable<object>>> GetFamilies()
        {
            var families = await _context.Families
                .OrderBy(f => f.Name)
                .Select(f => new
                {
                    f.Id,
                    f.Name,
                    f.Description
                })
                .ToListAsync();

            return Ok(families);
        }

        // GET: api/TaxonomyApi/Genera
        [HttpGet("Genera")]
        public async Task<ActionResult<IEnumerable<object>>> GetGenera([FromQuery] int? familyId = null)
        {
            var query = _context.Genera
                .Include(g => g.Family)
                .AsQueryable();

            if (familyId.HasValue)
            {
                query = query.Where(g => g.FamilyId == familyId.Value);
            }

            var genera = await query
                .OrderBy(g => g.LatinName)
                .Select(g => new
                {
                    g.Id,
                    g.Name,
                    g.LatinName,
                    g.Description,
                    FamilyId = g.Family.Id,
                    FamilyName = g.Family.Name
                })
                .ToListAsync();

            return Ok(genera);
        }

        // GET: api/TaxonomyApi/Species
        [HttpGet("Species")]
        public async Task<ActionResult<IEnumerable<object>>> GetSpecies([FromQuery] int? genusId = null)
        {
            var query = _context.Species
                .Include(s => s.Genus)
                    .ThenInclude(g => g.Family)
                .AsQueryable();

            if (genusId.HasValue)
            {
                query = query.Where(s => s.GenusId == genusId.Value);
            }

            var species = await query
                .OrderBy(s => s.LatinName)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    s.LatinName,
                    s.Description,
                    GenusId = s.Genus.Id,
                    GenusName = s.Genus.LatinName,
                    FamilyId = s.Genus.Family.Id,
                    FamilyName = s.Genus.Family.Name
                })
                .ToListAsync();

            return Ok(species);
        }
    }
}
