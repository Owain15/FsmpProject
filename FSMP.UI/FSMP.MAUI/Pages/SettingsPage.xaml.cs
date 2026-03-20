namespace FSMP.MAUI.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private async void OnLibraryTapped(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("settingsLibrary");

    private async void OnPlaybackTapped(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("settingsPlayback");

    private async void OnAppearanceTapped(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("settingsAppearance");

    private async void OnBehaviorTapped(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("settingsBehavior");

    private async void OnAboutTapped(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("settingsAbout");
}
