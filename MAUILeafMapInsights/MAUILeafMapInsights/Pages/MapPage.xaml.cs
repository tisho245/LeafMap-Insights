using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Карта с маркери за всички дървета от API. На Android без валиден API ключ показваме само съобщение, за да не крашва приложението.</summary>
public partial class MapPage : ContentPage
{
    private LeafMapApiService? _api;
    private readonly Dictionary<Pin, int> _pinToTreeId = new();
    private Microsoft.Maui.Controls.Maps.Map? _map;

    public MapPage()
    {
        InitializeComponent();

        // На Windows MAUI Map няма handler – показваме placeholder. На Android без API ключ също.
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            MapContainer.Content = new Label
            {
                Text = "За карта на Android задайте Google Maps API ключ в Platforms/Android/AndroidManifest.xml (com.google.android.geo.API_KEY). Дърветата можете да преглеждате от „Дървета“.",
                Margin = new Thickness(16),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
        }
        else if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            MapContainer.Content = new Label
            {
                Text = "Картата не е налична на Windows. Дърветата можете да преглеждате от „Дървета“.",
                Margin = new Thickness(16),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
        }
        else
        {
            _map = new Microsoft.Maui.Controls.Maps.Map();
            MapContainer.Content = _map;
        }

        Loaded += (_, _) => _ = LoadTreesAsync();
    }

    private LeafMapApiService? Api => _api ??= AppServices.Get<LeafMapApiService>();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadTreesAsync();
    }

    private async Task LoadTreesAsync()
    {
        Loading.IsRunning = true;
        Loading.IsVisible = true;
        try
        {
            var api = Api;
            if (api == null) return;
            var list = await api.GetTreesAsync();
            if (_map == null)
            {
                if (list is { Count: > 0 } && MapContainer.Content is Label lbl)
                    lbl.Text = (DeviceInfo.Platform == DevicePlatform.WinUI ? "Картата не е налична на Windows.\n\n" : "За карта на Android задайте Google Maps API ключ в Platforms/Android/AndroidManifest.xml (com.google.android.geo.API_KEY).\n\n") + "Дървета в каталога: " + list.Count + ".";
                return;
            }

            _map.Pins.Clear();
            _pinToTreeId.Clear();

            if (list is not { Count: > 0 })
                return;

            foreach (var tree in list)
            {
                var pin = new Pin
                {
                    Label = tree.Name,
                    Address = tree.Species?.Name ?? $"Дърво #{tree.Id}",
                    Type = PinType.Place,
                    Location = new Location(tree.Latitude, tree.Longitude)
                };
                pin.MarkerClicked += OnPinMarkerClicked;
                _map.Pins.Add(pin);
                _pinToTreeId[pin] = tree.Id;
            }

            var minLat = list.Min(t => t.Latitude);
            var maxLat = list.Max(t => t.Latitude);
            var minLng = list.Min(t => t.Longitude);
            var maxLng = list.Max(t => t.Longitude);
            var center = new Location((minLat + maxLat) / 2, (minLng + maxLng) / 2);
            var latSpan = Math.Max(maxLat - minLat, 0.005);
            var lngSpan = Math.Max(maxLng - minLng, 0.005);
            var km = Math.Max(latSpan * 111, lngSpan * 111 * Math.Cos(center.Latitude * Math.PI / 180));
            _map.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(Math.Max(km, 1))));
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private async void OnPinMarkerClicked(object? sender, PinClickedEventArgs e)
    {
        e.HideInfoWindow = true;
        if (sender is Pin pin && _pinToTreeId.TryGetValue(pin, out var id))
            await Shell.Current.GoToAsync($"TreeDetail?id={id}");
    }

    private void OnRefreshClicked(object? sender, EventArgs e) => _ = LoadTreesAsync();
}
