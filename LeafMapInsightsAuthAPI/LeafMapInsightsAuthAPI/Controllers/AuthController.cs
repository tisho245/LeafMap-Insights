using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Models.Auth;
using ASPLeadMapInsightsAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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

    public AuthController(UserManager<IdentityUser> userManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

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

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        var ok = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!ok)
            return Unauthorized("Invalid email or password.");

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
