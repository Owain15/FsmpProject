namespace FSMP.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("playlistDetail", typeof(Pages.PlaylistDetailPage));
        Routing.RegisterRoute("customTheme", typeof(Pages.CustomThemePage));
        Routing.RegisterRoute("settingsLibrary", typeof(Pages.Settings.LibrarySettingsPage));
        Routing.RegisterRoute("settingsPlayback", typeof(Pages.Settings.PlaybackSettingsPage));
        Routing.RegisterRoute("settingsAppearance", typeof(Pages.Settings.AppearanceSettingsPage));
        Routing.RegisterRoute("settingsBehavior", typeof(Pages.Settings.BehaviorSettingsPage));
        Routing.RegisterRoute("settingsAbout", typeof(Pages.Settings.AboutSettingsPage));
    }
}
