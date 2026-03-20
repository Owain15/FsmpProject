namespace FSMP.MAUI.Components;

public partial class TopNavBar : ContentView
{
    private static readonly Dictionary<string, string> RouteTitles = new()
    {
        ["NowPlaying"] = "Now Playing",
        ["Library"] = "Library",
        ["Playlists"] = "Playlists",
        ["Settings"] = "Settings",
    };

    private readonly Dictionary<string, Label> _navLabels;
    private readonly Dictionary<string, BoxView> _indicators;

    public event EventHandler? HamburgerClicked;

    public static readonly BindableProperty CurrentRouteProperty =
        BindableProperty.Create(nameof(CurrentRoute), typeof(string), typeof(TopNavBar), "NowPlaying",
            propertyChanged: OnCurrentRouteChanged);

    public string CurrentRoute
    {
        get => (string)GetValue(CurrentRouteProperty);
        set => SetValue(CurrentRouteProperty, value);
    }

    public TopNavBar()
    {
        InitializeComponent();
        _navLabels = new Dictionary<string, Label>
        {
            ["NowPlaying"] = NavNowPlaying,
            ["Library"] = NavLibrary,
            ["Playlists"] = NavPlaylists,
            ["Settings"] = NavSettings,
        };
        _indicators = new Dictionary<string, BoxView>
        {
            ["NowPlaying"] = IndicatorNowPlaying,
            ["Library"] = IndicatorLibrary,
            ["Playlists"] = IndicatorPlaylists,
            ["Settings"] = IndicatorSettings,
        };

        AppShell.CompactModeChanged += OnCompactModeChanged;
        UpdateNavVisibility(AppShell.IsCompactMode);
        HighlightActiveNav();
    }

    private static void OnCurrentRouteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is TopNavBar nav)
        {
            nav.HighlightActiveNav();
            if (newValue is string route && RouteTitles.TryGetValue(route, out var title))
                nav.PageTitleLabel.Text = title;
        }
    }

    private void HighlightActiveNav()
    {
        var highlightColor = Colors.DodgerBlue;
        if (Application.Current?.Resources.TryGetValue("ThemeHighlight", out var res) == true && res is Color h)
            highlightColor = h;

        // Get default text color from theme
        Color textColor = Colors.White;
        if (Application.Current?.Resources.TryGetValue("ThemeText", out var textRes) == true && textRes is Color tc)
            textColor = tc;

        foreach (var (route, label) in _navLabels)
        {
            var isActive = route == CurrentRoute;
            label.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
            label.TextColor = isActive ? highlightColor : textColor;
        }

        foreach (var (route, indicator) in _indicators)
        {
            indicator.Color = route == CurrentRoute ? highlightColor : Colors.Transparent;
        }
    }

    private async void OnNavTabTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is string route)
            await Shell.Current.GoToAsync($"//{route}");
    }

    private void OnHamburgerClicked(object? sender, EventArgs e)
    {
        HamburgerClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnCompactModeChanged()
    {
        MainThread.BeginInvokeOnMainThread(() => UpdateNavVisibility(AppShell.IsCompactMode));
    }

    private void UpdateNavVisibility(bool compact)
    {
        HamburgerBtn.IsVisible = compact;
        NavButtonRow.IsVisible = !compact;
    }
}
