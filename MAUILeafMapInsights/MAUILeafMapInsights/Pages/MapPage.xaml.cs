using System.Text.Json;
using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Карта с дървета чрез OpenStreetMap (Leaflet) в WebView – без API ключ.</summary>
public partial class MapPage : ContentPage
{
    private LeafMapApiService? _api;
    private List<TreeDto>? _pendingTrees;
    private bool _webViewReady;

    public MapPage()
    {
        InitializeComponent();
        MapWebView.Source = new HtmlWebViewSource { Html = GetMapHtml() };
        MapWebView.Navigating += OnMapWebViewNavigating;
        MapWebView.Navigated += OnMapWebViewNavigated;
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadTreesAsync();
    }

    private void OnMapWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (e.Url?.StartsWith("leafmap://", StringComparison.OrdinalIgnoreCase) == true)
        {
            e.Cancel = true;
            var uri = new Uri(e.Url);
            if (uri.Host.Equals("tree", StringComparison.OrdinalIgnoreCase) && uri.Segments.Length > 1 && int.TryParse(uri.Segments[^1].TrimEnd('/'), out var id))
                _ = Shell.Current.GoToAsync($"TreeDetail?id={id}");
        }
    }

    private async void OnMapWebViewNavigated(object? sender, WebNavigatedEventArgs e)
    {
        _webViewReady = true;
        if (_pendingTrees != null)
        {
            await InjectTreesAsync(_pendingTrees);
            _pendingTrees = null;
        }
    }

    private async Task LoadTreesAsync()
    {
        Loading.IsRunning = true;
        Loading.IsVisible = true;
        try
        {
            var list = await Api.GetTreesAsync();
            var trees = list ?? new List<TreeDto>();
            _pendingTrees = trees;
            if (_webViewReady)
            {
                await InjectTreesAsync(trees);
                _pendingTrees = null;
            }
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private async Task InjectTreesAsync(List<TreeDto> trees)
    {
        var data = trees.Select(t => new { id = t.Id, name = t.Name ?? "", lat = t.Latitude, lng = t.Longitude }).ToList();
        var json = JsonSerializer.Serialize(data);
        var script = "setTrees(" + json + ");";
        try
        {
            await MapWebView.EvaluateJavaScriptAsync(script);
        }
        catch
        {
            // WebView още не е готов или платформата не поддържа
        }
    }

    private void OnRefreshClicked(object? sender, EventArgs e) => _ = LoadTreesAsync();

    private static string GetMapHtml()
    {
        return """
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no"/>
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" integrity="sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=" crossorigin=""/>
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js" integrity="sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=" crossorigin=""></script>
<style>
*{margin:0;padding:0;}
html,body,#map{height:100%;width:100%;}
.leaflet-popup-content-wrapper{border-radius:12px;}
.tree-marker-icon{background:none!important;border:none!important;}
.tree-marker-emoji{font-size:28px;line-height:1;display:block;text-align:center;}
</style>
</head>
<body>
<div id="map"></div>
<script>
(function(){
var DEFAULT_MAP_CENTER = [42.655780716559626, 24.747289594150434];
var map = L.map('map', { zoomControl: true }).setView(DEFAULT_MAP_CENTER, 12);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
  attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
}).addTo(map);
function getTreeMarkerIcon() {
  return L.divIcon({
    className: 'tree-marker-icon',
    html: '<span class="tree-marker-emoji" aria-hidden="true">🌳</span>',
    iconSize: [32, 32],
    iconAnchor: [16, 32],
    popupAnchor: [0, -32]
  });
}
var treeIcon = getTreeMarkerIcon();
var markers = [];
window.setTrees = function(data) {
  markers.forEach(function(m){ map.removeLayer(m); });
  markers = [];
  map.setView(DEFAULT_MAP_CENTER, 12);
  if (!data || data.length === 0) return;
  data.forEach(function(t){
    var m = L.marker([t.lat, t.lng], { icon: treeIcon }).addTo(map).bindPopup((t.name && t.name.length) ? t.name : ('ID ' + t.id));
    m.on('click', function(){ window.location = 'leafmap://tree/' + t.id; });
    markers.push(m);
  });
};
})();
</script>
</body>
</html>
""";
    }
}
