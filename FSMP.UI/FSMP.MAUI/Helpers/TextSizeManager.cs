namespace FSMP.MAUI.Helpers;

/// <summary>
/// Manages runtime text size changes using the same DynamicResource dictionary-swap
/// pattern as ThemeManager. Defines semantic font-size keys that scale across four presets.
/// </summary>
public static class TextSizeManager
{
    public static readonly IReadOnlyList<string> AvailableTextSizes = new[] { "Small", "Medium", "Large", "Extra Large" };

    // Semantic key → double value per preset
    // Key        | Small | Medium | Large | Extra Large
    // TextSizeXS |   9   |   11   |   13  |     15
    // TextSizeSmall | 11 |   12   |   14  |     16
    // TextSizeBody  | 12 |   14   |   16  |     18
    // TextSizeSubtitle | 13 | 16  |   18  |     21
    // TextSizeTitle | 16 |   20   |   24  |     28
    // TextSizeHeading | 18 | 22   |   26  |     30
    // TextSizeHero  | 20 |   24   |   28  |     32

    private static readonly Dictionary<string, double> SmallSizes = new()
    {
        ["TextSizeXS"] = 9,
        ["TextSizeSmall"] = 11,
        ["TextSizeBody"] = 12,
        ["TextSizeSubtitle"] = 13,
        ["TextSizeTitle"] = 16,
        ["TextSizeHeading"] = 18,
        ["TextSizeHero"] = 20,
    };

    private static readonly Dictionary<string, double> MediumSizes = new()
    {
        ["TextSizeXS"] = 11,
        ["TextSizeSmall"] = 12,
        ["TextSizeBody"] = 14,
        ["TextSizeSubtitle"] = 16,
        ["TextSizeTitle"] = 20,
        ["TextSizeHeading"] = 22,
        ["TextSizeHero"] = 24,
    };

    private static readonly Dictionary<string, double> LargeSizes = new()
    {
        ["TextSizeXS"] = 13,
        ["TextSizeSmall"] = 14,
        ["TextSizeBody"] = 16,
        ["TextSizeSubtitle"] = 18,
        ["TextSizeTitle"] = 24,
        ["TextSizeHeading"] = 26,
        ["TextSizeHero"] = 28,
    };

    private static readonly Dictionary<string, double> ExtraLargeSizes = new()
    {
        ["TextSizeXS"] = 15,
        ["TextSizeSmall"] = 16,
        ["TextSizeBody"] = 18,
        ["TextSizeSubtitle"] = 21,
        ["TextSizeTitle"] = 28,
        ["TextSizeHeading"] = 30,
        ["TextSizeHero"] = 32,
    };

    /// <summary>All semantic text size keys.</summary>
    public static readonly IReadOnlyList<string> AllKeys = new[]
    {
        "TextSizeXS", "TextSizeSmall", "TextSizeBody", "TextSizeSubtitle",
        "TextSizeTitle", "TextSizeHeading", "TextSizeHero"
    };

    private static ResourceDictionary? _currentTextSizeDictionary;

    /// <summary>
    /// Applies a text size preset by swapping a ResourceDictionary into the app's merged dictionaries.
    /// </summary>
    public static void ApplyTextSize(string sizeName)
    {
        var app = Application.Current;
        if (app is null) return;

        var sizes = GetSizeDictionary(sizeName);
        var dict = new ResourceDictionary();
        foreach (var kvp in sizes)
            dict[kvp.Key] = kvp.Value;

        var merged = app.Resources.MergedDictionaries;
        if (_currentTextSizeDictionary is not null)
            merged.Remove(_currentTextSizeDictionary);

        merged.Add(dict);
        _currentTextSizeDictionary = dict;
    }

    /// <summary>
    /// Gets the raw size values for a given preset name. Useful for testing.
    /// </summary>
    public static Dictionary<string, double> GetSizeDictionary(string sizeName)
    {
        return sizeName switch
        {
            "Small" => SmallSizes,
            "Large" => LargeSizes,
            "Extra Large" => ExtraLargeSizes,
            _ => MediumSizes,
        };
    }
}
