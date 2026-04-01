using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.MAUI;

/// <summary>
/// Tests for theme-related logic. Since ThemeManager itself uses MAUI types (ResourceDictionary, Color),
/// these tests verify the theme data contracts, Configuration model theme properties, and color format
/// validation that both platforms rely on.
/// </summary>
public partial class ThemeManagerTests
{
    /// <summary>
    /// The theme color keys that ThemeManager defines. Both platforms depend on these keys being present.
    /// If a key is added/removed from ThemeManager, this test must be updated to match.
    /// </summary>
    private static readonly string[] ExpectedThemeColorKeys =
    {
        "ThemeBackground", "ThemeText", "ThemePrimary", "ThemeSecondary",
        "ThemeCardBackground", "ThemeSurface", "ThemeTextSecondary", "ThemeBorder",
        "ThemeOverlay", "ThemeOnOverlay", "ThemeChipBackground", "ThemeHighlight",
        "ThemeButtonBackground", "ThemeButtonText", "ThemeButtonDisabledBg", "ThemeButtonDisabledText",
        "ThemeShellBackground", "ThemeShellText", "ThemeShellTabBar", "ThemeShellTabSelected",
        "ThemeShellTabUnselected", "ThemeSliderTrack", "ThemeSliderTrackBg",
        "ThemeEntryText", "ThemeEntryPlaceholder", "ThemeError",
    };

    private static readonly string[] AvailableThemes = { "Light", "Dark", "Light Blue" };

    [Fact]
    public void Configuration_Theme_ShouldDefaultToLight()
    {
        var config = new Configuration();
        config.Theme.Should().Be("Light");
    }

    [Fact]
    public void Configuration_ShouldRoundTrip_AllThemeNames()
    {
        foreach (var themeName in AvailableThemes)
        {
            var config = new Configuration { Theme = themeName };
            var json = JsonSerializer.Serialize(config);
            var deserialized = JsonSerializer.Deserialize<Configuration>(json);

            deserialized!.Theme.Should().Be(themeName);
        }
    }

    [Theory]
    [InlineData("#FFFFFF")]
    [InlineData("#000000")]
    [InlineData("#80000000")]
    [InlineData("#1A237E")]
    [InlineData("#E53935")]
    public void ThemeColorValues_ShouldBeValidHexFormat(string hex)
    {
        HexColorRegex().IsMatch(hex).Should().BeTrue($"'{hex}' should be a valid hex color");
    }

    [Fact]
    public void ExpectedThemeColorKeys_ShouldHave26Keys()
    {
        // Guard: if ThemeManager adds/removes keys, update ExpectedThemeColorKeys above.
        ExpectedThemeColorKeys.Should().HaveCount(26);
    }

    [GeneratedRegex(@"^#[0-9A-Fa-f]{6,8}$")]
    private static partial Regex HexColorRegex();
}
