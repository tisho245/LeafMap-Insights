namespace LeafMapInsightsAuthAPI.Models;

/// <summary>Отговор при успешен login или register – JWT токен, потребителско име, имейл и роли.</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new();
}
