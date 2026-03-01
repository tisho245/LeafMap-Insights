using System.Text.Json;
using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Карта с дървета чрез OpenStreetMap (Leaflet) в WebView – без API ключ.</summary>
public partial class MapPage : ContentPage
{
    private LeafMapApiService? _api;

    public MapPage()
    {
        InitializeComponent();
        MapWebView.Navigating += OnMapWebViewNavigating;
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadTreesAndShowMapAsync();
    }

    private async void OnMapWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (e.Url?.StartsWith("leafmap://", StringComparison.OrdinalIgnoreCase) != true) return;
        e.Cancel = true;
        var uri = new Uri(e.Url);
        if (uri.Host.Equals("tree", StringComparison.OrdinalIgnoreCase) && uri.Segments.Length > 1 && int.TryParse(uri.Segments[^1].TrimEnd('/'), out var id))
        {
            await Shell.Current.GoToAsync($"TreeDetail?id={id}");
            return;
        }
        if (uri.Host.Equals("bounds", StringComparison.OrdinalIgnoreCase) && uri.Segments.Length >= 5
            && double.TryParse(uri.Segments[1].TrimEnd('/'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var minLat)
            && double.TryParse(uri.Segments[2].TrimEnd('/'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var minLng)
            && double.TryParse(uri.Segments[3].TrimEnd('/'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var maxLat)
            && double.TryParse(uri.Segments[4].TrimEnd('/'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var maxLng))
            await UpdateTreesInMapAsync(minLat, minLng, maxLat, maxLng);
    }

    private async Task UpdateTreesInMapAsync(double minLat, double minLng, double maxLat, double maxLng)
    {
        try
        {
            var list = await Api.GetTreesWithinBoundsAsync(minLat, minLng, maxLat, maxLng);
            var trees = list ?? new List<TreeDto>();
            var data = trees.Select(t => new { id = t.Id, name = t.Name ?? "", lat = t.Latitude, lng = t.Longitude }).ToList();
            var json = JsonSerializer.Serialize(data);
            await MapWebView.EvaluateJavaScriptAsync($"if(typeof setTrees==='function')setTrees({json});");
        }
        catch { /* игнорираме при грешка от мрежа */ }
    }

    /// <summary>Зарежда дърветата и показва картата с данните вградени в HTML – така маркерите винаги се виждат.</summary>
    private async Task LoadTreesAndShowMapAsync()
    {
        Loading.IsRunning = true;
        Loading.IsVisible = true;
        try
        {
            var list = await Api.GetTreesAsync();
            var trees = list ?? new List<TreeDto>();
            var data = trees.Select(t => new { id = t.Id, name = t.Name ?? "", lat = t.Latitude, lng = t.Longitude }).ToList();
            var json = JsonSerializer.Serialize(data);
            var html = GetMapHtml(json);
            MapWebView.Source = new HtmlWebViewSource { Html = html };
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
        }
    }

    private void OnRefreshClicked(object? sender, EventArgs e) => _ = LoadTreesAndShowMapAsync();

    private static string GetMapHtml(string treesJson = "[]")
    {
        const string head = """
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
var initialTrees = 
""";
        const string tail = """
;
window.setTrees = function(data) {
  if (!data) data = initialTrees;
  markers.forEach(function(m){ map.removeLayer(m); });
  markers = [];
  if (!data || data.length === 0) return;
  data.forEach(function(t){
    var m = L.marker([t.lat, t.lng], { icon: treeIcon }).addTo(map).bindPopup((t.name && t.name.length) ? t.name : ('ID ' + t.id));
    m.on('click', function(){ window.location = 'leafmap://tree/' + t.id; });
    markers.push(m);
  });
};
if (initialTrees && initialTrees.length) window.setTrees(initialTrees);
var boundsTimeout;
map.on('moveend', function() {
  clearTimeout(boundsTimeout);
  boundsTimeout = setTimeout(function() {
    var b = map.getBounds();
    window.location = 'leafmap://bounds/' + b.getSouth() + '/' + b.getWest() + '/' + b.getNorth() + '/' + b.getEast();
  }, 400);
});
})();
</script>
</body>
</html>
""";
        return head + treesJson + tail;
    }
}
