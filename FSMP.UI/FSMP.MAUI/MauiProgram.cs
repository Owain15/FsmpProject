using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Services;
using FSMP.MAUI.Pages;
using FSMP.MAUI.ViewModels;
using FSMP.Platform.Windows.Audio;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
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
            .UseMauiApp<App>()
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

        // Interface mappings for services already registered as concrete types
        services.AddScoped<IMetadataService>(sp => sp.GetRequiredService<MetadataService>());
        services.AddScoped<ILibraryScanService>(sp => sp.GetRequiredService<LibraryScanService>());
        services.AddScoped<IPlaylistService>(sp => sp.GetRequiredService<PlaylistService>());
        services.AddSingleton<IConfigurationService>(sp => sp.GetRequiredService<ConfigurationService>());

        // Repository interfaces (resolved from UnitOfWork)
        services.AddScoped<ITrackRepository>(sp => sp.GetRequiredService<UnitOfWork>().Tracks);
        services.AddScoped<IArtistRepository>(sp => sp.GetRequiredService<UnitOfWork>().Artists);
        services.AddScoped<IAlbumRepository>(sp => sp.GetRequiredService<UnitOfWork>().Albums);

        // Orchestration layer
        services.AddScoped<IPlaybackController, PlaybackController>();
        services.AddScoped<ILibraryBrowser, LibraryBrowser>();
        services.AddScoped<ILibraryManager, LibraryManager>();
        services.AddScoped<IPlaylistManager, PlaylistManager>();

        // Pages
        services.AddTransient<LibraryPage>();
        services.AddTransient<NowPlayingPage>();
        services.AddTransient<SettingsPage>();
        services.AddTransient<PlaylistsPage>();

        // ViewModels
        services.AddTransient<NowPlayingViewModel>();
        services.AddTransient<Core.ViewModels.LibraryBrowseViewModel>(sp =>
            new Core.ViewModels.LibraryBrowseViewModel(
                sp.GetRequiredService<ILibraryBrowser>(),
                sp.GetRequiredService<IPlaybackController>(),
                MainThread.BeginInvokeOnMainThread));
        services.AddTransient<Core.ViewModels.SettingsViewModel>(sp =>
            new Core.ViewModels.SettingsViewModel(
                sp.GetRequiredService<ILibraryManager>(),
                sp.GetRequiredService<IConfigurationService>(),
                MainThread.BeginInvokeOnMainThread));
        services.AddTransient<Core.ViewModels.PlaylistsViewModel>(sp =>
            new Core.ViewModels.PlaylistsViewModel(
                sp.GetRequiredService<IPlaylistManager>(),
                MainThread.BeginInvokeOnMainThread));
    }
}
