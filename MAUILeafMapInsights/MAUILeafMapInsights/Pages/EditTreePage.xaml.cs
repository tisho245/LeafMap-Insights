using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

[QueryProperty(nameof(TreeIdStr), "id")]
public partial class EditTreePage : ContentPage
{
    private LeafMapApiService? _api;
    private Entry _nameEntry = null!;
    private Entry _latEntry = null!;
    private Entry _lngEntry = null!;
    private Entry _descEntry = null!;
    private WebView _mapPickerWebView = null!;
    private Picker _divisionPicker = null!;
    private Picker _classPicker = null!;
    private Picker _familyPicker = null!;
    private Picker _genusPicker = null!;
    private Picker _speciesPicker = null!;
    private Label _errorLabel = null!;
    private Image _photoPreview = null!;
    private byte[]? _photoBytes;
    private List<DivisionDto> _divisions = new();
    private List<TaxonomyClassDto> _classes = new();
    private List<FamilyDto> _families = new();
    private List<GenusDto> _genera = new();
    private List<SpeciesDto> _species = new();
    private int _treeId;
    private string? _currentPhotoUrl;
    private int _loadedDivisionId = 1, _loadedClassId = 1, _loadedFamilyId = 1, _loadedGenusId = 1, _loadedSpeciesId = 1;

    public string? TreeIdStr { get; set; }

    public EditTreePage()
    {
        InitializeComponent();
        BuildForm();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (string.IsNullOrEmpty(TreeIdStr) || !int.TryParse(TreeIdStr.Trim(), out _treeId))
        {
            _errorLabel.Text = "Липсва id на дървото.";
            _errorLabel.IsVisible = true;
            return;
        }
        await LoadTaxonomyAsync();
        await LoadTreeAsync();
    }

    private void BuildForm()
    {
        var mutedColor = (Color)(Application.Current?.Resources["Muted"] ?? Colors.Gray);
        _nameEntry = new Entry { Placeholder = "Име *" };
        _latEntry = new Entry { Placeholder = "Ширина", Keyboard = Keyboard.Numeric, IsReadOnly = true };
        _lngEntry = new Entry { Placeholder = "Дължина", Keyboard = Keyboard.Numeric, IsReadOnly = true };
        _descEntry = new Entry { Placeholder = "Описание" };
        _mapPickerWebView = new WebView { MinimumHeightRequest = 220 };
        _mapPickerWebView.Navigating += OnMapPickerNavigating;
        _divisionPicker = new Picker { Title = "Отдел" };
        _classPicker = new Picker { Title = "Клас" };
        _familyPicker = new Picker { Title = "Семейство" };
        _genusPicker = new Picker { Title = "Род" };
        _speciesPicker = new Picker { Title = "Вид" };
        _errorLabel = new Label { TextColor = (Color)(Application.Current?.Resources["Error"] ?? Colors.Red), IsVisible = false };
        _photoPreview = new Image { HeightRequest = 180, WidthRequest = 180, BackgroundColor = Colors.LightGray, Aspect = Aspect.AspectFill };
        var takeBtn = new Button { Text = "Вземи снимка", StyleClass = new[] { "PrimaryButton" } };
        takeBtn.Clicked += OnTakePhotoClicked;
        var pickBtn = new Button { Text = "Избери от галерия", StyleClass = new[] { "SecondaryButton" } };
        pickBtn.Clicked += OnPickPhotoClicked;
        var photoGrid = new Grid { ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Star } }, ColumnSpacing = 10 };
        photoGrid.Add(takeBtn, 0, 0);
        photoGrid.Add(pickBtn, 1, 0);
        var photoCard = new Border { StyleClass = new[] { "Card" }, Padding = 16, Content = new VerticalStackLayout { Spacing = 12, Children = { new Label { Text = "Снимка", FontSize = 18 }, _photoPreview, photoGrid } } };
        FormStack.Children.Add(_nameEntry);
        FormStack.Children.Add(new Label { Text = "Кликни на картата, за да промениш местоположението на дървото.", FontSize = 14, TextColor = mutedColor, Margin = new Thickness(0, 8, 0, 4) });
        FormStack.Children.Add(new Border { StyleClass = new[] { "CardSoft" }, Padding = 12, Content = _mapPickerWebView });
        FormStack.Children.Add(new Label { Text = "Ширина (Latitude)", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = mutedColor, Margin = new Thickness(0, 4, 0, 0) });
        FormStack.Children.Add(_latEntry);
        FormStack.Children.Add(new Label { Text = "Дължина (Longitude)", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = mutedColor });
        FormStack.Children.Add(_lngEntry);
        FormStack.Children.Add(_descEntry);
        FormStack.Children.Add(photoCard);
        FormStack.Children.Add(_divisionPicker);
        FormStack.Children.Add(_classPicker);
        FormStack.Children.Add(_familyPicker);
        FormStack.Children.Add(_genusPicker);
        FormStack.Children.Add(_speciesPicker);
        FormStack.Children.Add(_errorLabel);
        var saveBtn = new Button { Text = "Запази", StyleClass = new[] { "PrimaryButton" } };
        saveBtn.Clicked += OnSaveClicked;
        FormStack.Children.Add(saveBtn);
    }

    private async Task LoadTaxonomyAsync()
    {
        _divisions = (await Api.GetDivisionsAsync()) ?? new List<DivisionDto>();
        _classes = (await Api.GetTaxonomyClassesAsync()) ?? new List<TaxonomyClassDto>();
        _families = (await Api.GetFamiliesAsync()) ?? new List<FamilyDto>();
        _genera = (await Api.GetGeneraAsync()) ?? new List<GenusDto>();
        _species = (await Api.GetSpeciesAsync()) ?? new List<SpeciesDto>();
        _divisionPicker.ItemsSource = _divisions.Select(d => d.Name).ToList();
        _classPicker.ItemsSource = _classes.Select(c => c.Name).ToList();
        _familyPicker.ItemsSource = _families.Select(f => f.Name).ToList();
        _genusPicker.ItemsSource = _genera.Select(g => g.Name).ToList();
        _speciesPicker.ItemsSource = _species.Select(s => s.Name).ToList();
    }

    private void OnMapPickerNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (e.Url?.StartsWith("leafmap://pick", StringComparison.OrdinalIgnoreCase) != true) return;
        e.Cancel = true;
        try
        {
            var uri = new Uri(e.Url);
            var query = uri.Query.TrimStart('?');
            foreach (var part in query.Split('&'))
            {
                var kv = part.Split('=', 2, StringSplitOptions.None);
                if (kv.Length != 2) continue;
                var key = Uri.UnescapeDataString(kv[0].Trim());
                var val = Uri.UnescapeDataString(kv[1].Trim());
                if (key.Equals("lat", StringComparison.OrdinalIgnoreCase)) _latEntry.Text = val;
                else if (key.Equals("lng", StringComparison.OrdinalIgnoreCase)) _lngEntry.Text = val;
            }
        }
        catch { /* ignore */ }
    }

    private static string GetMapPickerHtml(double lat, double lng)
    {
        var latStr = lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lngStr = lng.ToString(System.Globalization.CultureInfo.InvariantCulture);
        const string head = """
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1, user-scalable=no"/>
<link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" crossorigin=""/>
<script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js" crossorigin=""></script>
<style>
*{margin:0;padding:0;}
html,body,#map{height:100%;min-height:200px;}
.leaflet-marker-draggable{cursor:move;}
.tree-marker-icon{background:none!important;border:none!important;}
.tree-marker-emoji{font-size:28px;line-height:1;display:block;text-align:center;}
</style>
</head>
<body>
<div id="map"></div>
<script>
(function(){
var center = [
""";
        const string tail = """
];
var map = L.map('map').setView(center, 13);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { attribution: '&copy; OpenStreetMap' }).addTo(map);
function getTreeMarkerIcon() {
  return L.divIcon({
    className: 'tree-marker-icon',
    html: '<span class="tree-marker-emoji" aria-hidden="true">🌳</span>',
    iconSize: [32, 32],
    iconAnchor: [16, 32],
    popupAnchor: [0, -32]
  });
}
var marker = L.marker(center, { draggable: true, icon: getTreeMarkerIcon() }).addTo(map);
function send(latLng) {
  var lat = (typeof latLng.lat === 'function' ? latLng.lat() : latLng.lat);
  var lng = (typeof latLng.lng === 'function' ? latLng.lng() : latLng.lng);
  lat = Math.round(lat * 1e6) / 1e6;
  lng = Math.round(lng * 1e6) / 1e6;
  window.location = 'leafmap://pick?lat=' + lat + '&lng=' + lng;
}
map.on('click', function(e) { marker.setLatLng(e.latlng); send(e.latlng); });
marker.on('dragend', function() { send(this.getLatLng()); });
})();
</script>
</body>
</html>
""";
        return head + latStr + "," + lngStr + tail;
    }

    private async Task LoadTreeAsync()
    {
        var tree = await Api.GetTreeAsync(_treeId);
        if (tree == null) { _errorLabel.Text = "Дървото не е намерено."; _errorLabel.IsVisible = true; return; }
        _nameEntry.Text = tree.Name;
        _latEntry.Text = tree.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _lngEntry.Text = tree.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        _descEntry.Text = tree.Description ?? "";
        _currentPhotoUrl = tree.PhotoURL;
        if (!string.IsNullOrEmpty(tree.PhotoURL))
        {
            if (tree.PhotoURL.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var i = tree.PhotoURL.IndexOf(",", StringComparison.Ordinal);
                if (i >= 0) try { var b64 = tree.PhotoURL.Substring(i + 1); _photoPreview.Source = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(b64))); } catch { }
            }
            else _photoPreview.Source = tree.PhotoURL;
        }
        _loadedDivisionId = tree.DivisionId;
        _loadedClassId = tree.TaxonomyClassId;
        _loadedFamilyId = tree.FamilyId;
        _loadedGenusId = tree.GenusId;
        _loadedSpeciesId = tree.SpeciesId;
        var divIdx = _divisions.FindIndex(d => d.Id == tree.DivisionId); if (divIdx >= 0) _divisionPicker.SelectedIndex = divIdx;
        var clIdx = _classes.FindIndex(c => c.Id == tree.TaxonomyClassId); if (clIdx >= 0) _classPicker.SelectedIndex = clIdx;
        var famIdx = _families.FindIndex(f => f.Id == tree.FamilyId); if (famIdx >= 0) _familyPicker.SelectedIndex = famIdx;
        var genIdx = _genera.FindIndex(g => g.Id == tree.GenusId); if (genIdx >= 0) _genusPicker.SelectedIndex = genIdx;
        var specIdx = _species.FindIndex(s => s.Id == tree.SpeciesId); if (specIdx >= 0) _speciesPicker.SelectedIndex = specIdx;
        _mapPickerWebView.Source = new HtmlWebViewSource { Html = GetMapPickerHtml(tree.Latitude, tree.Longitude) };
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try { var p = await MediaPicker.Default.CapturePhotoAsync(); if (p != null) await SetPhotoFromFile(p.FullPath); } catch (Exception ex) { _errorLabel.Text = ex.Message; _errorLabel.IsVisible = true; }
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
    {
        try { var p = await MediaPicker.Default.PickPhotoAsync(); if (p != null) await SetPhotoFromFile(p.FullPath); } catch (Exception ex) { _errorLabel.Text = ex.Message; _errorLabel.IsVisible = true; }
    }

    private async Task SetPhotoFromFile(string path)
    {
        _photoPreview.Source = path;
        _errorLabel.IsVisible = false;
        try { _photoBytes = await File.ReadAllBytesAsync(path); } catch { _photoBytes = null; }
        await Task.CompletedTask;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        _errorLabel.IsVisible = false;
        if (string.IsNullOrWhiteSpace(_nameEntry.Text)) { _errorLabel.Text = "Въведете име."; _errorLabel.IsVisible = true; return; }
        var photoUrl = _currentPhotoUrl;
        if (_photoBytes != null && _photoBytes.Length > 0)
        {
            var maxBytes = 280_000;
            if (_photoBytes.Length > maxBytes)
            {
                _errorLabel.Text = "Снимката е твърде голяма. Направете по-малка или изберете друга.";
                _errorLabel.IsVisible = true;
                return;
            }
            photoUrl = "data:image/jpeg;base64," + Convert.ToBase64String(_photoBytes);
        }
        int divId = _divisionPicker.SelectedIndex >= 0 && _divisionPicker.SelectedIndex < _divisions.Count ? _divisions[_divisionPicker.SelectedIndex].Id : _loadedDivisionId;
        int classId = _classPicker.SelectedIndex >= 0 && _classPicker.SelectedIndex < _classes.Count ? _classes[_classPicker.SelectedIndex].Id : _loadedClassId;
        int famId = _familyPicker.SelectedIndex >= 0 && _familyPicker.SelectedIndex < _families.Count ? _families[_familyPicker.SelectedIndex].Id : _loadedFamilyId;
        int genId = _genusPicker.SelectedIndex >= 0 && _genusPicker.SelectedIndex < _genera.Count ? _genera[_genusPicker.SelectedIndex].Id : _loadedGenusId;
        int specId = _speciesPicker.SelectedIndex >= 0 && _speciesPicker.SelectedIndex < _species.Count ? _species[_speciesPicker.SelectedIndex].Id : _loadedSpeciesId;
        var tree = new TreeDto
        {
            Id = _treeId,
            Name = _nameEntry.Text!.Trim(),
            Description = string.IsNullOrWhiteSpace(_descEntry.Text) ? null : _descEntry.Text.Trim(),
            PhotoURL = photoUrl,
            Latitude = double.TryParse(_latEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 42.655780716559626,
            Longitude = double.TryParse(_lngEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lng) ? lng : 24.747289594150434,
            DivisionId = divId,
            TaxonomyClassId = classId,
            FamilyId = famId,
            GenusId = genId,
            SpeciesId = specId
        };
        var (success, error) = await Api.UpdateTreeWithErrorAsync(_treeId, tree);
        if (success) await Shell.Current.GoToAsync("..");
        else
        {
            _errorLabel.Text = string.IsNullOrWhiteSpace(error) ? "Неуспешно запазване." : "Грешка: " + (error.Length > 200 ? error.Substring(0, 200) + "…" : error);
            _errorLabel.IsVisible = true;
        }
    }
}
