using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LeafMap_Insights.Data;
using LeafMap_Insights.Models;
using LeafMap_Insights.Models.ViewModels;
using LeafMap_Insights.Services;

namespace LeafMap_Insights.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IQRCodeService _qrCodeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, IQRCodeService qrCodeService, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _qrCodeService = qrCodeService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        #region Dashboard
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalTrees = await _context.Trees.CountAsync(),
                TotalSpecies = await _context.Species.CountAsync(),
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalFamilies = await _context.Families.CountAsync(),
                TotalGenera = await _context.Genera.CountAsync()
            };

            return View(viewModel);
        }
        #endregion

        #region Tree Management
        public async Task<IActionResult> Trees()
        {
            var trees = await _context.Trees
                .Include(t => t.Species)
                    .ThenInclude(s => s.Genus)
                        .ThenInclude(g => g.Family)
                .ToListAsync();

            return View(trees);
        }

        public async Task<IActionResult> CreateTree()
        {
            var viewModel = new TreeViewModel
            {
                AvailableSpecies = await _context.Species
                    .Include(s => s.Genus)
                        .ThenInclude(g => g.Family)
                    .OrderBy(s => s.LatinName)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTree(TreeViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var tree = new Tree
                {
                    SpeciesId = viewModel.SpeciesId,
                    Latitude = viewModel.Latitude,
                    Longitude = viewModel.Longitude,
                    EstimatedAge = viewModel.EstimatedAge,
                    Height = viewModel.Height,
                    Condition = viewModel.Condition,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Add(tree);
                await _context.SaveChangesAsync();

                // Generate QR code after tree is saved (so we have the ID)
                var request = _httpContextAccessor.HttpContext?.Request;
                var baseUrl = $"{request?.Scheme}://{request?.Host}";
                var treeUrl = $"{baseUrl}/Trees/Details/{tree.Id}";
                tree.QRCodeValue = _qrCodeService.GenerateQRCode(treeUrl);
                _context.Update(tree);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Trees));
            }

            viewModel.AvailableSpecies = await _context.Species
                .Include(s => s.Genus)
                    .ThenInclude(g => g.Family)
                .OrderBy(s => s.LatinName)
                .ToListAsync();

            return View(viewModel);
        }

        public async Task<IActionResult> EditTree(int? id)
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

            var viewModel = new TreeViewModel
            {
                Id = tree.Id,
                SpeciesId = tree.SpeciesId,
                Latitude = tree.Latitude,
                Longitude = tree.Longitude,
                EstimatedAge = tree.EstimatedAge,
                Height = tree.Height,
                Condition = tree.Condition,
                AvailableSpecies = await _context.Species
                    .Include(s => s.Genus)
                        .ThenInclude(g => g.Family)
                    .OrderBy(s => s.LatinName)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTree(int id, TreeViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var tree = await _context.Trees.FindAsync(id);
                if (tree == null)
                {
                    return NotFound();
                }

                tree.SpeciesId = viewModel.SpeciesId;
                tree.Latitude = viewModel.Latitude;
                tree.Longitude = viewModel.Longitude;
                tree.EstimatedAge = viewModel.EstimatedAge;
                tree.Height = viewModel.Height;
                tree.Condition = viewModel.Condition;
                tree.UpdatedAt = DateTime.UtcNow;

                try
                {
                    _context.Update(tree);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TreeExists(tree.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }

                return RedirectToAction(nameof(Trees));
            }

            viewModel.AvailableSpecies = await _context.Species
                .Include(s => s.Genus)
                    .ThenInclude(g => g.Family)
                .OrderBy(s => s.LatinName)
                .ToListAsync();

            return View(viewModel);
        }

        public async Task<IActionResult> DeleteTree(int? id)
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

            return View(tree);
        }

        [HttpPost, ActionName("DeleteTree")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTreeConfirmed(int id)
        {
            var tree = await _context.Trees.FindAsync(id);
            if (tree != null)
            {
                _context.Trees.Remove(tree);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Trees));
        }

        private bool TreeExists(int id)
        {
            return _context.Trees.Any(e => e.Id == id);
        }
        #endregion

        #region Taxonomy Management - Family
        public async Task<IActionResult> Families()
        {
            return View(await _context.Families.OrderBy(f => f.Name).ToListAsync());
        }

        public IActionResult CreateFamily()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFamily(Family family)
        {
            if (ModelState.IsValid)
            {
                _context.Add(family);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Families));
            }
            return View(family);
        }

        public async Task<IActionResult> EditFamily(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var family = await _context.Families.FindAsync(id);
            if (family == null)
            {
                return NotFound();
            }
            return View(family);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFamily(int id, Family family)
        {
            if (id != family.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(family);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FamilyExists(family.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Families));
            }
            return View(family);
        }

        public async Task<IActionResult> DeleteFamily(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var family = await _context.Families
                .FirstOrDefaultAsync(m => m.Id == id);
            if (family == null)
            {
                return NotFound();
            }

            return View(family);
        }

        [HttpPost, ActionName("DeleteFamily")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFamilyConfirmed(int id)
        {
            var family = await _context.Families
                .Include(f => f.Genera)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if (family == null)
            {
                return NotFound();
            }

            if (family.Genera.Any())
            {
                TempData["ErrorMessage"] = "Не можете да изтриете това семейство, защото има родове, които го използват. Първо изтрийте всички свързани родове.";
                return RedirectToAction(nameof(DeleteFamily), new { id });
            }

            _context.Families.Remove(family);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Families));
        }

        private bool FamilyExists(int id)
        {
            return _context.Families.Any(e => e.Id == id);
        }
        #endregion

        #region Taxonomy Management - Genus
        public async Task<IActionResult> Genera()
        {
            var genera = await _context.Genera
                .Include(g => g.Family)
                .OrderBy(g => g.LatinName)
                .ToListAsync();

            return View(genera);
        }

        public async Task<IActionResult> CreateGenus()
        {
            ViewData["FamilyId"] = new SelectList(_context.Families.OrderBy(f => f.Name), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGenus(Genus genus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Genera));
            }
            ViewData["FamilyId"] = new SelectList(_context.Families.OrderBy(f => f.Name), "Id", "Name", genus.FamilyId);
            return View(genus);
        }

        public async Task<IActionResult> EditGenus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genus = await _context.Genera.FindAsync(id);
            if (genus == null)
            {
                return NotFound();
            }
            ViewData["FamilyId"] = new SelectList(_context.Families.OrderBy(f => f.Name), "Id", "Name", genus.FamilyId);
            return View(genus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGenus(int id, Genus genus)
        {
            if (id != genus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GenusExists(genus.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Genera));
            }
            ViewData["FamilyId"] = new SelectList(_context.Families.OrderBy(f => f.Name), "Id", "Name", genus.FamilyId);
            return View(genus);
        }

        public async Task<IActionResult> DeleteGenus(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var genus = await _context.Genera
                .Include(g => g.Family)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (genus == null)
            {
                return NotFound();
            }

            return View(genus);
        }

        [HttpPost, ActionName("DeleteGenus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGenusConfirmed(int id)
        {
            var genus = await _context.Genera
                .Include(g => g.Species)
                .FirstOrDefaultAsync(g => g.Id == id);
            
            if (genus == null)
            {
                return NotFound();
            }

            if (genus.Species.Any())
            {
                TempData["ErrorMessage"] = "Не можете да изтриете този род, защото има видове, които го използват. Първо изтрийте всички свързани видове.";
                return RedirectToAction(nameof(DeleteGenus), new { id });
            }

            _context.Genera.Remove(genus);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Genera));
        }

        private bool GenusExists(int id)
        {
            return _context.Genera.Any(e => e.Id == id);
        }
        #endregion

        #region Taxonomy Management - Species
        public async Task<IActionResult> Species()
        {
            var species = await _context.Species
                .Include(s => s.Genus)
                    .ThenInclude(g => g.Family)
                .OrderBy(s => s.LatinName)
                .ToListAsync();

            return View(species);
        }

        public async Task<IActionResult> CreateSpecies()
        {
            ViewData["GenusId"] = new SelectList(_context.Genera
                .Include(g => g.Family)
                .OrderBy(g => g.LatinName)
                .Select(g => new { g.Id, Name = $"{g.LatinName} ({g.Family.Name})" }), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSpecies(Species species)
        {
            if (ModelState.IsValid)
            {
                _context.Add(species);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Species));
            }
            ViewData["GenusId"] = new SelectList(_context.Genera
                .Include(g => g.Family)
                .OrderBy(g => g.LatinName)
                .Select(g => new { g.Id, Name = $"{g.LatinName} ({g.Family.Name})" }), "Id", "Name", species.GenusId);
            return View(species);
        }

        public async Task<IActionResult> EditSpecies(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var species = await _context.Species.FindAsync(id);
            if (species == null)
            {
                return NotFound();
            }
            ViewData["GenusId"] = new SelectList(_context.Genera
                .Include(g => g.Family)
                .OrderBy(g => g.LatinName)
                .Select(g => new { g.Id, Name = $"{g.LatinName} ({g.Family.Name})" }), "Id", "Name", species.GenusId);
            return View(species);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSpecies(int id, Species species)
        {
            if (id != species.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(species);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SpeciesExists(species.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Species));
            }
            ViewData["GenusId"] = new SelectList(_context.Genera
                .Include(g => g.Family)
                .OrderBy(g => g.LatinName)
                .Select(g => new { g.Id, Name = $"{g.LatinName} ({g.Family.Name})" }), "Id", "Name", species.GenusId);
            return View(species);
        }

        public async Task<IActionResult> DeleteSpecies(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var species = await _context.Species
                .Include(s => s.Genus)
                    .ThenInclude(g => g.Family)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (species == null)
            {
                return NotFound();
            }

            return View(species);
        }

        [HttpPost, ActionName("DeleteSpecies")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpeciesConfirmed(int id)
        {
            var species = await _context.Species
                .Include(s => s.Trees)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (species == null)
            {
                return NotFound();
            }

            if (species.Trees.Any())
            {
                TempData["ErrorMessage"] = "Не можете да изтриете този вид, защото има дървета, които го използват. Първо изтрийте всички свързани дървета.";
                return RedirectToAction(nameof(DeleteSpecies), new { id });
            }

            _context.Species.Remove(species);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Species));
        }

        private bool SpeciesExists(int id)
        {
            return _context.Species.Any(e => e.Id == id);
        }
        #endregion

        #region User Management
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    EmailConfirmed = user.EmailConfirmed,
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> EditUser(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var viewModel = new UserEditViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                EmailConfirmed = user.EmailConfirmed,
                AvailableRoles = allRoles.Select(r => r.Name ?? "").ToList(),
                SelectedRoles = userRoles.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(string id, UserEditViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = viewModel.Email;
                user.UserName = viewModel.UserName;
                user.EmailConfirmed = viewModel.EmailConfirmed;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Update roles
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = currentRoles.Except(viewModel.SelectedRoles ?? new List<string>());
                    var rolesToAdd = (viewModel.SelectedRoles ?? new List<string>()).Except(currentRoles);

                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    await _userManager.AddToRolesAsync(user, rolesToAdd);

                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var allRoles = await _roleManager.Roles.ToListAsync();
            viewModel.AvailableRoles = allRoles.Select(r => r.Name ?? "").ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> DeleteUser(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles.ToList()
            };

            return View(viewModel);
        }

        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View("DeleteUser", new UserViewModel { Id = id });
                }
            }

            return RedirectToAction(nameof(Users));
        }
        #endregion
    }
}
