namespace ASPLeadMapInsightsAPI.Models.Auth;

/// <summary>Тяло на заявка за регистрация – POST api/auth/register. UserName е по избор; ако липсва, използва се Email.</summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? UserName { get; set; }
}
