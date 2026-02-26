using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MAUILeafMapInsights.Models;

namespace MAUILeafMapInsights.Services;

/// <summary>
/// Един HttpClient за всички заявки към API. Как: BaseAddress от ApiSettings.BaseUrl; преди всяка заявка EnsureTokenAsync() слага текущия JWT в Authorization.
/// Защо преди всяка заявка: HttpClient е singleton – след login или logout токенът се сменя и заглавката трябва да се обнови.
/// </summary>
public class LeafMapApiService
{
    private readonly HttpClient _http;
    private readonly AuthService _auth;
    private readonly JsonSerializerOptions _jsonOpt = new() { PropertyNameCaseInsensitive = true };

    public LeafMapApiService(HttpClient http, AuthService auth)
    {
        _http = http;
        _auth = auth;
        _http.BaseAddress = new Uri(ApiSettings.BaseUrl.TrimEnd('/'));
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>Обновява заглавката Authorization с текущия токен от SecureStorage (ако има).</summary>
    private async Task EnsureTokenAsync()
    {
        var token = await _auth.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
            ? null
            : new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>Вход – POST към Auth API (api/auth/login). При успех връща LoginResponse с Token.</summary>
    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var baseAuth = ApiSettings.AuthApiBaseUrl.TrimEnd('/');
        var body = JsonSerializer.Serialize(new LoginRequest { Email = email, Password = password });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var res = await _http.PostAsync($"{baseAuth}/api/auth/login", content, ct);
        if (!res.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<LoginResponse>(await res.Content.ReadAsStringAsync(ct), _jsonOpt);
    }

    /// <summary>Регистрация – POST към Auth API (api/auth/register). При успех връща LoginResponse с Token.</summary>
    public async Task<LoginResponse?> RegisterAsync(string email, string password, string? userName, CancellationToken ct = default)
    {
        var baseAuth = ApiSettings.AuthApiBaseUrl.TrimEnd('/');
        var body = JsonSerializer.Serialize(new RegisterRequest { Email = email, Password = password, UserName = userName });
        var content = new StringContent(body, Encoding.UTF8, "application/json");
        var res = await _http.PostAsync($"{baseAuth}/api/auth/register", content, ct);
        if (!res.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<LoginResponse>(await res.Content.ReadAsStringAsync(ct), _jsonOpt);
    }

    /// <summary>Списък всички дървета; includeLookups включва Division, Species и др.</summary>
    public async Task<List<TreeDto>?> GetTreesAsync(bool includeLookups = true, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var path = includeLookups ? "api/trees?includeLookups=true" : "api/trees";
        return await GetAsync<List<TreeDto>>(path, ct);
    }

    /// <summary>Едно дърво по id.</summary>
    public async Task<TreeDto?> GetTreeAsync(int id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return await GetAsync<TreeDto>($"api/trees/{id}?includeLookups=true", ct);
    }

    /// <summary>Създава ново дърво – POST api/trees. Изисква валиден JWT.</summary>
    public async Task<bool> CreateTreeAsync(TreeDto tree, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(new
        {
            name = tree.Name,
            photoURL = tree.PhotoURL,
            description = tree.Description,
            latitude = tree.Latitude,
            longitude = tree.Longitude,
            divisionId = tree.DivisionId,
            taxonomyClassId = tree.TaxonomyClassId,
            genusId = tree.GenusId,
            familyId = tree.FamilyId,
            speciesId = tree.SpeciesId
        });
        var res = await _http.PostAsync("api/trees", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    /// <summary>Референтни списъци за таксономия – използват се в AddTreePage за dropdown-и.</summary>
    public async Task<List<DivisionDto>?> GetDivisionsAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return await GetAsync<List<DivisionDto>>("api/divisions", ct);
    }

    public async Task<List<TaxonomyClassDto>?> GetTaxonomyClassesAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return await GetAsync<List<TaxonomyClassDto>>("api/taxonomyclasses", ct);
    }

    public async Task<List<GenusDto>?> GetGeneraAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return await GetAsync<List<GenusDto>>("api/genera", ct);
    }

    public async Task<List<FamilyDto>?> GetFamiliesAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return await GetAsync<List<FamilyDto>>("api/families", ct);
    }

    public async Task<List<SpeciesDto>?> GetSpeciesAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return await GetAsync<List<SpeciesDto>>("api/species", ct);
    }

    /// <summary>Списък потребители – изисква Admin, вика Auth API api/users.</summary>
    public async Task<List<UserDto>?> GetUsersAsync(CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var baseAuth = ApiSettings.AuthApiBaseUrl.TrimEnd('/');
        var res = await _http.GetAsync($"{baseAuth}/api/users", ct);
        if (!res.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<List<UserDto>>(await res.Content.ReadAsStringAsync(ct), _jsonOpt);
    }

    /// <summary>Вътрешен GET помощник – десериализира JSON отговор в T.</summary>
    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        var res = await _http.GetAsync(path, ct);
        if (!res.IsSuccessStatusCode) return default;
        return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(ct), _jsonOpt);
    }
}
