using FSMP.Core;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Audio;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FsmpConsole;

/// <summary>
/// Encapsulates application startup: config loading, DB init, DI wiring, and menu launch.
/// Extracted from Program.cs for testability.
/// </summary>
public class AppStartup
{
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly string? _configPathOverride;
    private readonly string? _dbPathOverride;
    private readonly Action? _onClear;

    public AppStartup(TextReader input, TextWriter output, string? configPathOverride = null, string? dbPathOverride = null, Action? onClear = null)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _configPathOverride = configPathOverride;
        _dbPathOverride = dbPathOverride;
        _onClear = onClear;
    }

    /// <summary>
    /// Resolves the config file path (%AppData%\FSMP\config.json by default).
    /// </summary>
    public string GetConfigPath()
    {
        if (_configPathOverride != null)
            return _configPathOverride;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "FSMP", "config.json");
    }

    /// <summary>
    /// Resolves the database path from config or override.
    /// </summary>
    public string GetDatabasePath(Configuration? config)
    {
        if (_dbPathOverride != null)
            return _dbPathOverride;

        if (config != null && !string.IsNullOrEmpty(config.DatabasePath))
            return config.DatabasePath;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "FSMP", "fsmp.db");
    }

    /// <summary>
    /// Runs the full application startup sequence and menu loop.
    /// </summary>
    public async Task RunAsync()
    {
        _output.WriteLine("\nFSMP - File System Music Player\n\n");

        // 1. Load configuration
        _output.WriteLine("Getting Config...");
        var configPath = GetConfigPath();
        var configService = new ConfigurationService(configPath);
        var config = await configService.LoadConfigurationAsync();
        _output.WriteLine($"Config loaded from: {configPath}");

        // 2. Initialize database
        _output.WriteLine($"Getting Data Source...");
        var dbPath = GetDatabasePath(config);
        var dbDir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dbDir))
            Directory.CreateDirectory(dbDir);

        var optionsBuilder = new DbContextOptionsBuilder<FsmpDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath};Pooling=False");
        using var context = new FsmpDbContext(optionsBuilder.Options);

        await context.Database.MigrateAsync();
        _output.WriteLine($"Database ready at: {dbPath}");

        // 3. Wire up services
        using var unitOfWork = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(unitOfWork, metadataService);

        using var services = new ServiceCollection()
            .AddSingleton<IAudioPlayerFactory, LibVlcAudioPlayerFactory>()
            .AddSingleton<IAudioService>(sp =>
                new AudioService(sp.GetRequiredService<IAudioPlayerFactory>(), config.DefaultVolume / 100f))
            .BuildServiceProvider();

        var audioService = services.GetRequiredService<IAudioService>();
        audioService.VolumeChanged += async (_, vol) =>
        {
            config.DefaultVolume = (int)(vol * 100);
            await configService.SaveConfigurationAsync(config);
        };

        // 3b. Eagerly initialize audio engine
        var factory = services.GetRequiredService<IAudioPlayerFactory>();
        _output.Write("Initializing audio engine...");
        if (await factory.InitializeAsync())
        {
            _output.WriteLine(" ready.");
        }
        else
        {
            var errorMsg = (factory as LibVlcAudioPlayerFactory)?.InitializationError ?? "Unknown error";
            _output.WriteLine($" failed: {errorMsg}. Playback will not be available.");
        }

        // 4. Auto-scan if enabled
        if (config.AutoScanOnStartup && config.LibraryPaths.Count > 0)
        {
            _output.WriteLine("Auto-scanning libraries...");
            var result = await scanService.ScanAllLibrariesAsync(config.LibraryPaths);
            _output.WriteLine($"Scan complete: {result.TracksAdded} added, {result.TracksUpdated} updated, {result.TracksRemoved} removed");
        }

        // 5. Create orchestration services
        var activePlaylist = new ActivePlaylistService();
        var playlistService = new PlaylistService(unitOfWork);

        IPlaybackController playback = new PlaybackController(audioService, activePlaylist, unitOfWork.Tracks);
        ILibraryBrowser browser = new LibraryBrowser(unitOfWork.Artists, unitOfWork.Albums, unitOfWork.Tracks);
        IPlaylistManager playlists = new PlaylistManager(playlistService, activePlaylist);
        ILibraryManager library = new LibraryManager(configService, scanService);

        // 6. Clear startup messages and launch menu
        _onClear?.Invoke();
        var menu = new MenuSystem(playback, playlists, library, browser, _input, _output, _onClear);
        await menu.RunAsync();
    }
}
