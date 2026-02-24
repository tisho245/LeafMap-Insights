using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Карта с маркери за всички дървета от API. Клик по маркер отваря детайли за дървото.</summary>
public partial class MapPage : ContentPage
{
    private LeafMapApiService? _api;
    private readonly Dictionary<Pin, int> _pinToTreeId = new();

    public MapPage()
    {
        InitializeComponent();
        Loaded += (_, _) => _ = LoadTreesAsync();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

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
            var list = await Api.GetTreesAsync();
            Map.Pins.Clear();
            _pinToTreeId.Clear();

            if (list is not { Count: > 0 })
            {
                Loading.IsRunning = false;
                Loading.IsVisible = false;
                return;
            }

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
                Map.Pins.Add(pin);
                _pinToTreeId[pin] = tree.Id;
            }

            // Центриране на картата така че да се виждат всички маркери
            var minLat = list.Min(t => t.Latitude);
            var maxLat = list.Max(t => t.Latitude);
            var minLng = list.Min(t => t.Longitude);
            var maxLng = list.Max(t => t.Longitude);
            var center = new Location((minLat + maxLat) / 2, (minLng + maxLng) / 2);
            var latSpan = Math.Max(maxLat - minLat, 0.005);
            var lngSpan = Math.Max(maxLng - minLng, 0.005);
            var km = Math.Max(latSpan * 111, lngSpan * 111 * Math.Cos(center.Latitude * Math.PI / 180));
            Map.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(Math.Max(km, 1))));
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
