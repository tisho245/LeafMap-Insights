using ASPLeafMapInsightsWebMVC.Services;
using ASPLeafMapInsightsWebMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASPLeafMapInsightsWebMVC.Controllers;

/// <summary>Карта с дървета. Зарежда всички дървета от API (api/trees) и ги подава на View за маркери.</summary>
public class MapController : Controller
{
    private readonly ILeafMapApiClient _api;

    public MapController(ILeafMapApiClient api) => _api = api;

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var trees = await _api.GetAsync<List<TreeVm>>("api/trees", ct) ?? new List<TreeVm>();
        return View(trees);
    }
}
