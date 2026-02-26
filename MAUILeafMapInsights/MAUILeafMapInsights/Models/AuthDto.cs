namespace MAUILeafMapInsights.Models;

/// <summary>Тяло на заявка за вход – изпраща се на api/auth/login.</summary>
public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

/// <summary>Тяло на заявка за регистрация – api/auth/register.</summary>
public class RegisterRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? UserName { get; set; }
}

/// <summary>Отговор при login/register – Token се пази в SecureStorage чрез AuthService.</summary>
public class LoginResponse
{
    public string Token { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new();
}
