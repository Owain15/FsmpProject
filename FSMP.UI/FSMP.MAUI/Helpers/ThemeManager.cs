using FSMP.MAUI.Resources.Styles.Themes;

namespace FSMP.MAUI.Helpers;

public static class ThemeManager
{
    public static readonly IReadOnlyList<string> AvailableThemes = new[] { "Light", "Dark", "Light Blue" };

    private static ResourceDictionary? _currentThemeDictionary;

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
