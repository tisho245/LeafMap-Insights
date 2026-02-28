using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Services;
using LeafMapInsightsAuthAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeafMapInsightsAuthAPI.Controllers;

/// <summary>
/// Auth server: login and register only. Returns JWT; clients use it against the Data API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly LeafMapDbContext _db;

    public AuthController(UserManager<IdentityUser> userManager, IJwtService jwtService, LeafMapDbContext db)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _db = db;
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register([FromBody] ASPLeadMapInsightsAPI.Models.Auth.RegisterRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.UserName ?? request.Email,
            Email = request.Email
        };
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

            // Новите акаунти по подразбиране са с роля "User".
            var defaultRole = "User";
            await _userManager.AddToRoleAsync(user, defaultRole);
            var roles = await _userManager.GetRolesAsync(user);

            var token = _jwtService.GenerateToken(user.Id, user.Email!, roles);
        var expiresMinutes = int.TryParse(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:ExpiresMinutes"],
            out var m) ? m : 60;
        return Ok(new LoginResponse
        {
            Token = token,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email!,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes),
            Roles = roles.ToList()
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Вход с потребителско име и парола; при дублирани записи взимаме първия с FirstOrDefault
        var normalizedName = _userManager.NormalizeName(request.UserName ?? string.Empty);
        var user = await _db.Users
            .Where(u => u.NormalizedUserName == normalizedName)
            .FirstOrDefaultAsync();
        if (user == null)
            return Unauthorized("Invalid username or password.");

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
            return Unauthorized("Invalid username or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _jwtService.GenerateToken(user.Id, user.Email ?? user.UserName ?? string.Empty, roles);
        var expiresMinutes = int.TryParse(
            HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Jwt:ExpiresMinutes"],
            out var m) ? m : 60;
        return Ok(new LoginResponse
        {
            Token = token,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expiresMinutes),
            Roles = roles.ToList()
        });
    }
}
