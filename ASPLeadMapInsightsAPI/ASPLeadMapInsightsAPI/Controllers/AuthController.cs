using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models.Auth;
using ASPLeadMapInsightsAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASPLeadMapInsightsAPI.Controllers;

/// <summary>
/// API за вход и регистрация. Не изисква [Authorize] – достъпни са без токен.
/// При успех връща LoginResponse с JWT; клиентът го пази и го изпраща при следващи заявки.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IJwtService _jwtService;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }

    /// <summary>Регистрира нов потребител в Identity. При грешка (имейл вече съществува или слаба парола) връща 400.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] RegisterRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.UserName ?? request.Email,
            Email = request.Email
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        // Веднага след регистрация връщаме токен – потребителят е "логнат".
        var token = _jwtService.GenerateToken(user.Id, user.Email!);
        var expiresMinutes = int.TryParse(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:ExpiresMinutes"],
            out var m) ? m : 60;
        return Ok(new LoginResponse
        {
            Token = token,
            Email = user.Email!,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes)
        });
    }

    /// <summary>Вход по имейл и парола. При успех връща JWT и ExpiresAt. При грешка – 401.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
            return Unauthorized("Invalid email or password.");

        // Ролите се добавят в claims на токена – за [Authorize(Roles = "...")] при нужда.
        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.Email!, roles);
        var expiresMinutes = int.TryParse(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:ExpiresMinutes"],
            out var m) ? m : 60;
        return Ok(new LoginResponse
        {
            Token = token,
            Email = user.Email!,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes)
        });
    }
}
