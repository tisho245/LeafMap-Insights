namespace ASPLeafMapInsightsWebMVC.Services;

/// <summary>
/// Клиент за LeafMap API. Всички заявки отиват към ApiBaseUrl; при наличие на JWT в Session
/// той се изпраща в заглавката Authorization: Bearer. Използва се от Trees, Taxonomy, Map контролерите.
/// </summary>
public interface ILeafMapApiClient
{
    Task<T?> GetAsync<T>(string path, CancellationToken ct = default);
    Task<HttpResponseMessage> PostAsync(string path, object body, CancellationToken ct = default);
    Task<HttpResponseMessage> PutAsync(string path, object body, CancellationToken ct = default);
    Task<HttpResponseMessage> DeleteAsync(string path, CancellationToken ct = default);
}
