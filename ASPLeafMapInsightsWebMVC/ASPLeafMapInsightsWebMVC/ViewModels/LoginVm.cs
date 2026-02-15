namespace ASPLeafMapInsightsWebMVC.ViewModels;

/// <summary>Модел за формата за вход – Email и Password; подава се на Auth/Login.</summary>
public class LoginVm
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}
