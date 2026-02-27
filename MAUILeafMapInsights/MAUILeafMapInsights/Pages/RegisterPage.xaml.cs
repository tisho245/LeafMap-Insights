using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Страница за регистрация – email, парола, потребителско име (по избор). При успех записва токен и отива към Trees.</summary>
public partial class RegisterPage : ContentPage
{
    private LeafMapApiService? _api;
    private AuthService? _auth;

    public RegisterPage()
    {
        InitializeComponent();
    }

    private void EnsureServices()
    {
        _api ??= AppServices.GetRequired<LeafMapApiService>();
        _auth ??= AppServices.GetRequired<AuthService>();
    }

    /// <summary>Вика api/auth/register; при успех записва Token в SecureStorage и навигира към //Trees.</summary>
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        EnsureServices();
        ErrorLabel.IsVisible = false;
        var email = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Въведете email и парола.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var resp = await _api!.RegisterAsync(email, password, UserNameEntry.Text?.Trim());
        if (resp == null)
        {
            ErrorLabel.Text = "Регистрацията не успешна.";
            ErrorLabel.IsVisible = true;
            return;
        }

        await _auth!.SetTokenAsync(resp.Token);
        _auth.SetRoles(resp.Roles ?? new List<string>());
        if (Shell.Current is AppShell shell)
            _ = shell.UpdateAuthFlyoutTitleAsync();
        await Shell.Current.GoToAsync("//Trees");
    }

    private async void OnGoLoginClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Login");
}
