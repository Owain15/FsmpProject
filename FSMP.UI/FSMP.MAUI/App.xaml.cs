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
        return new Window(new AppShell());
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
