using MAUILeafMapInsights.Pages;

namespace MAUILeafMapInsights;

/// <summary>Главен Shell с flyout меню. Регистрира маршрути за Register, AddTree и TreeDetail – за навигация от кода.</summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("Register", typeof(RegisterPage));
        Routing.RegisterRoute("AddTree", typeof(AddTreePage));
        Routing.RegisterRoute("TreeDetail", typeof(TreeDetailPage));
    }
}
