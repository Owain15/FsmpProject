namespace FSMP.MAUI.Components;

public partial class NavMenuOverlay : ContentView
{
    private readonly Dictionary<string, Button> _navButtons;

    public string CurrentRoute { get; set; } = string.Empty;

    public NavMenuOverlay()
    {
        InitializeComponent();
        _navButtons = new Dictionary<string, Button>
        {
            ["NowPlaying"] = BtnNowPlaying,
            ["Library"] = BtnLibrary,
            ["Playlists"] = BtnPlaylists,
            ["Settings"] = BtnSettings,
        };
    }

    public void Toggle()
    {
        IsVisible = !IsVisible;
        if (IsVisible)
            HighlightCurrent();
    }

    private void HighlightCurrent()
    {
        foreach (var (route, btn) in _navButtons)
        {
            btn.BackgroundColor = route == CurrentRoute
                ? (Color)(Application.Current!.Resources["ThemeHighlight"])
                : Colors.Transparent;
        }
    }

    private async void OnNavItemClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            IsVisible = false;
            await Shell.Current.GoToAsync($"//{btn.ClassId}");
        }
    }

    private void OnBackdropTapped(object? sender, EventArgs e)
    {
        IsVisible = false;
    }
}
