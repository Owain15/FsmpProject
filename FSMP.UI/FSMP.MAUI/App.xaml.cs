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

    public App(IServiceProvider services)
    {
        try
        {
            Services = services;
            Log("App constructor starting");
            InitializeComponent();
            Log("InitializeComponent done");

            // Apply EF migrations on startup
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FsmpDbContext>();
            context.Database.Migrate();
            Log("Database migration done");

            // Restore previous session queue
            try
            {
                var queueStateRepo = services.GetRequiredService<IQueueStateRepository>();
                var savedState = Task.Run(() => queueStateRepo.LoadAsync()).GetAwaiter().GetResult();
                if (savedState != null)
                {
                    var activePlaylist = services.GetRequiredService<ActivePlaylistService>();
                    activePlaylist.RestoreState(savedState);
                    Log($"Session restored: {savedState.PlayOrder.Count} tracks, index {savedState.CurrentIndex}");
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to restore session (non-fatal): {ex.Message}");
            }
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
