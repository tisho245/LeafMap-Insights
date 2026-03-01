using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Детайли за едно дърво. Id се подава чрез query string (TreeDetail?id=...). Логнат потребител/админ може да добави снимка.</summary>
[QueryProperty(nameof(TreeIdStr), "id")]
public partial class TreeDetailPage : ContentPage
{
    private LeafMapApiService? _api;
    private AuthService? _auth;
    private TreeDto? _currentTree;
    private int _currentTreeId;

    /// <summary>Стойността на id от query string (напр. "5").</summary>
    public string? TreeIdStr { get; set; }

    public TreeDetailPage()
    {
        InitializeComponent();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();
    private AuthService Auth => _auth ??= AppServices.GetRequired<AuthService>();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        ContentStack.Children.Clear();
        ContentStack.Children.Add(new Label { Text = "Зареждане...", TextColor = (Color)(Application.Current?.Resources["Muted"] ?? Colors.Gray) });

        if (string.IsNullOrEmpty(TreeIdStr) || !int.TryParse(TreeIdStr.Trim(), out _currentTreeId))
        {
            ContentStack.Children.Clear();
            ContentStack.Children.Add(new Label { Text = "Липсва идентификатор на дървото.", TextColor = (Color)(Application.Current?.Resources["Error"] ?? Colors.Red) });
            return;
        }

        try
        {
            var tree = await Api.GetTreeAsync(_currentTreeId);
            _currentTree = tree;
            if (tree == null)
            {
                ContentStack.Children.Clear();
                ContentStack.Children.Add(new Label { Text = "Дървото не е намерено.", TextColor = (Color)(Application.Current?.Resources["Muted"] ?? Colors.Gray) });
                EditTreeButton.IsVisible = false;
                return;
            }

            EditTreeButton.IsVisible = await Auth.IsAdminAsync();
            FillContent(tree);
        }
        catch (Exception ex)
        {
            ContentStack.Children.Clear();
            ContentStack.Children.Add(new Label { Text = "Грешка: " + ex.Message, TextColor = (Color)(Application.Current?.Resources["Error"] ?? Colors.Red), LineBreakMode = LineBreakMode.WordWrap });
        }
    }

    private void FillContent(TreeDto tree)
    {
        ContentStack.Children.Clear();
        var textColor = (Color)(Application.Current?.Resources["Text"] ?? Colors.Black);
        var mutedColor = (Color)(Application.Current?.Resources["Muted"] ?? Colors.Gray);

        ContentStack.Children.Add(new Label { Text = tree.Name, FontSize = 22, FontAttributes = FontAttributes.Bold, TextColor = textColor });
        ContentStack.Children.Add(new Label { Text = tree.Description ?? "—", TextColor = mutedColor, LineBreakMode = LineBreakMode.WordWrap });
        ContentStack.Children.Add(new Label { Text = $"Координати: {tree.Latitude:F4}, {tree.Longitude:F4}", TextColor = textColor });
        ContentStack.Children.Add(new Label { Text = $"Вид: {tree.Species?.Name ?? "—"}", TextColor = textColor });
        ContentStack.Children.Add(new Label { Text = $"Семейство: {tree.Family?.Name ?? "—"}", TextColor = textColor });
        ContentStack.Children.Add(new Label { Text = $"Род: {tree.Genus?.Name ?? "—"}", TextColor = textColor });

        if (!string.IsNullOrEmpty(tree.PhotoURL))
        {
            var photo = new Image
            {
                HeightRequest = 220,
                WidthRequest = 220,
                Aspect = Aspect.AspectFill,
                Margin = new Thickness(0, 12, 0, 0)
            };
            photo.Source = PhotoUrlToImageSource(tree.PhotoURL);
            ContentStack.Children.Add(photo);
        }
    }

    /// <summary>Превръща PhotoURL (обикновен URL или data:image/...;base64,...) в ImageSource.</summary>
    private static ImageSource? PhotoUrlToImageSource(string? url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            var base64 = url.IndexOf(",", StringComparison.Ordinal) is int i && i >= 0
                ? url.Substring(i + 1)
                : url;
            try
            {
                var bytes = Convert.FromBase64String(base64);
                return ImageSource.FromStream(() => new MemoryStream(bytes));
            }
            catch { return null; }
        }
        return url;
    }

    private async void OnEditTreeClicked(object? sender, EventArgs e)
    {
        if (_currentTreeId <= 0) return;
        await Shell.Current.GoToAsync($"EditTree?id={_currentTreeId}");
    }

    private async void OnAddPhotoClicked(object? sender, EventArgs e)
    {
        if (_currentTree == null) return;
        if (!await Auth.IsLoggedInAsync())
        {
            await DisplayAlert("Вход изискван", "Влезте в профила си, за да добавите снимка към дървото.", "OK");
            return;
        }
        try
        {
            var action = await DisplayActionSheet("Добави снимка", "Отказ", null, "Вземи снимка", "Избери от галерия");
            if (string.IsNullOrEmpty(action) || action == "Отказ") return;

            FileResult? result = action == "Вземи снимка"
                ? await MediaPicker.Default.CapturePhotoAsync()
                : await MediaPicker.Default.PickPhotoAsync();
            if (result == null) return;

            var path = result.FullPath;
            if (string.IsNullOrEmpty(path)) return;
            var bytes = await File.ReadAllBytesAsync(path);
            if (bytes.Length == 0) return;

            var dataUrl = "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
            _currentTree.PhotoURL = dataUrl;
            var ok = await Api.UpdateTreeAsync(_currentTreeId, _currentTree);
            if (ok)
            {
                await DisplayAlert("Готово", "Снимката е добавена.", "OK");
                var refreshed = await Api.GetTreeAsync(_currentTreeId);
                _currentTree = refreshed;
                if (refreshed != null)
                    FillContent(refreshed);
            }
            else
                await DisplayAlert("Грешка", "Неуспешно обновяване. Опитайте отново.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", ex.Message, "OK");
        }
    }
}
