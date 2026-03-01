using System.Collections.ObjectModel;
using MAUILeafMapInsights.Models;
using MAUILeafMapInsights.Services;

namespace MAUILeafMapInsights.Pages;

/// <summary>Списък с дървета от API. Бутон „Добави дърво” само за логнати. При избор – навигира към TreeDetail.</summary>
public partial class TreesPage : ContentPage
{
    private LeafMapApiService? _api;
    private AuthService? _auth;
    private ObservableCollection<TreeDto> _trees = new();

    public TreesPage()
    {
        InitializeComponent();
        TreeList.ItemsSource = _trees;
        _ = LoadTreesAsync();
    }

    private LeafMapApiService Api => _api ??= AppServices.GetRequired<LeafMapApiService>();
    private AuthService Auth => _auth ??= AppServices.GetRequired<AuthService>();

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var logged = await Auth.IsLoggedInAsync();
        AddTreeButton.IsVisible = logged;
        await LoadTreesAsync();
    }

    /// <summary>Зарежда дърветата чрез GetTreesAsync и попълва ObservableCollection за ListView.</summary>
    private async Task LoadTreesAsync()
    {
        Loading.IsRunning = true;
        Loading.IsVisible = true;
        try
        {
            var list = await Api.GetTreesAsync();
            _trees.Clear();
            if (list != null)
                foreach (var t in list)
                    _trees.Add(t);
        }
        finally
        {
            Loading.IsRunning = false;
            Loading.IsVisible = false;
            RefreshView.IsRefreshing = false;
        }
    }

    private void OnRefreshClicked(object? sender, EventArgs e) => _ = LoadTreesAsync();

    private async void OnRefreshing(object? sender, EventArgs e) => await LoadTreesAsync();

    private async void OnAddTreeClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("AddTree");
    }

    /// <summary>При избор на дърво от списъка – навигираме към TreeDetail с id като параметър.</summary>
    private async void OnTreeSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is TreeDto tree)
        {
            TreeList.SelectedItem = null;
            await Shell.Current.GoToAsync($"TreeDetail?id={tree.Id}");
        }
    }
}
