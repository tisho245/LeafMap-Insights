namespace ASPLeadMapInsightsAPI.Models.Auth;

/// <summary>Тяло на заявка за вход – изпраща се на POST api/auth/login. Вход с потребителско име и парола.</summary>
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
