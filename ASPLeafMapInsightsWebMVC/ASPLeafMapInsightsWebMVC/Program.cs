using ASPLeafMapInsightsWebMVC.Services;
var builder = WebApplication.CreateBuilder(args);

// Именуван HttpClient за API заявки. В разработка игнорираме SSL грешки (само за localhost).
var leafMapBuilder = builder.Services.AddHttpClient("LeafMapApi");
if (builder.Environment.IsDevelopment())
    leafMapBuilder.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true });

builder.Services.AddHttpContextAccessor();  // За LeafMapApiClient – достъп до Session за JWT.
builder.Services.AddScoped<ILeafMapApiClient, LeafMapApiClient>();

// Session – тук пазим JWT след логин (ключ "Token"). Защо Session, а не cookie с токена: Session е сървърно свързан с cookie; по-лесно за една уеб апликация без отделен token store.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();   // Трябва преди контролерите, за да четат Session.
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
