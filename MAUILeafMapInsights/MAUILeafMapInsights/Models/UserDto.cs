namespace MAUILeafMapInsights.Models;

/// <summary>Потребител от api/users (Auth API) – само за Admin.</summary>
public class UserDto
{
    public string Id { get; set; } = "";
    public string Email { get; set; } = "";
    public List<string> Roles { get; set; } = new();
}
