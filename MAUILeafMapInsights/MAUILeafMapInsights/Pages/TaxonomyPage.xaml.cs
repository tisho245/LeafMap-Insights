using Microsoft.Extensions.DependencyInjection;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Страница с таксономия – отделни карти (контейнери) за всяка секция, като в Node client.</summary>
public partial class TaxonomyPage : ContentPage
{
    private LeafMapApiService? _api;

    public TaxonomyPage()
    {
        InitializeComponent();
        _ = LoadTaxonomyAsync();
    }

    private LeafMapApiService? Api => _api ??= AppServices.Services?.GetService<LeafMapApiService>();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTaxonomyAsync();
    }

    static Color GetTextColor() => (Color)(Application.Current?.Resources["Text"] ?? Colors.Black);
    static Color GetMutedColor() => (Color)(Application.Current?.Resources["Muted"] ?? Colors.Gray);
    static Color GetBorderColor() => (Color)(Application.Current?.Resources["Border"] ?? Colors.LightGray);

    /// <summary>Зарежда данните и попълва по една карта (контейнер) за всяка секция.</summary>
    private async Task LoadTaxonomyAsync()
    {
        TaxonomyStack.Children.Clear();
        var textColor = GetTextColor();
        var mutedColor = GetMutedColor();

        if (Api == null)
        {
            TaxonomyStack.Children.Add(new Label
            {
                Text = "Услугите не са налични. Рестартирайте приложението.",
                TextColor = mutedColor,
                FontSize = 14
            });
            return;
        }

        try
        {
            var divs = await Api.GetDivisionsAsync();
            var classes = await Api.GetTaxonomyClassesAsync();
            var families = await Api.GetFamiliesAsync();
            var genera = await Api.GetGeneraAsync();
            var species = await Api.GetSpeciesAsync();

            void AddSectionCard(string title, IEnumerable<string>? items)
            {
                var list = items?.ToList() ?? new List<string>();
                var countLabel = new Label
                {
                    Text = list.Count.ToString(),
                    FontSize = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = textColor,
                    VerticalOptions = LayoutOptions.Center
                };
                var header = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Auto } },
                    Padding = new Thickness(0, 0, 0, 10)
                };
                header.Add(new Label
                {
                    Text = title,
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = textColor
                }, 0, 0);
                header.Add(countLabel, 1, 0);

                var itemsStack = new VerticalStackLayout { Spacing = 6 };
                if (list.Count == 0)
                    itemsStack.Children.Add(new Label { Text = "(няма данни)", TextColor = mutedColor, FontSize = 14 });
                else
                    foreach (var n in list)
                        itemsStack.Children.Add(new Label
                        {
                            Text = "• " + n,
                            TextColor = textColor,
                            FontSize = 14,
                            Padding = new Thickness(8, 6)
                        });

                var card = new Border
                {
                    StyleClass = new[] { "Card" },
                    Padding = new Thickness(16),
                    Content = new VerticalStackLayout { Spacing = 0, Children = { header, itemsStack } }
                };
                TaxonomyStack.Children.Add(card);
            }

            AddSectionCard("Отдели (Divisions)", divs?.Select(d => d.Name));
            AddSectionCard("Класове (Taxonomy Classes)", classes?.Select(c => c.Name));
            AddSectionCard("Семейства (Families)", families?.Select(f => f.Name));
            AddSectionCard("Родове (Genera)", genera?.Select(g => g.Name));
            AddSectionCard("Видове (Species)", species?.Select(s => s.Name));
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
}
