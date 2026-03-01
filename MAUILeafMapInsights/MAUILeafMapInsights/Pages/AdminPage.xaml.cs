using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Админ – CRUD за потребители, дървета и таксономични данни. Достъп само за Admin.</summary>
public partial class AdminPage : ContentPage
{
    private LeafMapApiService? _api;
    private int _lastSectionIndex = -1;
    private bool _isLoadingSection;
    private const int SectUsers = 0, SectTrees = 1, SectDivisions = 2, SectClasses = 3, SectFamilies = 4, SectGenera = 5, SectSpecies = 6;

    public AdminPage()
    {
        InitializeComponent();
        SectionPicker.ItemsSource = new[] { "Потребители", "Дървета", "Отдели", "Класове", "Семейства", "Родове", "Видове" };
        SectionPicker.SelectedIndex = 0;
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var idx = SectionPicker.SelectedIndex >= 0 ? SectionPicker.SelectedIndex : 0;
        _lastSectionIndex = idx;
        LoadSection();
    }

    private void OnSectionChanged(object? sender, EventArgs e)
    {
        var idx = SectionPicker.SelectedIndex;
        if (idx < 0) return;
        if (idx == _lastSectionIndex) return;
        _lastSectionIndex = idx;
        LoadSection();
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadSectionAsync();
        RefreshView.IsRefreshing = false;
    }

    private void LoadSection()
    {
        _ = LoadSectionAsync();
    }

    private async Task LoadSectionAsync()
    {
        if (_isLoadingSection) return;
        _isLoadingSection = true;
        var idx = SectionPicker.SelectedIndex;
        if (idx < 0) idx = 0;
        LoadingLabel.IsVisible = true;
        ErrorLabel.IsVisible = false;
        ListStack.Children.Clear();
        AddButton.IsVisible = true;

        try
        {
            if (idx == SectUsers)
            {
                var users = await Api.GetUsersAsync();
                LoadingLabel.IsVisible = false;
                if (users == null) { ErrorLabel.Text = "Грешка при зареждане."; ErrorLabel.IsVisible = true; return; }
                foreach (var u in users)
                {
                    var b = BuildRow(u.UserName + " / " + (u.Email ?? ""), u.Roles?.Count > 0 ? string.Join(", ", u.Roles) : "—",
                        () => OpenUserEdit(u.Id), () => DeleteUser(u.Id));
                    ListStack.Children.Add(b);
                }
            }
            else if (idx == SectTrees)
            {
                var trees = await Api.GetTreesAsync();
                LoadingLabel.IsVisible = false;
                if (trees == null) { ErrorLabel.Text = "Грешка при зареждане."; ErrorLabel.IsVisible = true; return; }
                foreach (var t in trees)
                {
                    var name = t.Name;
                    var sub = t.Species?.Name ?? $"#{t.Id}";
                    var id = t.Id;
                    var b = BuildRow(name, sub, () => Shell.Current.GoToAsync($"EditTree?id={id}"), () => DeleteTree(id));
                    ListStack.Children.Add(b);
                }
            }
            else if (idx == SectDivisions)
            {
                var list = await Api.GetDivisionsAsync();
                LoadingLabel.IsVisible = false;
                if (list == null) { ErrorLabel.Text = "Грешка."; ErrorLabel.IsVisible = true; return; }
                foreach (var d in list)
                    ListStack.Children.Add(BuildTaxonomyRowGeneric(d.Id, d.Name, "Отдел", async (n) => await Api.UpdateDivisionAsync(d.Id, new DivisionDto { Id = d.Id, Name = n }), async () => await Api.DeleteDivisionAsync(d.Id)));
            }
            else if (idx == SectClasses)
            {
                var list = await Api.GetTaxonomyClassesAsync();
                LoadingLabel.IsVisible = false;
                if (list == null) { ErrorLabel.Text = "Грешка."; ErrorLabel.IsVisible = true; return; }
                foreach (var d in list)
                    ListStack.Children.Add(BuildTaxonomyRowGeneric(d.Id, d.Name, "Клас", async (n) => await Api.UpdateTaxonomyClassAsync(d.Id, new TaxonomyClassDto { Id = d.Id, Name = n }), async () => await Api.DeleteTaxonomyClassAsync(d.Id)));
            }
            else if (idx == SectFamilies)
            {
                var list = await Api.GetFamiliesAsync();
                LoadingLabel.IsVisible = false;
                if (list == null) { ErrorLabel.Text = "Грешка."; ErrorLabel.IsVisible = true; return; }
                foreach (var d in list)
                    ListStack.Children.Add(BuildTaxonomyRowGeneric(d.Id, d.Name, "Семейство", async (n) => await Api.UpdateFamilyAsync(d.Id, new FamilyDto { Id = d.Id, Name = n }), async () => await Api.DeleteFamilyAsync(d.Id)));
            }
            else if (idx == SectGenera)
            {
                var list = await Api.GetGeneraAsync();
                LoadingLabel.IsVisible = false;
                if (list == null) { ErrorLabel.Text = "Грешка."; ErrorLabel.IsVisible = true; return; }
                foreach (var d in list)
                    ListStack.Children.Add(BuildTaxonomyRowGeneric(d.Id, d.Name, "Род", async (n) => await Api.UpdateGenusAsync(d.Id, new GenusDto { Id = d.Id, Name = n }), async () => await Api.DeleteGenusAsync(d.Id)));
            }
            else if (idx == SectSpecies)
            {
                var list = await Api.GetSpeciesAsync();
                LoadingLabel.IsVisible = false;
                if (list == null) { ErrorLabel.Text = "Грешка."; ErrorLabel.IsVisible = true; return; }
                foreach (var d in list)
                    ListStack.Children.Add(BuildTaxonomyRowGeneric(d.Id, d.Name, "Вид", async (n) => await Api.UpdateSpeciesAsync(d.Id, new SpeciesDto { Id = d.Id, Name = n }), async () => await Api.DeleteSpeciesAsync(d.Id)));
            }
        }
        catch (Exception ex)
        {
            LoadingLabel.IsVisible = false;
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            _isLoadingSection = false;
        }
    }

    private static Color GetTextColor() => (Color)(Application.Current?.Resources["Text"] ?? Colors.Black);
    private static Color GetMutedColor() => (Color)(Application.Current?.Resources["Muted"] ?? Colors.Gray);

    private static Border BuildRow(string title, string subtitle, Action onEdit, Action onDelete)
    {
        var textColor = GetTextColor();
        var editBtn = new Button { Text = "Редактирай", StyleClass = new[] { "SecondaryButton" }, Padding = 8, FontSize = 13, TextColor = textColor };
        var delBtn = new Button { Text = "Изтрий", StyleClass = new[] { "GhostButton" }, TextColor = Colors.Red, Padding = 8, FontSize = 13 };
        editBtn.Clicked += (_, _) => onEdit();
        delBtn.Clicked += (_, _) => onDelete();
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Auto } }, Padding = 8 };
        var vert = new VerticalStackLayout { Spacing = 2, VerticalOptions = LayoutOptions.Center };
        vert.Children.Add(new Label { Text = title, FontAttributes = FontAttributes.Bold, TextColor = textColor, FontSize = 15 });
        vert.Children.Add(new Label { Text = subtitle, FontSize = 13, TextColor = GetMutedColor() });
        grid.Add(vert, 0, 0);
        grid.Add(editBtn, 1, 0);
        grid.Add(delBtn, 2, 0);
        return new Border { StyleClass = new[] { "CardSoft" }, Content = grid, Padding = 8, Margin = new Thickness(0, 4) };
    }

    private Border BuildTaxonomyRowGeneric(int id, string name, string sectionName, Func<string, Task<bool>> update, Func<Task<bool>> delete)
    {
        var textColor = GetTextColor();
        var editBtn = new Button { Text = "Редактирай", StyleClass = new[] { "SecondaryButton" }, Padding = 8, FontSize = 13, TextColor = textColor };
        var delBtn = new Button { Text = "Изтрий", StyleClass = new[] { "GhostButton" }, TextColor = Colors.Red, Padding = 8, FontSize = 13 };
        editBtn.Clicked += async (_, _) =>
        {
            var newName = await DisplayPromptAsync($"Редактирай {sectionName}", "Име", initialValue: name, maxLength: 200);
            if (!string.IsNullOrWhiteSpace(newName) && await update(newName.Trim())) LoadSection();
        };
        delBtn.Clicked += async (_, _) =>
        {
            if (await DisplayAlert("Изтрий", $"Изтриване на \"{name}\"?", "Да", "Не") && await delete()) LoadSection();
        };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Auto } }, Padding = 8 };
        grid.Add(new Label { Text = name, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.Center, TextColor = textColor, FontSize = 15 }, 0, 0);
        grid.Add(editBtn, 1, 0);
        grid.Add(delBtn, 2, 0);
        return new Border { StyleClass = new[] { "CardSoft" }, Content = grid, Padding = 8, Margin = new Thickness(0, 4) };
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var idx = SectionPicker.SelectedIndex;
        if (idx == SectUsers)
        {
            await Shell.Current.GoToAsync("AdminUserEdit", new Dictionary<string, object> { { "id", "" } });
            return;
        }
        if (idx == SectTrees)
        {
            await Shell.Current.GoToAsync("AddTree");
            return;
        }
        var sectionNames = new[] { "", "", "Отдел", "Клас", "Семейство", "Род", "Вид" };
        var name = await DisplayPromptAsync($"Нов {sectionNames[idx]}", "Име", maxLength: 200);
        if (string.IsNullOrWhiteSpace(name)) return;
        name = name.Trim();
        bool ok = false;
        if (idx == SectDivisions) ok = await Api.CreateDivisionAsync(new DivisionDto { Name = name });
        else if (idx == SectClasses) ok = await Api.CreateTaxonomyClassAsync(new TaxonomyClassDto { Name = name });
        else if (idx == SectFamilies) ok = await Api.CreateFamilyAsync(new FamilyDto { Name = name });
        else if (idx == SectGenera) ok = await Api.CreateGenusAsync(new GenusDto { Name = name });
        else if (idx == SectSpecies) ok = await Api.CreateSpeciesAsync(new SpeciesDto { Name = name });
        if (ok) LoadSection();
        else await DisplayAlert("Грешка", "Неуспешно създаване.", "OK");
    }

    private async void OpenUserEdit(string id)
    {
        await Shell.Current.GoToAsync("AdminUserEdit", new Dictionary<string, object> { { "id", id } });
    }

    private async void DeleteUser(string id)
    {
        if (!await DisplayAlert("Изтрий потребител", "Сигурни ли сте?", "Да", "Не")) return;
        var (ok, err) = await Api.DeleteUserAsync(id);
        if (ok) LoadSection();
        else await DisplayAlert("Грешка", err ?? "Неуспешно изтриване.", "OK");
    }

    private async void DeleteTree(int id)
    {
        if (!await DisplayAlert("Изтрий дърво", "Сигурни ли сте?", "Да", "Не")) return;
        if (await Api.DeleteTreeAsync(id)) LoadSection();
        else await DisplayAlert("Грешка", "Неуспешно изтриване.", "OK");
    }
}

/// <summary>Елемент за показ в списъка потребители на AdminPage.</summary>
public class UserDisplay
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public string RolesDisplay { get; set; } = "";
}
