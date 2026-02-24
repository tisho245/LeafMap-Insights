using MAUILeafMapInsights.Pages;
using MAUILeafMapInsights.Services;
using Microsoft.Extensions.Logging;

namespace MAUILeafMapInsights;

/// <summary>Регистрация на приложението, шрифтове и DI – AuthService, HttpClient, LeafMapApiService и всички страници.</summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiMaps()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Singleton – един инстанс за цялото приложение. HttpClient и LeafMapApiService използват ApiSettings.BaseUrl.
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<LeafMapApiService>();

        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<TreesPage>();
        builder.Services.AddTransient<TreeDetailPage>();
        builder.Services.AddTransient<AddTreePage>();
        builder.Services.AddTransient<TaxonomyPage>();
        builder.Services.AddTransient<MapPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
