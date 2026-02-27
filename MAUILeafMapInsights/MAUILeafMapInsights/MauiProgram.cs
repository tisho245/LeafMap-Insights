using MAUILeafMapInsights.Pages;
using MAUILeafMapInsights.Services;
using Microsoft.Extensions.Logging;

namespace MAUILeafMapInsights;

/// <summary>Регистрация на приложението, шрифтове и DI – AuthService, HttpClient, LeafMapApiService и всички страници.</summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
        var builder = MauiApp.CreateBuilder();
        var maui = builder.UseMauiApp<App>();
        // Картите не са поддържани на Windows – UseMauiMaps() може да крашва. Включваме само на Android/iOS/MacCatalyst.
#if ANDROID || IOS || MACCATALYST
        maui.UseMauiMaps();
#endif
        // Шрифтове: ако добавите OpenSans-Regular.ttf и OpenSans-Semibold.ttf в Resources/Fonts/, добавете: .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold"); });

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
        builder.Services.AddTransient<AdminPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        // Задаваме AppServices още тук – на Android activationState.Context?.Services често е null при CreateWindow.
        AppServices.Services = app.Services;
        return app;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MauiProgram.CreateMauiApp failed: {ex}");
            throw;
        }
    }
}
