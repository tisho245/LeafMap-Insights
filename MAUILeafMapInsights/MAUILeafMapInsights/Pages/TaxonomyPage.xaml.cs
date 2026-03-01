using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Таксономия – преглед и за админ CRUD по секции (като Node client).</summary>
public partial class TaxonomyPage : ContentPage
{
    private LeafMapApiService? _api;
    private AuthService? _auth;

    public TaxonomyPage()
    {
        InitializeComponent();
        _ = LoadTaxonomyAsync();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();
    private AuthService Auth => _auth ??= AppServices.GetRequired<AuthService>();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTaxonomyAsync();
    }

    static Color GetTextColor() => (Color)(Application.Current?.Resources["Text"] ?? Colors.Black);
    static Color GetMutedColor() => (Color)(Application.Current?.Resources["Muted"] ?? Colors.Gray);

    private async Task LoadTaxonomyAsync()
    {
        TaxonomyStack.Children.Clear();
        var textColor = GetTextColor();
        var mutedColor = GetMutedColor();

        try
        {
            var isAdmin = await Auth.IsAdminAsync();
            var divs = await Api.GetDivisionsAsync();
            var classes = await Api.GetTaxonomyClassesAsync();
            var families = await Api.GetFamiliesAsync();
            var genera = await Api.GetGeneraAsync();
            var species = await Api.GetSpeciesAsync();

            AddSectionCard("Отдели (Divisions)", divs ?? new List<DivisionDto>(), "Отдел", isAdmin,
                async (d) => await Api.CreateDivisionAsync(new DivisionDto { Name = d }),
                (id, name) => Api.UpdateDivisionAsync(id, new DivisionDto { Id = id, Name = name }),
                id => Api.DeleteDivisionAsync(id));

            AddSectionCard("Класове (Taxonomy Classes)", classes ?? new List<TaxonomyClassDto>(), "Клас", isAdmin,
                async (d) => await Api.CreateTaxonomyClassAsync(new TaxonomyClassDto { Name = d }),
                (id, name) => Api.UpdateTaxonomyClassAsync(id, new TaxonomyClassDto { Id = id, Name = name }),
                id => Api.DeleteTaxonomyClassAsync(id));

            AddSectionCard("Семейства (Families)", families ?? new List<FamilyDto>(), "Семейство", isAdmin,
                async (d) => await Api.CreateFamilyAsync(new FamilyDto { Name = d }),
                (id, name) => Api.UpdateFamilyAsync(id, new FamilyDto { Id = id, Name = name }),
                id => Api.DeleteFamilyAsync(id));

            AddSectionCard("Родове (Genera)", genera ?? new List<GenusDto>(), "Род", isAdmin,
                async (d) => await Api.CreateGenusAsync(new GenusDto { Name = d }),
                (id, name) => Api.UpdateGenusAsync(id, new GenusDto { Id = id, Name = name }),
                id => Api.DeleteGenusAsync(id));

            AddSectionCard("Видове (Species)", species ?? new List<SpeciesDto>(), "Вид", isAdmin,
                async (d) => await Api.CreateSpeciesAsync(new SpeciesDto { Name = d }),
                (id, name) => Api.UpdateSpeciesAsync(id, new SpeciesDto { Id = id, Name = name }),
                id => Api.DeleteSpeciesAsync(id));
        }
        catch (Exception ex)
        {
            TaxonomyStack.Children.Add(new Label
            {
                Text = "Грешка при зареждане: " + ex.Message,
                TextColor = mutedColor,
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap
            });
        }
    }

    private void AddSectionCard<T>(
        string title,
        IList<T> items,
        string sectionName,
        bool isAdmin,
        Func<string, Task<bool>> create,
        Func<int, string, Task<bool>> update,
        Func<int, Task<bool>> delete) where T : class
    {
        int getId(dynamic d) => (int)d.Id;
        string getName(dynamic d) => (string)(d.Name ?? "");

        var textColor = GetTextColor();
        var mutedColor = GetMutedColor();

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
            Padding = new Thickness(0, 0, 0, 10)
        };
        header.Add(new Label { Text = title, FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = textColor }, 0, 0);
        header.Add(new Label { Text = items.Count.ToString(), FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = textColor, VerticalOptions = LayoutOptions.Center }, 1, 0);

        var contentStack = new VerticalStackLayout { Spacing = 8 };

        if (isAdmin)
        {
            var addBtn = new Button { Text = "Добави", StyleClass = new[] { "SecondaryButton" }, Padding = 10, FontSize = 13, HorizontalOptions = LayoutOptions.Start };
            addBtn.Clicked += async (_, _) =>
            {
                var name = await DisplayPromptAsync($"Нов {sectionName}", "Име", maxLength: 200);
                if (!string.IsNullOrWhiteSpace(name) && await create(name.Trim())) await LoadTaxonomyAsync();
                else if (!string.IsNullOrWhiteSpace(name)) await DisplayAlert("Грешка", "Неуспешно създаване.", "OK");
            };
            contentStack.Children.Add(addBtn);
        }

        if (items.Count == 0)
            contentStack.Children.Add(new Label { Text = "(няма данни)", TextColor = mutedColor, FontSize = 14 });
        else
            foreach (var item in items)
            {
                var id = getId(item);
                var name = getName(item);
                var row = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto }, new ColumnDefinition { Width = GridLength.Auto } },
                    Padding = new Thickness(0, 4),
                    ColumnSpacing = 8
                };
                row.Add(new Label { Text = "• " + name, TextColor = textColor, FontSize = 14, VerticalOptions = LayoutOptions.Center }, 0, 0);
                if (isAdmin)
                {
                    var editBtn = new Button { Text = "Редактирай", StyleClass = new[] { "GhostButton" }, Padding = 6, FontSize = 12, TextColor = textColor };
                    var delBtn = new Button { Text = "Изтрий", StyleClass = new[] { "GhostButton" }, Padding = 6, FontSize = 12, TextColor = Colors.Red };
                    editBtn.Clicked += async (_, _) =>
                    {
                        var newName = await DisplayPromptAsync($"Редактирай {sectionName}", "Име", initialValue: name, maxLength: 200);
                        if (!string.IsNullOrWhiteSpace(newName) && await update(id, newName.Trim())) await LoadTaxonomyAsync();
                    };
                    delBtn.Clicked += async (_, _) =>
                    {
                        if (await DisplayAlert("Изтрий", $"Изтриване на \"{name}\"?", "Да", "Не") && await delete(id)) await LoadTaxonomyAsync();
                    };
                    row.Add(editBtn, 1, 0);
                    row.Add(delBtn, 2, 0);
                }
                contentStack.Children.Add(row);
            }

        var card = new Border
        {
            StyleClass = new[] { "Card" },
            Padding = new Thickness(16),
            Content = new VerticalStackLayout { Spacing = 0, Children = { header, contentStack } }
        };
        TaxonomyStack.Children.Add(card);
    }
}
