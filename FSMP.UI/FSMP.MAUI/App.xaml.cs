using FSMP.Core;
using FSMP.Core.Interfaces;
using FsmpDataAcsses;
using Microsoft.EntityFrameworkCore;

namespace FSMP.MAUI;

public partial class App : Application
{
    private static readonly string LogPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FSMP", "maui-debug.log");

    public static IServiceProvider Services { get; private set; } = null!;

    public static bool IsInitialized { get; private set; }
    public static string InitStatusMessage { get; private set; } = "Starting...";
    public static event Action? InitializationStatusChanged;
    public static event Action? InitializationComplete;
    private static int _initStarted;

    public App(IServiceProvider services)
    {
        try
        {
            Services = services;
            Log("App constructor starting");
            InitializeComponent();
            Log("InitializeComponent done");
        }
        catch (Exception ex)
        {
            Log($"CRASH in App constructor: {ex}");
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Log("CreateWindow called");
        var window = new Window(new AppShell());

#if WINDOWS
        // Dismiss the native Win32 splash now that the MAUI window is ready
        window.Created += (_, _) =>
        {
            FSMP.MAUI.WinUI.NativeSplash.Close();
            Log("Native splash dismissed");
        };
#endif

        // Fire-and-forget on background thread — don't block the main thread rendering pipeline
        _ = Task.Run(InitializeServicesAsync);

        window.Destroying += (_, _) =>
        {
            try
            {
                var activePlaylist = Services.GetRequiredService<ActivePlaylistService>();
                var repo = Services.GetRequiredService<IQueueStateRepository>();
                Task.Run(() => repo.SaveAsync(activePlaylist.GetState())).GetAwaiter().GetResult();
                Log("Session saved");
            }
            catch (Exception ex)
            {
                Log($"Failed to save session: {ex.Message}");
            }
        };
        return window;
    }

    private async Task InitializeServicesAsync()
    {
        // Guard against duplicate calls (MAUI can construct App twice)
        if (Interlocked.Exchange(ref _initStarted, 1) == 1) return;

        try
        {
            UpdateStatus("Initializing audio engine...");
            var audioFactory = Services.GetRequiredService<IAudioPlayerFactory>();
            await audioFactory.InitializeAsync();

            UpdateStatus("Migrating database...");

            // Clear stale SQLite lock files that persist after force-kill
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FSMP", "fsmp.db");
            foreach (var ext in new[] { "-wal", "-shm" })
            {
                var lockFile = dbPath + ext;
                if (File.Exists(lockFile))
                {
                    Log($"Removing stale lock file: {lockFile}");
                    try { File.Delete(lockFile); } catch (Exception ex) { Log($"Could not delete {lockFile}: {ex.Message}"); }
                }
            }

            // Run migration with a 15-second timeout to avoid hanging indefinitely
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            try
            {
                cts.Token.Register(() => Log("Migration timeout triggered — cancelling"));
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FsmpDbContext>();
                await Task.Run(() => context.Database.Migrate(), cts.Token);
                Log("Database migration done");
            }
            catch (OperationCanceledException)
            {
                Log("Database migration timed out after 15 seconds");
                UpdateStatus("Migration timed out — continuing without database");
            }

            UpdateStatus("Restoring session...");
            try
            {
                var queueStateRepo = Services.GetRequiredService<IQueueStateRepository>();
                var savedState = await queueStateRepo.LoadAsync();
                if (savedState != null)
                {
                    var activePlaylist = Services.GetRequiredService<ActivePlaylistService>();
                    activePlaylist.RestoreState(savedState);
                    Log($"Session restored: {savedState.PlayOrder.Count} tracks, index {savedState.CurrentIndex}");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to restore session (non-fatal): {ex.Message}");
            }

            UpdateStatus("Ready");
            IsInitialized = true;
            MainThread.BeginInvokeOnMainThread(() => InitializationComplete?.Invoke());
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            Log($"Init failed: {ex}");
            // App still usable — pages handle missing data gracefully
            IsInitialized = true;
            MainThread.BeginInvokeOnMainThread(() => InitializationComplete?.Invoke());
        }
    }

    private static void UpdateStatus(string message)
    {
        InitStatusMessage = message;
        Log($"Init status: {message}");
        MainThread.BeginInvokeOnMainThread(() => InitializationStatusChanged?.Invoke());
    }

    internal static void Log(string message)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch { }
    }
}
