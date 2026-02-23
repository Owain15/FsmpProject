using FsmpDataAcsses;

namespace FSMP.MAUI;

public partial class App : Application
{
    public App(IServiceProvider services)
    {
        InitializeComponent();

        // Apply EF migrations on startup
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FsmpDbContext>();
        context.Database.Migrate();

        MainPage = new AppShell();
    }
}
