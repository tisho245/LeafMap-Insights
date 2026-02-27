using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Начална страница – бутони за Дървета, Таксономия, Добави дърво и Вход/Изход. Услугите се взимат чрез AppServices.GetRequired.</summary>
public partial class HomePage : ContentPage
{
    private AuthService? _auth;

    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();
        if (AppServices.Services != null)
            _auth ??= AppServices.Get<AuthService>();
    }

    /// <summary>При появяване обновяваме текста на бутона за вход и заглавието в flyout менюто.</summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            if (AppServices.Services == null) return;
            var auth = _auth ?? AppServices.Get<AuthService>();
            if (auth == null) { if (AuthButton != null) AuthButton.Text = "Вход"; return; }
            if (AuthButton != null)
                AuthButton.Text = await auth.IsLoggedInAsync() ? "Изход" : "Вход";
            if (Shell.Current is AppShell shell)
                _ = shell.UpdateAuthFlyoutTitleAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"HomePage OnAppearing: {ex}");
            if (AuthButton != null) AuthButton.Text = "Вход";
        }
    }

    private async void OnTreesClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Trees");
    private async void OnMapClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Map");
    private async void OnTaxonomyClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Taxonomy");

    private async void OnAddTreeClicked(object sender, EventArgs e)
    {
        var auth = _auth ?? AppServices.Get<AuthService>();
        if (auth == null || !await auth.IsLoggedInAsync())
        {
            await Shell.Current.GoToAsync("//Login");
            return;
        }
        await Shell.Current.GoToAsync("AddTree");
    }

    /// <summary>Ако потребителят е логнат – изтриваме токена и показваме "Вход"; иначе навигираме към Login.</summary>
    private async void OnAuthClicked(object sender, EventArgs e)
    {
        var auth = _auth ?? AppServices.Get<AuthService>();
        if (auth == null) { await Shell.Current.GoToAsync("//Login"); return; }
        if (await auth.IsLoggedInAsync())
        {
            await auth.RemoveTokenAsync();
            AuthButton.Text = "Вход";
            if (Shell.Current is AppShell shell)
                _ = shell.UpdateAuthFlyoutTitleAsync();
        }
        else
            await Shell.Current.GoToAsync("//Login");
    }
}
