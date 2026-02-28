using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Страница за вход – email и парола. При успех записва JWT в AuthService и навигира към Trees.</summary>
public partial class LoginPage : ContentPage
{
    private LeafMapApiService? _api;
    private AuthService? _auth;

    public LoginPage()
    {
        InitializeComponent();
    }

    /// <summary>Lazy инициализация на API и Auth услугите от DI.</summary>
    private void EnsureServices()
    {
        _api ??= AppServices.GetRequired<LeafMapApiService>();
        _auth ??= AppServices.GetRequired<AuthService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        EnsureServices();
        ErrorLabel.IsVisible = false;
    }

    /// <summary>Вика api/auth/login; при успех записва Token в SecureStorage и отива към списъка с дървета.</summary>
    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        EnsureServices();
        ErrorLabel.IsVisible = false;

        var userNameOrEmail = EmailEntry.Text?.Trim() ?? "";
        var password = PasswordEntry.Text ?? "";

        if (string.IsNullOrEmpty(userNameOrEmail) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Въведете потребителско име/email и парола.";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            var resp = await _api!.LoginAsync(userNameOrEmail, password);
            if (resp == null)
            {
                ErrorLabel.Text = "Невалидно потребителско име или парола.";
                ErrorLabel.IsVisible = true;
                return;
            }

            await _auth!.SetTokenAsync(resp.Token);
            _auth.SetRoles(resp.Roles ?? new List<string>());

            if (Shell.Current is AppShell shell)
                shell.UpdateAuthFlyoutTitleAsync();

            await Shell.Current.GoToAsync("//Trees");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message ?? "Грешка при вход.";
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnGoRegisterClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("Register");
}
