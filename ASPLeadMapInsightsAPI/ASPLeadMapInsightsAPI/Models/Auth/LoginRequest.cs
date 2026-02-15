namespace ASPLeadMapInsightsAPI.Models.Auth;

/// <summary>Тяло на заявка за вход – изпраща се на POST api/auth/login.</summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
