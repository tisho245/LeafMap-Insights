using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

[QueryProperty(nameof(UserId), "id")]
public partial class AdminUserEditPage : ContentPage
{
    private LeafMapApiService? _api;
    public string? UserId { get; set; }

    public AdminUserEditPage()
    {
        InitializeComponent();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ErrorLabel.IsVisible = false;
        var id = UserId?.Trim();
        var isCreate = string.IsNullOrEmpty(id);
        PageTitleLabel.Text = isCreate ? "Нов потребител" : "Редактирай потребител";
        PasswordWrap.IsVisible = isCreate;
        if (!isCreate)
        {
            var user = await Api.GetUserByIdAsync(id!);
            if (user != null)
            {
                UserNameEntry.Text = user.UserName;
                EmailEntry.Text = user.Email ?? "";
                RolesEntry.Text = user.Roles != null ? string.Join(", ", user.Roles) : "User";
            }
        }
        else
        {
            UserNameEntry.Text = "";
            EmailEntry.Text = "";
            PasswordEntry.Text = "";
            RolesEntry.Text = "User";
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;
        var userName = UserNameEntry.Text?.Trim() ?? "";
        if (string.IsNullOrEmpty(userName)) { ErrorLabel.Text = "Въведете потребителско име."; ErrorLabel.IsVisible = true; return; }
        var email = EmailEntry.Text?.Trim();
        var rolesStr = RolesEntry.Text?.Trim() ?? "User";
        var roles = rolesStr.Split(',', ';').Select(r => r.Trim()).Where(r => r.Length > 0).ToList();
        if (roles.Count == 0) roles.Add("User");

        var id = UserId?.Trim();
        var isCreate = string.IsNullOrEmpty(id);

        if (isCreate)
        {
            var password = PasswordEntry.Text ?? "";
            if (string.IsNullOrEmpty(password)) { ErrorLabel.Text = "Въведете парола."; ErrorLabel.IsVisible = true; return; }
            var (ok, err) = await Api.CreateUserAsync(new CreateUserRequest { UserName = userName, Email = email, Password = password, Roles = roles });
            if (ok)
            {
                await DisplayAlert("Готово", "Потребителят е създаден успешно.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }
            ErrorLabel.Text = err ?? "Грешка при създаване.";
            ErrorLabel.IsVisible = true;
        }
        else
        {
            var (ok, err) = await Api.UpdateUserAsync(id!, new UpdateUserRequest { UserName = userName, Email = email, Roles = roles });
            if (ok) { await Shell.Current.GoToAsync(".."); return; }
            ErrorLabel.Text = err ?? "Грешка при запис.";
            ErrorLabel.IsVisible = true;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");
}
