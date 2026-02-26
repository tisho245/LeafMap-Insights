namespace ASPLeadMapInsightsAPI.Models.Auth;

/// <summary>Отговор при успешен login или register – JWT токен, имейл и срок на валидност. Клиентът пази Token и го изпраща в Authorization: Bearer.</summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    /// <summary>Ролите на потребителя към момента на издаване на токена (напр. User, Admin).</summary>
    public List<string> Roles { get; set; } = new();
}
