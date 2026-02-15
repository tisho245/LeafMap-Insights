using ASPLeafMapInsightsWebMVC.Services;
using ASPLeafMapInsightsWebMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASPLeafMapInsightsWebMVC.Controllers;

/// <summary>Страница с таксономия – зарежда Division, TaxonomyClass, Genus, Family, Species от API и ги подава на View.</summary>
public class TaxonomyController : Controller
{
    private readonly ILeafMapApiClient _api;

    public TaxonomyController(ILeafMapApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewBag.Divisions = await _api.GetAsync<List<DivisionVm>>("api/divisions", ct) ?? new List<DivisionVm>();
        ViewBag.TaxonomyClasses = await _api.GetAsync<List<TaxonomyClassVm>>("api/taxonomyclasses", ct) ?? new List<TaxonomyClassVm>();
        ViewBag.Genera = await _api.GetAsync<List<GenusVm>>("api/genera", ct) ?? new List<GenusVm>();
        ViewBag.Families = await _api.GetAsync<List<FamilyVm>>("api/families", ct) ?? new List<FamilyVm>();
        ViewBag.Species = await _api.GetAsync<List<SpeciesVm>>("api/species", ct) ?? new List<SpeciesVm>();
        return View();
    }
}
