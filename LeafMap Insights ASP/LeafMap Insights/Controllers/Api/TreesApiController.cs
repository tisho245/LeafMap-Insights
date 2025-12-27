using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeafMap_Insights.Data;
using LeafMap_Insights.Models;

namespace LeafMap_Insights.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class TreesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TreesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TreesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTrees()
        {
            var trees = await _context.Trees
                .Include(t => t.Species)
                    .ThenInclude(s => s.Genus)
                        .ThenInclude(g => g.Family)
                .Select(t => new
                {
                    t.Id,
                    t.Latitude,
                    t.Longitude,
                    t.EstimatedAge,
                    t.Height,
                    t.Condition,
                    Species = new
                    {
                        t.Species.Id,
                        t.Species.Name,
                        t.Species.LatinName,
                        t.Species.Description,
                        Genus = new
                        {
                            t.Species.Genus.Id,
                            t.Species.Genus.Name,
                            t.Species.Genus.LatinName,
                            Family = new
                            {
                                t.Species.Genus.Family.Id,
                                t.Species.Genus.Family.Name,
                                t.Species.Genus.Family.Description
                            }
                        }
                    },
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .ToListAsync();

            return Ok(trees);
        }

        // GET: api/TreesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTree(int id)
        {
            var tree = await _context.Trees
                .Include(t => t.Species)
                    .ThenInclude(s => s.Genus)
                        .ThenInclude(g => g.Family)
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    t.Id,
                    t.Latitude,
                    t.Longitude,
                    t.EstimatedAge,
                    t.Height,
                    t.Condition,
                    Species = new
                    {
                        t.Species.Id,
                        t.Species.Name,
                        t.Species.LatinName,
                        t.Species.Description,
                        Genus = new
                        {
                            t.Species.Genus.Id,
                            t.Species.Genus.Name,
                            t.Species.Genus.LatinName,
                            t.Species.Genus.Description,
                            Family = new
                            {
                                t.Species.Genus.Family.Id,
                                t.Species.Genus.Family.Name,
                                t.Species.Genus.Family.Description
                            }
                        }
                    },
                    t.CreatedAt,
                    t.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (tree == null)
            {
                return NotFound();
            }

            return Ok(tree);
        }

        // GET: api/TreesApi/WithinBounds
        [HttpGet("WithinBounds")]
        public async Task<ActionResult<IEnumerable<object>>> GetTreesWithinBounds(
            [FromQuery] decimal minLat,
            [FromQuery] decimal minLng,
            [FromQuery] decimal maxLat,
            [FromQuery] decimal maxLng)
        {
            var trees = await _context.Trees
                .Include(t => t.Species)
                    .ThenInclude(s => s.Genus)
                        .ThenInclude(g => g.Family)
                .Where(t => t.Latitude >= minLat && t.Latitude <= maxLat &&
                           t.Longitude >= minLng && t.Longitude <= maxLng)
                .Select(t => new
                {
                    t.Id,
                    t.Latitude,
                    t.Longitude,
                    t.EstimatedAge,
                    t.Height,
                    t.Condition,
                    Species = new
                    {
                        t.Species.Id,
                        t.Species.Name,
                        t.Species.LatinName,
                        SpeciesName = t.Species.LatinName,
                        Genus = new
                        {
                            t.Species.Genus.LatinName,
                            Family = new
                            {
                                t.Species.Genus.Family.Name
                            }
                        }
                    }
                })
                .ToListAsync();

            return Ok(trees);
        }
    }
}
