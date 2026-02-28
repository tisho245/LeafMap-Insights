namespace MAUILeafMapInsights.Models;

/// <summary>Потребител от api/users (Auth API) – за Admin CRUD.</summary>
public class UserDto
{
    public string Id { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>Създаване на потребител – POST api/users.</summary>
public class CreateUserRequest
{
    public string UserName { get; set; } = "";
    public string? Email { get; set; }
    public string Password { get; set; } = "";
    public List<string>? Roles { get; set; }
}

/// <summary>Редакция на потребител – PUT api/users/{id}.</summary>
public class UpdateUserRequest
{
    public string UserName { get; set; } = "";
    public string? Email { get; set; }
    public List<string>? Roles { get; set; }
}
