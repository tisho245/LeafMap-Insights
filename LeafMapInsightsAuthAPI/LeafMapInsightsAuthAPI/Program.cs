using System.Runtime.InteropServices;
using System.Text;
using ASPLeadMapInsightsAPI.Data;
using ASPLeadMapInsightsAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// --- Auth-only API: login, register, JWT issuance. Same DB as Data API for Identity. ---
var builder = WebApplication.CreateBuilder(args);
if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
    builder.WebHost.UseUrls("http://0.0.0.0:5203");

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

// JWT generation only (no validation here)
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "LeafMap Insights Auth API", Version = "v1" }));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// JWT validation – същите Key/Issuer/Audience като при издаване, за да защитим api/users с [Authorize(Roles = "Admin")].
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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LeafMapDbContext>();
    await db.Database.MigrateAsync();

    // Seed default roles and an administrator user if they do not exist yet.
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    var userRoleName = "User";
    var adminRoleName = "Admin";

    if (!await roleManager.RoleExistsAsync(userRoleName))
        await roleManager.CreateAsync(new IdentityRole(userRoleName));

    if (!await roleManager.RoleExistsAsync(adminRoleName))
        await roleManager.CreateAsync(new IdentityRole(adminRoleName));

    // Admin credentials – configurable via appsettings, with safe defaults for development.
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var adminEmail = config["Admin:Email"] ?? "admin@leafmap.local";
    var adminPassword = config["Admin:Password"] ?? "SuperAdmin2026";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }
    else
    {
        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
        {
            await userManager.AddToRoleAsync(adminUser, adminRoleName);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
