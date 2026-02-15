namespace MAUILeafMapInsights.Services;

/// <summary>
/// Пази и чете JWT от SecureStorage. Как: SetAsync/GetAsync/Remove с ключ LeafMapJwtToken.
/// Защо SecureStorage: на мобилно няма „сесия“ като в браузър; SecureStorage е платформеното сигурно хранилище и не се изтрива при затваряне на приложението.
/// </summary>
public class AuthService
{
    private const string TokenKey = "LeafMapJwtToken";

    public async Task<string?> GetTokenAsync() => await SecureStorage.Default.GetAsync(TokenKey);

    public async Task SetTokenAsync(string token) => await SecureStorage.Default.SetAsync(TokenKey, token);

    /// <summary>Изтрива токена – при logout.</summary>
    public async Task RemoveTokenAsync() => SecureStorage.Default.Remove(TokenKey);

    public async Task<bool> IsLoggedInAsync() => !string.IsNullOrEmpty(await GetTokenAsync());
}
