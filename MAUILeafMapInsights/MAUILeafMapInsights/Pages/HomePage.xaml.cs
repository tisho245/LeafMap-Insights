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
        _auth ??= AppServices.GetRequired<AuthService>();
    }

    /// <summary>При появяване обновяваме текста на бутона за вход – "Вход" или "Изход" според наличието на токен.</summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        AuthButton.Text = await (_auth ?? AppServices.GetRequired<AuthService>()).IsLoggedInAsync() ? "Изход" : "Вход";
    }

    private async void OnTreesClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Trees");
    private async void OnTaxonomyClicked(object sender, EventArgs e) => await Shell.Current.GoToAsync("//Taxonomy");

    private async void OnAddTreeClicked(object sender, EventArgs e)
    {
        var auth = _auth ?? AppServices.GetRequired<AuthService>();
        if (!await auth.IsLoggedInAsync())
        {
            await Shell.Current.GoToAsync("//Login");
            return;
        }
        await Shell.Current.GoToAsync("AddTree");
    }

    /// <summary>Ако потребителят е логнат – изтриваме токена и показваме "Вход"; иначе навигираме към Login.</summary>
    private async void OnAuthClicked(object sender, EventArgs e)
    {
        var auth = _auth ?? AppServices.GetRequired<AuthService>();
        if (await auth.IsLoggedInAsync())
        {
            auth.RemoveTokenAsync();
            AuthButton.Text = "Вход";
        }
        else
            await Shell.Current.GoToAsync("//Login");
    }
}
