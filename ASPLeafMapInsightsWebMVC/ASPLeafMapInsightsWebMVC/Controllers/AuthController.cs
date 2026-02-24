using System.Text;
using System.Text.Json;
using ASPLeafMapInsightsWebMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASPLeafMapInsightsWebMVC.Controllers;

/// <summary>
/// Вход, регистрация и изход. При успешен login/register викаме API и записваме JWT в Session под ключ "Token".
/// Останалите контролери използват ILeafMapApiClient, който чете този Token и го изпраща при заявките.
/// </summary>
public class AuthController : Controller
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _factory;

    public AuthController(IConfiguration config, IHttpClientFactory factory)
    {
        _config = config;
        _factory = factory;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("", "Email и парола са задължителни.");
            return View(model);
        }

        var baseUrl = (_config["AuthApiBaseUrl"] ?? _config["ApiBaseUrl"])?.TrimEnd('/') ?? "";
        var client = _factory.CreateClient("LeafMapAuthApi");
        var json = JsonSerializer.Serialize(new { email = model.Email, password = model.Password });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var res = await client.PostAsync($"{baseUrl}/api/auth/login", content, ct);
        if (!res.IsSuccessStatusCode)
        {
            ModelState.AddModelError("", "Невалиден email или парола.");
            return View(model);
        }

        // Извличаме JWT от отговора и го пазим в Session – LeafMapApiClient го използва при следващи заявки.
        var responseJson = await res.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);
        var token = doc.RootElement.GetProperty("token").GetString();
        if (!string.IsNullOrEmpty(token))
            HttpContext.Session.SetString("Token", token);

        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("", "Email и парола са задължителни.");
            return View(model);
        }

        var baseUrl = (_config["AuthApiBaseUrl"] ?? _config["ApiBaseUrl"])?.TrimEnd('/') ?? "";
        var client = _factory.CreateClient("LeafMapAuthApi");
        var json = JsonSerializer.Serialize(new { email = model.Email, password = model.Password, userName = model.UserName });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var res = await client.PostAsync($"{baseUrl}/api/auth/register", content, ct);
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct);
            ModelState.AddModelError("", "Регистрацията не успешна. " + err);
            return View(model);
        }

        // След успешна регистрация API връща токен – записваме го в Session като при login.
        var responseJson = await res.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(responseJson);
        var token = doc.RootElement.GetProperty("token").GetString();
        if (!string.IsNullOrEmpty(token))
            HttpContext.Session.SetString("Token", token);

        return RedirectToAction("Index", "Home");
    }

    /// <summary>Изтрива Token от Session и пренасочва към началната страница.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Remove("Token");
        return RedirectToAction("Index", "Home");
    }
}
