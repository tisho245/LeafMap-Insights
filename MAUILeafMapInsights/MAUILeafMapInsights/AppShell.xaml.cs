using MAUILeafMapInsights.Pages;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights;

/// <summary>Главен Shell с flyout меню. Регистрира маршрути за Register, AddTree и TreeDetail. Обновява заглавието Вход/Изход според автентикацията.</summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("Register", typeof(RegisterPage));
        Routing.RegisterRoute("AddTree", typeof(AddTreePage));
        Routing.RegisterRoute("TreeDetail", typeof(TreeDetailPage));
        Routing.RegisterRoute("Admin", typeof(AdminPage));
    }

    /// <summary>Обновява заглавието на flyout елемента за вход – "Вход" или "Изход" според токена; показва/скрива Админ само за Admin.</summary>
    public async Task UpdateAuthFlyoutTitleAsync()
    {
        try
        {
            if (AppServices.Services == null) return;
            var auth = AppServices.Get<AuthService>();
            if (auth == null) return;
            if (AuthFlyoutItem != null)
                AuthFlyoutItem.Title = await auth.IsLoggedInAsync() ? "Изход" : "Вход";
            if (AdminFlyoutItem != null)
                AdminFlyoutItem.IsVisible = await auth.IsAdminAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"AppShell UpdateAuthFlyoutTitleAsync: {ex}");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = UpdateAuthFlyoutTitleAsync();
    }
}
