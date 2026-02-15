using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ASPLeafMapInsightsWebMVC.Services;

/// <summary>
/// Имплементация на ILeafMapApiClient. Как: при всяка заявка CreateClient() с BaseAddress от ApiBaseUrl и Bearer от Session.
/// Защо при всяка заявка: за да винаги изпращаме актуалния токен (след login/logout заглавката трябва да се обнови).
/// </summary>
public class LeafMapApiClient : ILeafMapApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _httpContext;
    private readonly string _baseUrl;

    public LeafMapApiClient(IHttpClientFactory factory, IHttpContextAccessor httpContext, IConfiguration config)
    {
        _factory = factory;
        _httpContext = httpContext;
        _baseUrl = config["ApiBaseUrl"]?.TrimEnd('/') ?? "";
    }

    /// <summary>Създава HttpClient с BaseAddress и Bearer токен от Session (ако има).</summary>
    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("LeafMapApi");
        client.BaseAddress = new Uri(_baseUrl);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var token = _httpContext.HttpContext?.Session.GetString("Token");
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>GET заявка; десериализира отговор като JSON в T. При неуспешен статус връща default.</summary>
    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
    {
        var client = CreateClient();
        var res = await client.GetAsync(path, ct);
        if (!res.IsSuccessStatusCode) return default;
        var json = await res.Content.ReadAsStringAsync(ct);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    /// <summary>POST с JSON тяло; връща HttpResponseMessage за проверка на статус в контролера.</summary>
    public async Task<HttpResponseMessage> PostAsync(string path, object body, CancellationToken ct = default)
    {
        var client = CreateClient();
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PostAsync(path, content, ct);
    }

    public async Task<HttpResponseMessage> PutAsync(string path, object body, CancellationToken ct = default)
    {
        var client = CreateClient();
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await client.PutAsync(path, content, ct);
    }

    /// <summary>DELETE заявка.</summary>
    public async Task<HttpResponseMessage> DeleteAsync(string path, CancellationToken ct = default)
    {
        var client = CreateClient();
        return await client.DeleteAsync(path, ct);
    }
}
