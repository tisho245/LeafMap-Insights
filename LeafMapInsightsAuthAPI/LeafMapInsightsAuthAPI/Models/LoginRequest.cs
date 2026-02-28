namespace LeafMapInsightsAuthAPI.Models;

/// <summary>Тяло на заявка за вход – POST api/auth/login. Вход с потребителско име и парола.</summary>
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
