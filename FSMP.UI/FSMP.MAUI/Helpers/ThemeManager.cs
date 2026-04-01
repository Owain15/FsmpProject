using FSMP.MAUI.Resources.Styles.Themes;

namespace FSMP.MAUI.Helpers;

public static class ThemeManager
{
    public static readonly IReadOnlyList<string> AvailableThemes = new[] { "Light", "Dark", "Light Blue" };

    private static ResourceDictionary? _currentThemeDictionary;

    /// <summary>
    /// Default color values for each theme key (Light theme defaults).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> LightDefaults = new Dictionary<string, string>
    {
        ["ThemeBackground"] = "#FFFFFF",
        ["ThemeText"] = "#1B1B1B",
        ["ThemePrimary"] = "#1A237E",
        ["ThemeSecondary"] = "#E8EAF6",
        ["ThemeCardBackground"] = "#F5F5F5",
        ["ThemeSurface"] = "#FFFFFF",
        ["ThemeTextSecondary"] = "#757575",
        ["ThemeBorder"] = "#E0E0E0",
        ["ThemeOverlay"] = "#80000000",
        ["ThemeOnOverlay"] = "#FFFFFF",
        ["ThemeChipBackground"] = "#E0E0E0",
        ["ThemeHighlight"] = "#E0E0FF",
        ["ThemeButtonBackground"] = "#1A237E",
        ["ThemeButtonText"] = "#FFFFFF",
        ["ThemeButtonDisabledBg"] = "#E0E0E0",
        ["ThemeButtonDisabledText"] = "#9E9E9E",
        ["ThemeShellBackground"] = "#FFFFFF",
        ["ThemeShellText"] = "#1B1B1B",
        ["ThemeShellTabBar"] = "#FFFFFF",
        ["ThemeShellTabSelected"] = "#1A237E",
        ["ThemeShellTabUnselected"] = "#757575",
        ["ThemeSliderTrack"] = "#1A237E",
        ["ThemeSliderTrackBg"] = "#E0E0E0",
        ["ThemeEntryText"] = "#1B1B1B",
        ["ThemeEntryPlaceholder"] = "#9E9E9E",
        ["ThemeError"] = "#D32F2F",
    };

    public static readonly IReadOnlyDictionary<string, string> DarkDefaults = new Dictionary<string, string>
    {
        ["ThemeBackground"] = "#121212",
        ["ThemeText"] = "#FFFFFF",
        ["ThemePrimary"] = "#E53935",
        ["ThemeSecondary"] = "#1E1E2E",
        ["ThemeCardBackground"] = "#1E1E1E",
        ["ThemeSurface"] = "#2C2C2C",
        ["ThemeTextSecondary"] = "#B0B0B0",
        ["ThemeBorder"] = "#3A3A3A",
        ["ThemeOverlay"] = "#80000000",
        ["ThemeOnOverlay"] = "#FFFFFF",
        ["ThemeChipBackground"] = "#5A3A3A",
        ["ThemeHighlight"] = "#6A3A3A",
        ["ThemeButtonBackground"] = "#E53935",
        ["ThemeButtonText"] = "#FFFFFF",
        ["ThemeButtonDisabledBg"] = "#404040",
        ["ThemeButtonDisabledText"] = "#757575",
        ["ThemeShellBackground"] = "#121212",
        ["ThemeShellText"] = "#FFFFFF",
        ["ThemeShellTabBar"] = "#1E1E1E",
        ["ThemeShellTabSelected"] = "#E53935",
        ["ThemeShellTabUnselected"] = "#B0B0B0",
        ["ThemeSliderTrack"] = "#E53935",
        ["ThemeSliderTrackBg"] = "#404040",
        ["ThemeEntryText"] = "#FFFFFF",
        ["ThemeEntryPlaceholder"] = "#757575",
        ["ThemeError"] = "#EF5350",
    };

    public static readonly IReadOnlyDictionary<string, string> LightBlueDefaults = new Dictionary<string, string>
    {
        ["ThemeBackground"] = "#E3F2FD",
        ["ThemeText"] = "#1B1B1B",
        ["ThemePrimary"] = "#1565C0",
        ["ThemeSecondary"] = "#BBDEFB",
        ["ThemeCardBackground"] = "#E1F5FE",
        ["ThemeSurface"] = "#F5F5F5",
        ["ThemeTextSecondary"] = "#546E7A",
        ["ThemeBorder"] = "#90CAF9",
        ["ThemeOverlay"] = "#80000000",
        ["ThemeOnOverlay"] = "#FFFFFF",
        ["ThemeChipBackground"] = "#BBDEFB",
        ["ThemeHighlight"] = "#90CAF9",
        ["ThemeButtonBackground"] = "#1565C0",
        ["ThemeButtonText"] = "#FFFFFF",
        ["ThemeButtonDisabledBg"] = "#B0BEC5",
        ["ThemeButtonDisabledText"] = "#78909C",
        ["ThemeShellBackground"] = "#E3F2FD",
        ["ThemeShellText"] = "#1B1B1B",
        ["ThemeShellTabBar"] = "#BBDEFB",
        ["ThemeShellTabSelected"] = "#1565C0",
        ["ThemeShellTabUnselected"] = "#546E7A",
        ["ThemeSliderTrack"] = "#1565C0",
        ["ThemeSliderTrackBg"] = "#90CAF9",
        ["ThemeEntryText"] = "#1B1B1B",
        ["ThemeEntryPlaceholder"] = "#78909C",
        ["ThemeError"] = "#D32F2F",
    };

    /// <summary>
    /// All theme color keys in display order.
    /// </summary>
    public static readonly IReadOnlyList<string> AllColorKeys = new List<string>(LightDefaults.Keys);

    public static void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        if (app is null) return;

        var newTheme = CreateThemeDictionary(themeName);
        if (newTheme is null) return;

        var mergedDictionaries = app.Resources.MergedDictionaries;

        if (_currentThemeDictionary is not null)
            mergedDictionaries.Remove(_currentThemeDictionary);

        mergedDictionaries.Add(newTheme);
        _currentThemeDictionary = newTheme;
    }

    /// <summary>
    /// Gets the default color dictionary for a given theme name.
    /// </summary>
    public static IReadOnlyDictionary<string, string> GetThemeDefaults(string themeName)
    {
        return themeName switch
        {
            "Dark" => DarkDefaults,
            "Light Blue" => LightBlueDefaults,
            _ => LightDefaults,
        };
    }

    private static ResourceDictionary? CreateThemeDictionary(string themeName)
    {
        return themeName switch
        {
            "Dark" => new DarkTheme(),
            "Light Blue" => new LightBlueTheme(),
            _ => new LightTheme()
        };
    }
}
