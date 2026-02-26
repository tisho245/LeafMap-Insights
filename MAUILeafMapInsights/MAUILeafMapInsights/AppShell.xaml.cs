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
    public async void UpdateAuthFlyoutTitleAsync()
    {
        var auth = AppServices.GetRequired<AuthService>();
        AuthFlyoutItem.Title = await auth.IsLoggedInAsync() ? "Изход" : "Вход";
        AdminFlyoutItem.IsVisible = await auth.IsAdminAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        UpdateAuthFlyoutTitleAsync();
    }
}
