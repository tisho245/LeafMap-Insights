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
        _nameEntry = new Entry { Placeholder = "Име *" };
        _latEntry = new Entry { Placeholder = "Ширина", Keyboard = Keyboard.Numeric };
        _lngEntry = new Entry { Placeholder = "Дължина", Keyboard = Keyboard.Numeric };
        _descEntry = new Entry { Placeholder = "Описание" };
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
        FormStack.Children.Add(_latEntry);
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
        var divIdx = _divisions.FindIndex(d => d.Id == tree.DivisionId); if (divIdx >= 0) _divisionPicker.SelectedIndex = divIdx;
        var clIdx = _classes.FindIndex(c => c.Id == tree.TaxonomyClassId); if (clIdx >= 0) _classPicker.SelectedIndex = clIdx;
        var famIdx = _families.FindIndex(f => f.Id == tree.FamilyId); if (famIdx >= 0) _familyPicker.SelectedIndex = famIdx;
        var genIdx = _genera.FindIndex(g => g.Id == tree.GenusId); if (genIdx >= 0) _genusPicker.SelectedIndex = genIdx;
        var specIdx = _species.FindIndex(s => s.Id == tree.SpeciesId); if (specIdx >= 0) _speciesPicker.SelectedIndex = specIdx;
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
        if (_photoBytes != null && _photoBytes.Length > 0) photoUrl = "data:image/jpeg;base64," + Convert.ToBase64String(_photoBytes);
        var tree = new TreeDto
        {
            Id = _treeId,
            Name = _nameEntry.Text!.Trim(),
            Description = string.IsNullOrWhiteSpace(_descEntry.Text) ? null : _descEntry.Text.Trim(),
            PhotoURL = photoUrl,
            Latitude = double.TryParse(_latEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lat) ? lat : 42.655780716559626,
            Longitude = double.TryParse(_lngEntry.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var lng) ? lng : 24.747289594150434,
            DivisionId = _divisionPicker.SelectedIndex >= 0 && _divisionPicker.SelectedIndex < _divisions.Count ? _divisions[_divisionPicker.SelectedIndex].Id : 1,
            TaxonomyClassId = _classPicker.SelectedIndex >= 0 && _classPicker.SelectedIndex < _classes.Count ? _classes[_classPicker.SelectedIndex].Id : 1,
            FamilyId = _familyPicker.SelectedIndex >= 0 && _familyPicker.SelectedIndex < _families.Count ? _families[_familyPicker.SelectedIndex].Id : 1,
            GenusId = _genusPicker.SelectedIndex >= 0 && _genusPicker.SelectedIndex < _genera.Count ? _genera[_genusPicker.SelectedIndex].Id : 1,
            SpeciesId = _speciesPicker.SelectedIndex >= 0 && _speciesPicker.SelectedIndex < _species.Count ? _species[_speciesPicker.SelectedIndex].Id : 1
        };
        if (await Api.UpdateTreeAsync(_treeId, tree)) await Shell.Current.GoToAsync("..");
        else { _errorLabel.Text = "Неуспешно запазване."; _errorLabel.IsVisible = true; }
    }
}
