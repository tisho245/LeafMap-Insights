using System.Text.Json;

namespace MAUILeafMapInsights.Services;

/// <summary>
/// Пази и чете JWT от SecureStorage. Как: SetAsync/GetAsync/Remove с ключ LeafMapJwtToken.
/// Защо SecureStorage: на мобилно няма „сесия“ като в браузър; SecureStorage е платформеното сигурно хранилище и не се изтрива при затваряне на приложението.
/// Ролите се пазят в Preferences (при login/register от отговора) или се четат от JWT при рестарт.
/// </summary>
public class AuthService
{
    private const string TokenKey = "LeafMapJwtToken";
    private const string RolesKey = "LeafMapRoles";

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.Default.GetAsync(TokenKey);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task SetTokenAsync(string token) => await SecureStorage.Default.SetAsync(TokenKey, token);

    /// <summary>Записва ролите след login/register (извиква се от LoginPage/RegisterPage с response.Roles).</summary>
    public void SetRoles(IEnumerable<string> roles)
    {
        var json = JsonSerializer.Serialize(roles.ToList());
        Preferences.Default.Set(RolesKey, json);
    }

    /// <summary>Връща ролите – от Preferences или от JWT payload при рестарт на приложението.</summary>
    public async Task<IReadOnlyList<string>> GetRolesAsync()
    {
        var stored = Preferences.Default.Get<string?>(RolesKey, null);
        if (!string.IsNullOrEmpty(stored))
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<string>>(stored);
                if (list != null) return list;
            }
            catch { /* ignore */ }
        }

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return Array.Empty<string>();

        var roles = GetRolesFromJwt(token);
        if (roles.Count > 0)
            SetRoles(roles);
        return roles;
    }

    /// <summary>Дали текущият потребител е в роля Admin.</summary>
    public async Task<bool> IsAdminAsync()
    {
        var roles = await GetRolesAsync();
        return roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Изтрива токена и ролите – при logout.</summary>
    public async Task RemoveTokenAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        Preferences.Default.Remove(RolesKey);
        await Task.CompletedTask;
    }

    public async Task<bool> IsLoggedInAsync() => !string.IsNullOrEmpty(await GetTokenAsync());

    private static IReadOnlyList<string> GetRolesFromJwt(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) return Array.Empty<string>();

        try
        {
            var payload = parts[1];
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            var bytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // ASP.NET Core Identity: ClaimTypes.Role -> "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            if (root.TryGetProperty("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out var roleEl))
            {
                if (roleEl.ValueKind == JsonValueKind.Array)
                {
                    var list = new List<string>();
                    foreach (var e in roleEl.EnumerateArray())
                        if (e.ValueKind == JsonValueKind.String) list.Add(e.GetString() ?? "");
                    return list;
                }
                if (roleEl.ValueKind == JsonValueKind.String)
                    return new[] { roleEl.GetString() ?? "" };
            }
            if (root.TryGetProperty("role", out roleEl))
            {
                if (roleEl.ValueKind == JsonValueKind.String)
                    return new[] { roleEl.GetString() ?? "" };
            }
        }
        catch { /* ignore */ }
        return Array.Empty<string>();
    }
}
