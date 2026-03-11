using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Services;
using FSMP.MAUI.Pages;
using FSMP.MAUI.ViewModels;
#if WINDOWS
using FSMP.Platform.Windows.Audio;
#elif ANDROID
using FSMP.Platform.Android.Audio;
#endif
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

    static string GetAppDataBase()
    {
#if ANDROID
        return FileSystem.AppDataDirectory;
#else
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FSMP");
#endif
    }

    static void RegisterServices(IServiceCollection services)
    {
        // Database path — platform-aware
        var appDataBase = GetAppDataBase();
        var dbPath = Path.Combine(appDataBase, "fsmp.db");
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

        // Queue state persistence
        var queueStatePath = Path.Combine(appDataBase, "queue-state.json");
        services.AddSingleton<IQueueStateRepository>(_ => new FsmpDataAcsses.Repositories.JsonQueueStateRepository(queueStatePath));

        // Config service
        var configPath = Path.Combine(appDataBase, "config.json");
        services.AddSingleton(_ => new ConfigurationService(configPath));

        // Audio
#if WINDOWS
        services.AddSingleton<IAudioPlayerFactory, LibVlcAudioPlayerFactory>();
#elif ANDROID
        services.AddSingleton<IAudioPlayerFactory, LibVlcAndroidAudioPlayerFactory>();
#endif
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
        services.AddScoped<ITagRepository>(sp => sp.GetRequiredService<UnitOfWork>().Tags);

        // Tag service
        services.AddScoped<ITagService>(sp => new TagService(
            sp.GetRequiredService<ITagRepository>(),
            sp.GetRequiredService<ITrackRepository>(),
            sp.GetRequiredService<IAlbumRepository>(),
            sp.GetRequiredService<IArtistRepository>(),
            sp.GetRequiredService<UnitOfWork>().SaveAsync));

        // Orchestration layer
        services.AddScoped<IPlaybackController, PlaybackController>();
        services.AddScoped<ILibraryBrowser>(sp => new LibraryBrowser(
            sp.GetRequiredService<IArtistRepository>(),
            sp.GetRequiredService<IAlbumRepository>(),
            sp.GetRequiredService<ITrackRepository>(),
            sp.GetRequiredService<ITagRepository>()));
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
