using FsmpDataAcsses;
using Microsoft.EntityFrameworkCore;

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
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
