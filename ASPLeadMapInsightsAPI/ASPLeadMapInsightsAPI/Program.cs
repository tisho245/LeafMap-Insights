using System.Text;
using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// --- Конфигурация на услуги ---
var builder = WebApplication.CreateBuilder(args);

// База данни: SQL Server (LocalDB в разработка). LeafMapDbContext съдържа таблиците за таксономия + Identity.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<LeafMapDbContext>(options =>
    options.UseSqlServer(connectionString));

// ASP.NET Core Identity – потребители, пароли, роли. Храни се в същата БД чрез LeafMapDbContext.
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<LeafMapDbContext>()
.AddDefaultTokenProviders();

// JWT: ключът за подпис и параметрите за валидация – същите трябва да са в appsettings и при клиентите.
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not set.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LeafMapInsightsAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LeafMapInsights";

// Автентикация по Bearer токен. Всички [Authorize] заявки изискват валиден JWT в заглавката Authorization.
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

builder.Services.AddScoped<IJwtService, JwtService>();
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

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();  // Чете JWT от заглавката и задава User.
app.UseAuthorization();  // Проверява [Authorize] и роли.
app.MapControllers();

app.Run();
