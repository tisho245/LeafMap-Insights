namespace MAUILeafMapInsights.Services;

/// <summary>
/// Базов URL на LeafMap API. По подразбиране localhost; за Android емулатор използвайте http://10.0.2.2:5202
/// (10.0.2.2 сочи към host машината от емулатора).
/// </summary>
public static class ApiSettings
{
    public static string BaseUrl { get; set; } = "https://localhost:7234";
}
