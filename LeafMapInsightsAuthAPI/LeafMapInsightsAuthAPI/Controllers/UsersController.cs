using ASPLeadMapInsightsAPI.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LeafMapInsightsAuthAPI.Controllers;

/// <summary>
/// Управление на потребители – достъпно само за администратори.
/// Admin може да вижда, създава, редактира и изтрива потребители и да управлява ролите им.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
    {
        var users = _userManager.Users.ToList();
        var result = new List<UserDto>(users.Count);

        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            result.Add(MapToDto(u, roles));
        }

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(MapToDto(user, roles));
    }

    private static UserDto MapToDto(IdentityUser u, IList<string> roles)
    {
        return new UserDto
        {
            Id = u.Id,
            UserName = u.UserName ?? string.Empty,
            NormalizedUserName = u.NormalizedUserName,
            Email = u.Email,
            NormalizedEmail = u.NormalizedEmail,
            EmailConfirmed = u.EmailConfirmed,
            PhoneNumber = u.PhoneNumber,
            PhoneNumberConfirmed = u.PhoneNumberConfirmed,
            TwoFactorEnabled = u.TwoFactorEnabled,
            LockoutEnd = u.LockoutEnd,
            LockoutEnabled = u.LockoutEnabled,
            AccessFailedCount = u.AccessFailedCount,
            ConcurrencyStamp = u.ConcurrencyStamp,
            Roles = roles.ToList()
        };
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        var user = new IdentityUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email?.Trim(),
            EmailConfirmed = request.EmailConfirmed,
            PhoneNumber = request.PhoneNumber?.Trim(),
            PhoneNumberConfirmed = request.PhoneNumberConfirmed,
            TwoFactorEnabled = request.TwoFactorEnabled,
            LockoutEnabled = request.LockoutEnabled
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return BadRequest(createResult.Errors.Select(e => e.Description));

        var rolesToAssign = request.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                          ?? new List<string> { "User" };

        foreach (var role in rolesToAssign)
        {
            if (!await _roleManager.RoleExistsAsync(role))
                await _roleManager.CreateAsync(new IdentityRole(role));

            await _userManager.AddToRoleAsync(user, role);
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapToDto(user, userRoles));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        user.UserName = request.UserName.Trim();
        user.Email = request.Email?.Trim();
        user.EmailConfirmed = request.EmailConfirmed;
        user.PhoneNumber = request.PhoneNumber?.Trim();
        user.PhoneNumberConfirmed = request.PhoneNumberConfirmed;
        user.TwoFactorEnabled = request.TwoFactorEnabled;
        user.LockoutEnd = request.LockoutEnd;
        user.LockoutEnabled = request.LockoutEnabled;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return BadRequest(updateResult.Errors.Select(e => e.Description));

        if (request.Roles != null)
        {
            var existingRoles = await _userManager.GetRolesAsync(user);
            var desiredRoles = request.Roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var toRemove = existingRoles.Except(desiredRoles, StringComparer.OrdinalIgnoreCase).ToList();
            var toAdd = desiredRoles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (toRemove.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, toRemove);

            foreach (var role in toAdd)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

                await _userManager.AddToRoleAsync(user, role);
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return NoContent();
    }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? NormalizedUserName { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    public string? ConcurrencyStamp { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public bool LockoutEnabled { get; set; }
    public List<string>? Roles { get; set; }
}

public class UpdateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public List<string>? Roles { get; set; }
}

