using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ASPLeadMapInsightsAPI.Services;

/// <summary>
/// Генерира JWT токен след успешен логин. Ключът, issuer и audience се четат от appsettings (Jwt:Key, Jwt:Issuer, Jwt:Audience).
/// Токенът съдържа userId, email и по избор роли; валидира се от AddJwtBearer в Program.cs.
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config) => _config = config;

    /// <summary>
    /// Създава подписан JWT с claims за потребителя. Срокът на живот се задава от Jwt:ExpiresMinutes (по подразбиране 60).
    /// </summary>
    public string GenerateToken(string userId, string email, IEnumerable<string>? roles = null)
    {
        // Секретният ключ за подпис – задължителен в appsettings.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not set")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var issuer = _config["Jwt:Issuer"] ?? "LeafMapInsightsAPI";
        var audience = _config["Jwt:Audience"] ?? "LeafMapInsights";
        var expiresMinutes = int.TryParse(_config["Jwt:ExpiresMinutes"], out var m) ? m : 60;

        // Claims – идентификатор, имейл и уникален Jti (за превенция на повторна употреба при нужда).
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        if (roles != null)
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(expiresMinutes),
            creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
