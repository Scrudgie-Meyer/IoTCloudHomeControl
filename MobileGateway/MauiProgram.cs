using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace MobileGateway;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Завантаження appsettings.json як embedded resource
        using var stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("MobileGateway.appsettings.json");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream!)
            .Build();

        builder.Configuration.AddConfiguration(config);
        builder.Services.AddSingleton<IConfiguration>(config);
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<App>();


        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
