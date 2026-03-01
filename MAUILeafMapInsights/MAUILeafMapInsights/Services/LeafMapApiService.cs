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

    /// <summary>Вход – POST към Auth API (api/auth/login). API очаква userName и password; изпращаме въведения текст като userName.</summary>
    public async Task<LoginResponse?> LoginAsync(string userNameOrEmail, string password, CancellationToken ct = default)
    {
        var baseAuth = ApiSettings.AuthApiBaseUrl.TrimEnd('/');
        var body = JsonSerializer.Serialize(new LoginRequest { UserName = userNameOrEmail, Password = password });
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

    /// <summary>Дървета в видимите граници на картата (minLat, minLng, maxLat, maxLng).</summary>
    public async Task<List<TreeDto>?> GetTreesWithinBoundsAsync(double minLat, double minLng, double maxLat, double maxLng, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var path = $"api/trees/WithinBounds?minLat={minLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&minLng={minLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}&maxLat={maxLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}&maxLng={maxLng.ToString(System.Globalization.CultureInfo.InvariantCulture)}&includeLookups=true";
        return await GetAsync<List<TreeDto>>(path, ct);
    }

    /// <summary>Едно дърво по id.</summary>
    public async Task<TreeDto?> GetTreeAsync(int id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return await GetAsync<TreeDto>($"api/trees/{id}?includeLookups=true", ct);
    }

    /// <summary>Обновява съществуващо дърво – PUT api/trees/{id}. Изисква валиден JWT. API очаква camelCase.</summary>
    public async Task<bool> UpdateTreeAsync(int id, TreeDto tree, CancellationToken ct = default)
    {
        var (ok, _) = await UpdateTreeWithErrorAsync(id, tree, ct);
        return ok;
    }

    /// <summary>Същото като UpdateTreeAsync, но при грешка връща съобщението от сървъра. Изпращаме Genus, Family и др. като обекти с Id (PascalCase), както очаква валидацията.</summary>
    public async Task<(bool success, string? error)> UpdateTreeWithErrorAsync(int id, TreeDto tree, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var payload = new Dictionary<string, object?>
        {
            ["Id"] = id,
            ["Name"] = tree.Name,
            ["PhotoURL"] = tree.PhotoURL,
            ["Description"] = tree.Description,
            ["Latitude"] = tree.Latitude,
            ["Longitude"] = tree.Longitude,
            ["DivisionId"] = tree.DivisionId,
            ["TaxonomyClassId"] = tree.TaxonomyClassId,
            ["GenusId"] = tree.GenusId,
            ["FamilyId"] = tree.FamilyId,
            ["SpeciesId"] = tree.SpeciesId,
            ["Division"] = new Dictionary<string, object> { ["Id"] = tree.DivisionId },
            ["TaxonomyClass"] = new Dictionary<string, object> { ["Id"] = tree.TaxonomyClassId },
            ["Genus"] = new Dictionary<string, object> { ["Id"] = tree.GenusId },
            ["Family"] = new Dictionary<string, object> { ["Id"] = tree.FamilyId },
            ["Species"] = new Dictionary<string, object> { ["Id"] = tree.SpeciesId }
        };
        var jsonOpt = new JsonSerializerOptions { PropertyNamingPolicy = null, WriteIndented = false };
        var body = JsonSerializer.Serialize(payload, jsonOpt);
        var res = await _http.PutAsync($"api/trees/{id}", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        if (res.IsSuccessStatusCode) return (true, null);
        var err = await res.Content.ReadAsStringAsync(ct);
        return (false, string.IsNullOrWhiteSpace(err) ? res.ReasonPhrase : err);
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

    /// <summary>Един потребител по id – Auth API api/users/{id}.</summary>
    public async Task<UserDto?> GetUserByIdAsync(string id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var baseAuth = ApiSettings.AuthApiBaseUrl.TrimEnd('/');
        var res = await _http.GetAsync($"{baseAuth}/api/users/{Uri.EscapeDataString(id)}", ct);
        if (!res.IsSuccessStatusCode) return null;
        return JsonSerializer.Deserialize<UserDto>(await res.Content.ReadAsStringAsync(ct), _jsonOpt);
    }

    /// <summary>Създава потребител – POST Auth API api/users.</summary>
    public async Task<(bool ok, string? error)> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var baseAuth = ApiSettings.AuthApiBaseUrl.TrimEnd('/');
        var body = JsonSerializer.Serialize(request);
        var res = await _http.PostAsync($"{baseAuth}/api/users", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        if (res.IsSuccessStatusCode) return (true, null);
        var err = await res.Content.ReadAsStringAsync(ct);
        return (false, err);
    }

    /// <summary>Обновява потребител – PUT Auth API api/users/{id}.</summary>
    public async Task<(bool ok, string? error)> UpdateUserAsync(string id, UpdateUserRequest request, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var baseAuth = ApiSettings.AuthApiBaseUrl.TrimEnd('/');
        var body = JsonSerializer.Serialize(request);
        var res = await _http.PutAsync($"{baseAuth}/api/users/{Uri.EscapeDataString(id)}", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await res.Content.ReadAsStringAsync(ct));
    }

    /// <summary>Изтрива потребител – DELETE Auth API api/users/{id}.</summary>
    public async Task<(bool ok, string? error)> DeleteUserAsync(string id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var baseAuth = ApiSettings.AuthApiBaseUrl.TrimEnd('/');
        var res = await _http.DeleteAsync($"{baseAuth}/api/users/{Uri.EscapeDataString(id)}", ct);
        if (res.IsSuccessStatusCode) return (true, null);
        return (false, await res.Content.ReadAsStringAsync(ct));
    }

    /// <summary>Изтрива дърво – DELETE api/trees/{id}. Admin.</summary>
    public async Task<bool> DeleteTreeAsync(int id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var res = await _http.DeleteAsync($"api/trees/{id}", ct);
        return res.IsSuccessStatusCode;
    }

    /// <summary>Създава отдел – POST api/divisions. Admin.</summary>
    public async Task<bool> CreateDivisionAsync(DivisionDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(dto);
        var res = await _http.PostAsync("api/divisions", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateDivisionAsync(int id, DivisionDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(new DivisionDto { Id = id, Name = dto.Name });
        var res = await _http.PutAsync($"api/divisions/{id}", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteDivisionAsync(int id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return (await _http.DeleteAsync($"api/divisions/{id}", ct)).IsSuccessStatusCode;
    }

    public async Task<bool> CreateTaxonomyClassAsync(TaxonomyClassDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(dto);
        var res = await _http.PostAsync("api/taxonomyclasses", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateTaxonomyClassAsync(int id, TaxonomyClassDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(new TaxonomyClassDto { Id = id, Name = dto.Name });
        var res = await _http.PutAsync($"api/taxonomyclasses/{id}", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTaxonomyClassAsync(int id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return (await _http.DeleteAsync($"api/taxonomyclasses/{id}", ct)).IsSuccessStatusCode;
    }

    public async Task<bool> CreateFamilyAsync(FamilyDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(dto);
        var res = await _http.PostAsync("api/families", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateFamilyAsync(int id, FamilyDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(new FamilyDto { Id = id, Name = dto.Name });
        var res = await _http.PutAsync($"api/families/{id}", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteFamilyAsync(int id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return (await _http.DeleteAsync($"api/families/{id}", ct)).IsSuccessStatusCode;
    }

    public async Task<bool> CreateGenusAsync(GenusDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(dto);
        var res = await _http.PostAsync("api/genera", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateGenusAsync(int id, GenusDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(new GenusDto { Id = id, Name = dto.Name });
        var res = await _http.PutAsync($"api/genera/{id}", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteGenusAsync(int id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return (await _http.DeleteAsync($"api/genera/{id}", ct)).IsSuccessStatusCode;
    }

    public async Task<bool> CreateSpeciesAsync(SpeciesDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(dto);
        var res = await _http.PostAsync("api/species", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSpeciesAsync(int id, SpeciesDto dto, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        var body = JsonSerializer.Serialize(new SpeciesDto { Id = id, Name = dto.Name });
        var res = await _http.PutAsync($"api/species/{id}", new StringContent(body, Encoding.UTF8, "application/json"), ct);
        return res.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSpeciesAsync(int id, CancellationToken ct = default)
    {
        await EnsureTokenAsync();
        return (await _http.DeleteAsync($"api/species/{id}", ct)).IsSuccessStatusCode;
    }

    /// <summary>Вътрешен GET помощник – десериализира JSON отговор в T.</summary>
    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        var res = await _http.GetAsync(path, ct);
        if (!res.IsSuccessStatusCode) return default;
        return JsonSerializer.Deserialize<T>(await res.Content.ReadAsStringAsync(ct), _jsonOpt);
    }
}
