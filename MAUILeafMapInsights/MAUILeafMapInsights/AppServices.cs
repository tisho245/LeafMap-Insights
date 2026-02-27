using Microsoft.Extensions.DependencyInjection;

namespace MAUILeafMapInsights;

/// <summary>
/// Достъп до DI контейнера от страниците. Как: Services се задава в App.CreateWindow; страниците викат GetRequired&lt;T&gt;().
/// Защо статичен достъп: в MAUI страниците се създават чрез Shell маршрути и не получават услуги в конструктора – тук ги взимат при нужда.
/// </summary>
public static class AppServices
{
    public static IServiceProvider? Services { get; set; }

    public static T? Get<T>() where T : class => Services?.GetService<T>();
    public static T GetRequired<T>() where T : class => (Services?.GetRequiredService<T>()) ?? throw new InvalidOperationException(
        Services == null ? "AppServices.Services is null. Ensure MauiProgram sets AppServices.Services = app.Services after Build()." : $"Type {typeof(T).Name} is not registered in DI.");
}
