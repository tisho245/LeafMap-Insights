using ASPLeafMapInsightsWebMVC.Services;
using ASPLeafMapInsightsWebMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASPLeafMapInsightsWebMVC.Controllers;

/// <summary>Списък дървета, детайли, създаване. Всички данни идват от API чрез ILeafMapApiClient (с JWT от Session при POST).</summary>
public class TreesController : Controller
{
    private readonly ILeafMapApiClient _api;

    public TreesController(ILeafMapApiClient api) => _api = api;

    /// <summary>Списък всички дървета с таксономия (includeLookups=true).</summary>
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _api.GetAsync<List<TreeVm>>("api/trees?includeLookups=true", ct);
        return View(list ?? new List<TreeVm>());
    }

    /// <summary>Едно дърво по id; 404 ако няма.</summary>
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var tree = await _api.GetAsync<TreeVm>($"api/trees/{id}?includeLookups=true", ct);
        if (tree == null) return NotFound();
        return View(tree);
    }

    /// <summary>Форма за ново дърво; зарежда референтни списъци (Division, Species и др.) от API за падащи менюта.</summary>
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await LoadTaxonomyViewData(ct);
        return View(new TreeVm());
    }

    /// <summary>POST на формата – изпраща данните към API api/trees. При неуспех (напр. нелогнат потребител) показва грешка.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TreeVm model, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
        {
            ModelState.AddModelError(nameof(model.Name), "Името е задължително.");
            await LoadTaxonomyViewData(ct);
            return View(model);
        }

        var res = await _api.PostAsync("api/trees", new
        {
            name = model.Name,
            photoURL = model.PhotoURL,
            description = model.Description,
            latitude = model.Latitude,
            longitude = model.Longitude,
            divisionId = model.DivisionId,
            taxonomyClassId = model.TaxonomyClassId,
            genusId = model.GenusId,
            familyId = model.FamilyId,
            speciesId = model.SpeciesId
        }, ct);

        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Неуспешно създаване. Уверете се, че сте логнати.");
            await LoadTaxonomyViewData(ct);
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Попълва ViewData с референтни списъци от API за dropdown-и във формата Create.</summary>
    private async Task LoadTaxonomyViewData(CancellationToken ct)
    {
        ViewData["Divisions"] = await _api.GetAsync<List<DivisionVm>>("api/divisions", ct) ?? new List<DivisionVm>();
        ViewData["TaxonomyClasses"] = await _api.GetAsync<List<TaxonomyClassVm>>("api/taxonomyclasses", ct) ?? new List<TaxonomyClassVm>();
        ViewData["Genera"] = await _api.GetAsync<List<GenusVm>>("api/genera", ct) ?? new List<GenusVm>();
        ViewData["Families"] = await _api.GetAsync<List<FamilyVm>>("api/families", ct) ?? new List<FamilyVm>();
        ViewData["Species"] = await _api.GetAsync<List<SpeciesVm>>("api/species", ct) ?? new List<SpeciesVm>();
    }
}
