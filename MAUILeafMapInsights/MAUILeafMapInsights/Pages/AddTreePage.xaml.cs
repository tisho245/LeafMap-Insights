using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Форма за добавяне на ново дърво. Зарежда референтни списъци (Division, Species и др.) за Picker-и, позволява добавяне на снимка (камера/галерия) и изпраща POST api/trees.</summary>
public partial class AddTreePage : ContentPage
{
    private LeafMapApiService? _api;
    private AuthService? _auth;
    private Entry _nameEntry = null!;
    private Entry _latEntry = null!;
    private Entry _lngEntry = null!;
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
        LoadTaxonomy();
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

    /// <summary>Създава полета и Picker-и за формата, преглед на снимка, бутони за снимка и Създай.</summary>
    private void BuildForm()
    {
        _nameEntry = new Entry { Placeholder = "Име *" };
        _latEntry = new Entry { Placeholder = "Ширина", Keyboard = Keyboard.Numeric, Text = "42.6977" };
        _lngEntry = new Entry { Placeholder = "Дължина", Keyboard = Keyboard.Numeric, Text = "23.3219" };
        _divisionPicker = new Picker { Title = "Отдел" };
        _classPicker = new Picker { Title = "Клас" };
        _familyPicker = new Picker { Title = "Семейство" };
        _genusPicker = new Picker { Title = "Род" };
        _speciesPicker = new Picker { Title = "Вид" };
        _errorLabel = new Label { TextColor = Colors.Red, IsVisible = false };

        _photoPreview = new Image
        {
            HeightRequest = 160,
            WidthRequest = 160,
            BackgroundColor = Colors.LightGray,
            Aspect = Aspect.AspectFill
        };
        var photoLabel = new Label { Text = "Снимка на дървото", FontAttributes = FontAttributes.Bold };
        var takePhotoBtn = new Button { Text = "Вземи снимка" };
        takePhotoBtn.Clicked += OnTakePhotoClicked;
        var pickPhotoBtn = new Button { Text = "Избери снимка от галерията", BackgroundColor = Colors.Transparent, TextColor = Color.FromArgb("#512BD4") };
        pickPhotoBtn.Clicked += OnPickPhotoClicked;

        FormStack.Children.Add(new Label { Text = "Добави дърво", FontSize = 22 });
        FormStack.Children.Add(_nameEntry);
        FormStack.Children.Add(_latEntry);
        FormStack.Children.Add(_lngEntry);
        FormStack.Children.Add(photoLabel);
        FormStack.Children.Add(_photoPreview);
        FormStack.Children.Add(takePhotoBtn);
        FormStack.Children.Add(pickPhotoBtn);
        FormStack.Children.Add(_divisionPicker);
        FormStack.Children.Add(_classPicker);
        FormStack.Children.Add(_familyPicker);
        FormStack.Children.Add(_genusPicker);
        FormStack.Children.Add(_speciesPicker);
        FormStack.Children.Add(_errorLabel);
        var submitBtn = new Button { Text = "Създай" };
        submitBtn.Clicked += OnSubmitClicked;
        FormStack.Children.Add(submitBtn);
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
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

    private async void OnPickPhotoClicked(object sender, EventArgs e)
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
    private async void LoadTaxonomy()
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
    private async void OnSubmitClicked(object sender, EventArgs e)
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
            Latitude = double.TryParse(_latEntry.Text, out var lat) ? lat : 42.6977,
            Longitude = double.TryParse(_lngEntry.Text, out var lng) ? lng : 23.3219,
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
