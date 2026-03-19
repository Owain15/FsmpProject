namespace FSMP.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("playlistDetail", typeof(Pages.PlaylistDetailPage));
        Routing.RegisterRoute("customTheme", typeof(Pages.CustomThemePage));
    }
}
