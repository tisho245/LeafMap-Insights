namespace ASPLeafMapInsightsWebMVC.ViewModels;

/// <summary>Модел за формата за регистрация – Email, Password, UserName (по избор); подава се на Auth/Register.</summary>
public class RegisterVm
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string? UserName { get; set; }
}
