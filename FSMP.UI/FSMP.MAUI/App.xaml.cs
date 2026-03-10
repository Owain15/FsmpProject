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

        MainThread.BeginInvokeOnMainThread(async () => await InitializeServicesAsync());

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
        try
        {
            UpdateStatus("Migrating database...");
            await Task.Run(() =>
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<FsmpDbContext>();
                context.Database.Migrate();
            });
            Log("Database migration done");

            UpdateStatus("Restoring session...");
            try
            {
                var queueStateRepo = Services.GetRequiredService<IQueueStateRepository>();
                var savedState = await Task.Run(() => queueStateRepo.LoadAsync());
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
            InitializationComplete?.Invoke();
        }
        catch (Exception ex)
        {
            UpdateStatus($"Error: {ex.Message}");
            Log($"Init failed: {ex}");
            // App still usable — pages handle missing data gracefully
            IsInitialized = true;
            InitializationComplete?.Invoke();
        }
    }

    private static void UpdateStatus(string message)
    {
        InitStatusMessage = message;
        Log($"Init status: {message}");
        InitializationStatusChanged?.Invoke();
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
