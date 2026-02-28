using System.Runtime.InteropServices;
using System.Text;
using ASPLeadMapInsightsAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// --- Data API: trees, taxonomy. JWT is validated only (tokens issued by Auth API). ---
var builder = WebApplication.CreateBuilder(args);
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
    builder.WebHost.UseUrls("http://0.0.0.0:5202");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// LocalDB is Windows-only; on Linux use SQL Server (env or default).
if (connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase) && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? "Server=localhost,1433;Database=LeafMapInsights;User Id=sa;Password=SuperAdmin2026;TrustServerCertificate=True;";
}
builder.Services.AddDbContext<LeafMapDbContext>(options =>
    options.UseSqlServer(connectionString));

// JWT validation only – same Key/Issuer/Audience as Auth API so tokens from Auth server are accepted.
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not set.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LeafMapInsightsAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LeafMapInsights";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS – разрешаваме заявки от произход. Защо: Web и Mobile са на различни порт/домейн от API; без CORS браузърът блокира заявките.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// При стартиране: прилагаме миграциите и запълваме начални данни ако БД е празна.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeafMapDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.EnsureSeededAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// В Development не правим HTTPS redirect, за да не се чупи CORS preflight при заявки от file:// или друг origin към http://localhost:5202
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();  // Чете JWT от заглавката и задава User.
app.UseAuthorization();  // Проверява [Authorize] и роли.
app.MapControllers();

app.Run();
