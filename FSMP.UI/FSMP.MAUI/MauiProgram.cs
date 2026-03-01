using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.MAUI.Pages;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Audio;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FSMP.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMaui()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        RegisterServices(builder.Services);

        return builder.Build();
    }

    static void RegisterServices(IServiceCollection services)
    {
        // Database — share the same path as the console app
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = Path.Combine(appData, "FSMP", "fsmp.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddDbContext<FsmpDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath};Pooling=False"),
            ServiceLifetime.Scoped);

        services.AddScoped<UnitOfWork>();

        // Data services
        services.AddScoped<LibraryScanService>();
        services.AddScoped<PlaybackTrackingService>();
        services.AddScoped<StatisticsService>();
        services.AddScoped<PlaylistService>();

        // Library services
        services.AddScoped<MetadataService>();

        // Config service (uses same %AppData%\FSMP\config.json as console app)
        var configPath = Path.Combine(appData, "FSMP", "config.json");
        services.AddSingleton(_ => new ConfigurationService(configPath));

        // Audio
        services.AddSingleton<IAudioPlayerFactory, LibVlcAudioPlayerFactory>();
        services.AddSingleton<IAudioService, AudioService>();
        services.AddSingleton<ActivePlaylistService>();
        services.AddSingleton<IActivePlaylistService>(sp => sp.GetRequiredService<ActivePlaylistService>());

        // Pages
        services.AddTransient<LibraryPage>();
        services.AddTransient<NowPlayingPage>();
        services.AddTransient<SettingsPage>();
    }
}
