using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Админ страница – списък потребители от Auth API (api/users). Достъпна само за Admin.</summary>
public partial class AdminPage : ContentPage
{
    private LeafMapApiService? _api;

    public AdminPage()
    {
        InitializeComponent();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        LoadingLabel.IsVisible = true;
        UsersList.IsVisible = false;
        ErrorLabel.IsVisible = false;
        ErrorLabel.Text = "";

        var users = await Api.GetUsersAsync();
        LoadingLabel.IsVisible = false;

        if (users == null)
        {
            ErrorLabel.Text = "Няма достъп или грешка от сървъра. Влезте като администратор.";
            ErrorLabel.IsVisible = true;
            return;
        }

        var display = users.Select(u => new UserDisplay
        {
            Email = u.Email,
            RolesDisplay = u.Roles != null && u.Roles.Count > 0 ? string.Join(", ", u.Roles) : "—"
        }).ToList();
        UsersList.ItemsSource = display;
        UsersList.IsVisible = true;
    }

    private class UserDisplay
    {
        public string Email { get; set; } = "";
        public string RolesDisplay { get; set; } = "";
    }
}
