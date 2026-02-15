using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Страница с таксономия – показва отдели, класове, семейства, родове и видове от API в секции.</summary>
public partial class TaxonomyPage : ContentPage
{
    private LeafMapApiService? _api;

    public TaxonomyPage()
    {
        InitializeComponent();
        LoadTaxonomy();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadTaxonomy();
    }

    /// <summary>Зарежда всички референтни списъци от API и ги показва в TaxonomyStack с AddSection.</summary>
    private async void LoadTaxonomy()
    {
        TaxonomyStack.Children.Clear();
        var divs = await Api.GetDivisionsAsync();
        var classes = await Api.GetTaxonomyClassesAsync();
        var families = await Api.GetFamiliesAsync();
        var genera = await Api.GetGeneraAsync();
        var species = await Api.GetSpeciesAsync();

        void AddSection(string title, IEnumerable<string>? items)
        {
            TaxonomyStack.Children.Add(new Label { Text = title, FontSize = 18, FontAttributes = FontAttributes.Bold });
            if (items != null)
                foreach (var n in items)
                    TaxonomyStack.Children.Add(new Label { Text = $"  • {n}", Margin = new Thickness(0, 2, 0, 0) });
            else
                TaxonomyStack.Children.Add(new Label { Text = "  (няма данни)", TextColor = Colors.Gray });
        }

        AddSection("Отдели", divs?.Select(d => d.Name));
        AddSection("Класове", classes?.Select(c => c.Name));
        AddSection("Семейства", families?.Select(f => f.Name));
        AddSection("Родове", genera?.Select(g => g.Name));
        AddSection("Видове", species?.Select(s => s.Name));
    }
}
