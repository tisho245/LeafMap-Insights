using System.Linq;
using MAUILeafMapInsights.Pages;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights;

public partial class AppShell : Shell
{
    public AppShell()
    {
        StartupLog.Info("AppShell() start");
        InitializeComponent();
        StartupLog.Info("AppShell() InitializeComponent done");
        Routing.RegisterRoute("Register", typeof(RegisterPage));
        Routing.RegisterRoute("TreeDetail", typeof(TreeDetailPage));
        Routing.RegisterRoute("Admin", typeof(AdminPage));
        Routing.RegisterRoute("AdminUserEdit", typeof(AdminUserEditPage));
        Routing.RegisterRoute("NewUser", typeof(AdminUserEditPage));
        Routing.RegisterRoute("Logout", typeof(LoginPage));
        Routing.RegisterRoute("EditTree", typeof(EditTreePage));
        Navigating += OnShellNavigating;
        StartupLog.Info("AppShell() done");
    }

    public async void UpdateAuthFlyoutTitleAsync()
    {
        var auth = AppServices.GetRequired<AuthService>();

        var logged = await auth.IsLoggedInAsync();
        var isAdmin = await auth.IsAdminAsync();

        AddTreeFlyoutItem.IsVisible = logged;
        NewUserFlyoutItem.IsVisible = isAdmin;
        AdminFlyoutItem.IsVisible = isAdmin;

        LoginFlyoutItem.IsVisible = !logged;
        RegisterFlyoutItem.IsVisible = !logged;
        LogoutFlyoutItem.IsVisible = logged;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (AppServices.Services != null)
        {
            try { UpdateAuthFlyoutTitleAsync(); }
            catch { /* avoid startup crash if auth not ready */ }
        }
    }

    private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target?.Location?.OriginalString ?? string.Empty;

        // Изход: при натискане на Изход – излизане и пренасочване към Начало
        if (target.Contains("Logout", StringComparison.OrdinalIgnoreCase))
        {
            e.Cancel();
            if (AppServices.Services != null)
            {
                var authLogout = AppServices.GetRequired<AuthService>();
                await authLogout.RemoveTokenAsync();
                UpdateAuthFlyoutTitleAsync();
            }
            FlyoutIsPresented = false;
            await GoToAsync("//Home");
            return;
        }

        // Защитени маршрути: за нелогнат потребител пренасочваме към Вход
        var protectedRoutes = new[] { "AddTree", "Admin", "EditTree", "AdminUserEdit", "NewUser" };
        var isProtected = protectedRoutes.Any(r => target.Contains(r, StringComparison.OrdinalIgnoreCase));
        if (AppServices.Services != null && isProtected)
        {
            var auth = AppServices.GetRequired<AuthService>();
            var logged = await auth.IsLoggedInAsync();
            if (!logged)
            {
                e.Cancel();
                await Dispatcher.DispatchAsync(async () =>
                {
                    await DisplayAlert("Вход изискван", "Моля, влезте в профила си, за да достъпите тази страница.", "ОК");
                    await GoToAsync("//Login");
                });
                return;
            }
            // Админ, Нов потребител и редакция на потребител – само за админ
            if (target.Contains("Admin", StringComparison.OrdinalIgnoreCase) || target.Contains("NewUser", StringComparison.OrdinalIgnoreCase))
            {
                var isAdmin = await auth.IsAdminAsync();
                if (!isAdmin)
                {
                    e.Cancel();
                    await Dispatcher.DispatchAsync(async () =>
                    {
                        await DisplayAlert("Нямате права", "Само администратор може да достъпи тази страница.", "ОК");
                        await GoToAsync("//Home");
                    });
                    return;
                }
            }
        }

    }
}