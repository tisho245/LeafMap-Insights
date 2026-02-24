namespace MAUILeafMapInsights.Services;

/// <summary>
/// Data API – дървета, таксономия. Auth API – вход, регистрация (отделен сървър при split).
/// За Android емулатор: BaseUrl = http://10.0.2.2:5202, AuthApiBaseUrl = http://10.0.2.2:5203 (ако Auth е на 5203).
/// </summary>
public static class ApiSettings
{
    public static string BaseUrl { get; set; } = "https://localhost:7234";
    public static string AuthApiBaseUrl { get; set; } = "https://localhost:7240";
}
