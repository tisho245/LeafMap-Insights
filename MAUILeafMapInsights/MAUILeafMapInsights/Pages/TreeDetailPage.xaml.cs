using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Детайли за едно дърво. Id се чете от query string (TreeDetail?id=...); данните се зареждат чрез GetTreeAsync.</summary>
public partial class TreeDetailPage : ContentPage
{
    private LeafMapApiService? _api;

    public TreeDetailPage()
    {
        InitializeComponent();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

    /// <summary>От query string извличаме id, зареждаме дървото от API и показваме име, описание, координати, вид, семейство.</summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var query = Shell.Current?.CurrentState?.Location?.ToString() ?? "";
        var id = query.Contains("id=") ? query.Split("id=").LastOrDefault()?.Split('&').FirstOrDefault()?.Trim() : null;
        if (string.IsNullOrEmpty(id) || !int.TryParse(id, out var treeId))
            return;
        var tree = await Api.GetTreeAsync(treeId);
        if (tree == null) return;
        ContentStack.Children.Clear();
        ContentStack.Children.Add(new Label { Text = tree.Name, FontSize = 22, FontAttributes = FontAttributes.Bold });
        ContentStack.Children.Add(new Label { Text = tree.Description ?? "-", TextColor = Colors.Gray });
        ContentStack.Children.Add(new Label { Text = $"Координати: {tree.Latitude:F4}, {tree.Longitude:F4}" });
        ContentStack.Children.Add(new Label { Text = $"Вид: {tree.Species?.Name ?? "-"}" });
        ContentStack.Children.Add(new Label { Text = $"Семейство: {tree.Family?.Name ?? "-"}" });
    }
}
