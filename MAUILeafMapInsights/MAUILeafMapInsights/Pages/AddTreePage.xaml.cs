using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Форма за добавяне на ново дърво. Карта за избор на място (цъкане), референтни списъци за таксономия, снимка и POST api/trees.</summary>
public partial class AddTreePage : ContentPage
{
    private LeafMapApiService? _api;
    private AuthService? _auth;
    private Entry _nameEntry = null!;
    private Entry _latEntry = null!;
    private Entry _lngEntry = null!;
    private WebView _mapPickerWebView = null!;
    private Picker _divisionPicker = null!;
    private Picker _classPicker = null!;
    private Picker _familyPicker = null!;
    private Picker _genusPicker = null!;
    private Picker _speciesPicker = null!;
    private Label _errorLabel = null!;
    private Image _photoPreview = null!;
    private string? _photoPath;
    private byte[]? _photoBytes;
    private List<DivisionDto> _divisions = new();
    private List<TaxonomyClassDto> _classes = new();
    private List<FamilyDto> _families = new();
    private List<GenusDto> _genera = new();
    private List<SpeciesDto> _species = new();

    public AddTreePage()
    {
        InitializeComponent();
        BuildForm();
        _ = LoadTaxonomyAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        EnsureServices();
        if (!await _auth!.IsLoggedInAsync())
        {
            await Shell.Current.GoToAsync("//Login");
            return;
        }
    }

    private void EnsureServices()
    {
        _api ??= AppServices.GetRequired<LeafMapApiService>();
        _auth ??= AppServices.GetRequired<AuthService>();
    }

    private static Color GetResourceColor(string key)
    {
        if (Application.Current?.Resources[key] is Color c) return c;
        return key == "Muted" ? Colors.Gray : key == "Error" ? Colors.Red : Colors.Black;
    }

    /// <summary>Създава полета, карта за избор на място, снимка и Picker-и – подредба и стил като node client.</summary>
    private void BuildForm()
    {
        var textColor = GetResourceColor("Text");
        var mutedColor = GetResourceColor("Muted");

        _nameEntry = new Entry { Placeholder = "Име на дървото *" };
        _latEntry = new Entry { Placeholder = "42.65578", Keyboard = Keyboard.Numeric, Text = "42.655780716559626", IsReadOnly = true };
        _lngEntry = new Entry { Placeholder = "24.74729", Keyboard = Keyboard.Numeric, Text = "24.747289594150434", IsReadOnly = true };
        _divisionPicker = new Picker { Title = "Отдел" };
        _classPicker = new Picker { Title = "Клас" };
        _familyPicker = new Picker { Title = "Семейство" };
        _genusPicker = new Picker { Title = "Род" };
        _speciesPicker = new Picker { Title = "Вид" };
        _errorLabel = new Label { TextColor = GetResourceColor("Error"), IsVisible = false };

        _mapPickerWebView = new WebView { MinimumHeightRequest = 220, Source = new HtmlWebViewSource { Html = GetMapPickerHtml() } };
        _mapPickerWebView.Navigating += OnMapPickerNavigating;

        _photoPreview = new Image
        {
            HeightRequest = 180,
            WidthRequest = 180,
            BackgroundColor = (Color)(Application.Current?.Resources["Gray200"] ?? Colors.LightGray),
            Aspect = Aspect.AspectFill
        };
        var takePhotoBtn = new Button { Text = "Вземи снимка", StyleClass = new[] { "PrimaryButton" } };
        takePhotoBtn.Clicked += OnTakePhotoClicked;
        var pickPhotoBtn = new Button { Text = "Избери от галерия", StyleClass = new[] { "SecondaryButton" } };
        pickPhotoBtn.Clicked += OnPickPhotoClicked;
        var photoButtonsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = GridLength.Star }, new ColumnDefinition { Width = GridLength.Star } },
            ColumnSpacing = 10
        };
        photoButtonsGrid.Add(takePhotoBtn, 0, 0);
        photoButtonsGrid.Add(pickPhotoBtn, 1, 0);

        var photoCard = new Border
        {
            StyleClass = new[] { "CardSoft" },
            Padding = new Thickness(16),
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    new Label { Text = "Снимка (по избор)", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = textColor },
                    new Label { Text = "Вземи снимка с камерата или избери от галерията.", FontSize = 14, TextColor = mutedColor },
                    _photoPreview,
                    photoButtonsGrid
                }
            }
        };

        FormStack.Children.Add(new Label { Text = "Име", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = mutedColor });
        FormStack.Children.Add(_nameEntry);
        FormStack.Children.Add(new Label { Text = "Кликни на картата по-долу, за да избереш местоположение — ширина и дължина се попълват автоматично.", FontSize = 14, TextColor = mutedColor, Margin = new Thickness(0, 8, 0, 4) });
        FormStack.Children.Add(new Border
        {
            StyleClass = new[] { "CardSoft" },
            Padding = new Thickness(12),
            Content = _mapPickerWebView
        });
        FormStack.Children.Add(new Label { Text = "Ширина (Latitude) — от картата", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = mutedColor, Margin = new Thickness(0, 4, 0, 0) });
        FormStack.Children.Add(_latEntry);
        FormStack.Children.Add(new Label { Text = "Дължина (Longitude) — от картата", FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = mutedColor });
        FormStack.Children.Add(_lngEntry);
        FormStack.Children.Add(photoCard);
        FormStack.Children.Add(new Label { Text = "Таксономия", FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = mutedColor, Margin = new Thickness(0, 8, 0, 0) });
        FormStack.Children.Add(_divisionPicker);
        FormStack.Children.Add(_classPicker);
        FormStack.Children.Add(_familyPicker);
        FormStack.Children.Add(_genusPicker);
        FormStack.Children.Add(_speciesPicker);
        FormStack.Children.Add(_errorLabel);
        var submitBtn = new Button { Text = "Запази", StyleClass = new[] { "PrimaryButton" }, Margin = new Thickness(0, 12, 0, 0) };
        submitBtn.Clicked += OnSubmitClicked;
        FormStack.Children.Add(submitBtn);
    }

    private void OnMapPickerNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (e.Url?.StartsWith("leafmap://pick", StringComparison.OrdinalIgnoreCase) != true) return;
        e.Cancel = true;
        try
        {
            var uri = new Uri(e.Url);
            var query = uri.Query.TrimStart('?');
            string? lat = null, lng = null;
            foreach (var part in query.Split('&'))
            {
                var kv = part.Split('=', 2, StringSplitOptions.None);
                if (kv.Length != 2) continue;
                var key = Uri.UnescapeDataString(kv[0].Trim());
                var val = Uri.UnescapeDataString(kv[1].Trim());
                if (key.Equals("lat", StringComparison.OrdinalIgnoreCase)) lat = val;
                else if (key.Equals("lng", StringComparison.OrdinalIgnoreCase)) lng = val;
            }
            if (!string.IsNullOrEmpty(lat)) _latEntry.Text = lat;
            if (!string.IsNullOrEmpty(lng)) _lngEntry.Text = lng;
        }
        catch { /* ignore */ }
    }

    private static string GetMapPickerHtml()
    {
        return """
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
var center = [42.655780716559626, 24.747289594150434];
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
    }

    private async void OnTakePhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                _errorLabel.Text = "Камерата не се поддържа на това устройство.";
                _errorLabel.IsVisible = true;
                return;
            }
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
                await SetPhotoFromFile(photo.FullPath);
        }
        catch (Exception ex)
        {
            _errorLabel.Text = "Грешка при заснемане: " + ex.Message;
            _errorLabel.IsVisible = true;
        }
    }

    private async void OnPickPhotoClicked(object? sender, EventArgs e)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
                await SetPhotoFromFile(photo.FullPath);
        }
        catch (Exception ex)
        {
            _errorLabel.Text = "Грешка при избор на снимка: " + ex.Message;
            _errorLabel.IsVisible = true;
        }
    }

    private async Task SetPhotoFromFile(string path)
    {
        _photoPath = path;
        _photoPreview.Source = path;
        _errorLabel.IsVisible = false;
        try
        {
            _photoBytes = await File.ReadAllBytesAsync(path);
        }
        catch
        {
            _photoBytes = null;
        }
    }

    /// <summary>Зарежда референтни списъци от API и попълва Picker-ите.</summary>
    private async Task LoadTaxonomyAsync()
    {
        EnsureServices();
        _divisions = (await _api!.GetDivisionsAsync()) ?? new List<DivisionDto>();
        _classes = (await _api.GetTaxonomyClassesAsync()) ?? new List<TaxonomyClassDto>();
        _families = (await _api.GetFamiliesAsync()) ?? new List<FamilyDto>();
        _genera = (await _api.GetGeneraAsync()) ?? new List<GenusDto>();
        _species = (await _api.GetSpeciesAsync()) ?? new List<SpeciesDto>();

        _divisionPicker.ItemsSource = _divisions.Select(d => d.Name).ToList();
        _classPicker.ItemsSource = _classes.Select(c => c.Name).ToList();
        _familyPicker.ItemsSource = _families.Select(f => f.Name).ToList();
        _genusPicker.ItemsSource = _genera.Select(g => g.Name).ToList();
        _speciesPicker.ItemsSource = _species.Select(s => s.Name).ToList();
        if (_divisions.Count > 0) _divisionPicker.SelectedIndex = 0;
        if (_classes.Count > 0) _classPicker.SelectedIndex = 0;
        if (_families.Count > 0) _familyPicker.SelectedIndex = 0;
        if (_genera.Count > 0) _genusPicker.SelectedIndex = 0;
        if (_species.Count > 0) _speciesPicker.SelectedIndex = 0;
    }

    /// <summary>Валидира формата, проверява дали потребителят е логнат, съставя TreeDto и вика CreateTreeAsync.</summary>
    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        _errorLabel.IsVisible = false;
        if (string.IsNullOrWhiteSpace(_nameEntry.Text))
        {
            _errorLabel.Text = "Въведете име.";
            _errorLabel.IsVisible = true;
            return;
        }

        EnsureServices();
        if (!await _auth!.IsLoggedInAsync())
        {
            _errorLabel.Text = "Трябва да сте логнати.";
            _errorLabel.IsVisible = true;
            return;
        }

        string? photoUrl = null;
        if (_photoBytes != null && _photoBytes.Length > 0)
            photoUrl = "data:image/jpeg;base64," + Convert.ToBase64String(_photoBytes);

        var tree = new TreeDto
        {
            Name = _nameEntry.Text!.Trim(),
            PhotoURL = photoUrl,
            Latitude = double.TryParse(_latEntry.Text, out var lat) ? lat : 42.655780716559626,
            Longitude = double.TryParse(_lngEntry.Text, out var lng) ? lng : 24.747289594150434,
            DivisionId = _divisionPicker.SelectedIndex >= 0 && _divisionPicker.SelectedIndex < _divisions.Count ? _divisions[_divisionPicker.SelectedIndex].Id : 1,
            TaxonomyClassId = _classPicker.SelectedIndex >= 0 && _classPicker.SelectedIndex < _classes.Count ? _classes[_classPicker.SelectedIndex].Id : 1,
            FamilyId = _familyPicker.SelectedIndex >= 0 && _familyPicker.SelectedIndex < _families.Count ? _families[_familyPicker.SelectedIndex].Id : 1,
            GenusId = _genusPicker.SelectedIndex >= 0 && _genusPicker.SelectedIndex < _genera.Count ? _genera[_genusPicker.SelectedIndex].Id : 1,
            SpeciesId = _speciesPicker.SelectedIndex >= 0 && _speciesPicker.SelectedIndex < _species.Count ? _species[_speciesPicker.SelectedIndex].Id : 1
        };

        var ok = await _api!.CreateTreeAsync(tree);
        if (ok)
            await Shell.Current.GoToAsync("//Trees");
        else
        {
            _errorLabel.Text = "Неуспешно създаване. Проверете връзката с API.";
            _errorLabel.IsVisible = true;
        }
    }
}
