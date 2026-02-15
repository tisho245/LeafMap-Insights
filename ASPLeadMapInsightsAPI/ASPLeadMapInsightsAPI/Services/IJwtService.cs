namespace ASPLeadMapInsightsAPI.Services;

/// <summary>Интерфейс за генериране на JWT – имплементацията е JwtService, регистрирана в Program.cs.</summary>
public interface IJwtService
{
    /// <summary>Създава подписан JWT с userId, email и по избор роли; срокът се чете от Jwt:ExpiresMinutes.</summary>
    string GenerateToken(string userId, string email, IEnumerable<string>? roles = null);
}
