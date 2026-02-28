namespace MAUILeafMapInsights.Services;

/// <summary>
/// Data API – дървета, таксономия. Auth API – вход, регистрация (отделен сървър при split).
/// </summary>
public static class ApiSettings
{
    public static string BaseUrl { get; set; } = "http://217.10.249.189:5202";
    public static string AuthApiBaseUrl { get; set; } = "http://217.10.249.189:5203";
}
