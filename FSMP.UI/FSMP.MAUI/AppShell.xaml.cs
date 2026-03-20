namespace FSMP.MAUI;

public partial class AppShell : Shell
{
    // Base breakpoints (before adjustments)
    private const double BaseCompactWidthBreakpoint = 600;
    private const double BaseCompactHeightBreakpoint = 500;

    // Current effective breakpoints (extensible for text size etc.)
    public static double CompactWidthBreakpoint { get; private set; } = BaseCompactWidthBreakpoint;
    public static double CompactHeightBreakpoint { get; private set; } = BaseCompactHeightBreakpoint;

    // Estimated tab bar height saved in compact mode
    public const double TabBarHeight = 48;

    public static bool IsCompactMode { get; private set; }
    public static event Action? CompactModeChanged;

    private bool _subscribed;

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

        // Hide tab bar programmatically
        foreach (var item in Items)
        {
            if (item is TabBar tabBar)
            {
                foreach (var section in tabBar.Items)
                {
                    foreach (var content in section.Items)
                    {
                        Shell.SetTabBarIsVisible(content, false);
                    }
                }
            }
        }

        // Watch for Window to be assigned for compact mode detection
        PropertyChanged += OnShellPropertyChanged;
    }

    private void OnShellPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Window) && Window is not null && !_subscribed)
        {
            _subscribed = true;
            Window.SizeChanged += OnWindowSizeChanged;
            Dispatcher.Dispatch(() =>
            {
                if (Window is not null)
                    EvaluateCompactMode(Window.Width, Window.Height);
            });
        }
    }

    private void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        if (Window is not null)
            EvaluateCompactMode(Window.Width, Window.Height);
    }

    private void EvaluateCompactMode(double width, double height)
    {
        if (width <= 0 || height <= 0)
            return;

        // Desktop uses smaller breakpoints — only switch to hamburger at extreme sizes.
        // Phones/tablets use the standard breakpoints.
        double widthBp, heightBp;
        if (DeviceInfo.Idiom == DeviceIdiom.Desktop)
        {
            widthBp = CompactWidthBreakpoint * 0.65;   // ~390px
            heightBp = CompactHeightBreakpoint * 0.65;  // ~325px
        }
        else
        {
            widthBp = CompactWidthBreakpoint;
            heightBp = CompactHeightBreakpoint;
        }

        var compact = width < widthBp || height < heightBp;
        if (compact != IsCompactMode)
        {
            IsCompactMode = compact;
            CompactModeChanged?.Invoke();
        }
    }

    /// <summary>
    /// Recalculate breakpoints based on dynamic factors (e.g. text size).
    /// Call this when a factor changes, then re-evaluate compact mode.
    /// </summary>
    public static void UpdateBreakpoints(double widthAdjustment = 0, double heightAdjustment = 0)
    {
        CompactWidthBreakpoint = BaseCompactWidthBreakpoint + widthAdjustment;
        CompactHeightBreakpoint = BaseCompactHeightBreakpoint + heightAdjustment;
    }
}
