using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LeafMap_Insights.Data;
using LeafMap_Insights.Models;
using LeafMap_Insights.Services;
using Microsoft.Extensions.Logging;

namespace LeafMap_Insights.Controllers
{
    public class TreesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IQRCodeService _qrCodeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TreesController>? _logger;

        public TreesController(ApplicationDbContext context, IQRCodeService qrCodeService, IHttpContextAccessor httpContextAccessor, ILogger<TreesController>? logger = null)
        {
            _context = context;
            _qrCodeService = qrCodeService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // GET: Trees
        public async Task<IActionResult> Index()
        {
            var trees = await _context.Trees
                .Include(t => t.Species)
                    .ThenInclude(s => s.Genus)
                        .ThenInclude(g => g.Family)
                .ToListAsync();

            return View(trees);
        }

        // GET: Trees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tree = await _context.Trees
                .Include(t => t.Species)
                    .ThenInclude(s => s.Genus)
                        .ThenInclude(g => g.Family)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (tree == null)
            {
                return NotFound();
            }

            // Generate QR code if not exists
            if (string.IsNullOrEmpty(tree.QRCodeValue))
            {
                try
                {
                    var request = _httpContextAccessor.HttpContext?.Request;
                    var baseUrl = $"{request?.Scheme}://{request?.Host}";
                    var treeUrl = $"{baseUrl}/Trees/Details/{tree.Id}";
                    tree.QRCodeValue = _qrCodeService.GenerateQRCode(treeUrl);
                    _context.Update(tree);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the request
                    _logger?.LogError(ex, "Грешка при генериране на QR код за дърво {TreeId}", tree.Id);
                }
            }

            ViewBag.QRCodeBase64 = tree.QRCodeValue;
            return View(tree);
        }

        // GET: Trees/Map
        public async Task<IActionResult> Map()
        {
            var trees = await _context.Trees
                .Include(t => t.Species)
                    .ThenInclude(s => s.Genus)
                        .ThenInclude(g => g.Family)
                .ToListAsync();

            return View(trees);
        }

        // GET: Trees/QRCode/5
        public async Task<IActionResult> QRCode(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tree = await _context.Trees.FindAsync(id);
            if (tree == null)
            {
                return NotFound();
            }

            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var treeUrl = $"{baseUrl}/Trees/Details/{tree.Id}";

            var qrCodeBytes = _qrCodeService.GenerateQRCodeBytes(treeUrl);
            return File(qrCodeBytes, "image/png", $"Tree_{tree.Id}_QRCode.png");
        }
    }
}
