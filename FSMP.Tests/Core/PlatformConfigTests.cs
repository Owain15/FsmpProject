using System.Text.Json;
using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.Core;

public class PlatformConfigTests
{
    [Fact]
    public void Configuration_ShouldStoreAndroidStylePaths()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string>
            {
                "/data/data/com.fsmp.app/files/music",
                "/storage/emulated/0/Music"
            },
            DatabasePath = "/data/data/com.fsmp.app/files/fsmp.db"
        };

        config.LibraryPaths.Should().HaveCount(2);
        config.LibraryPaths[0].Should().Be("/data/data/com.fsmp.app/files/music");
        config.DatabasePath.Should().Be("/data/data/com.fsmp.app/files/fsmp.db");
    }

    [Fact]
    public void Configuration_ShouldStoreWindowsStylePaths()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string> { @"C:\Users\Admin\Music", @"D:\Music" },
            DatabasePath = @"C:\Users\Admin\AppData\Roaming\FSMP\fsmp.db"
        };

        config.LibraryPaths.Should().HaveCount(2);
        config.DatabasePath.Should().Contain(@"\");
    }

    [Fact]
    public void Configuration_ShouldSerializeAndDeserialize_WithAndroidPaths()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string>
            {
                "/storage/emulated/0/Music",
                "/storage/sdcard1/Music"
            },
            DatabasePath = "/data/data/com.fsmp.app/files/fsmp.db",
            AutoScanOnStartup = true,
            DefaultVolume = 80
        };

        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<Configuration>(json);

        deserialized.Should().NotBeNull();
        deserialized!.LibraryPaths.Should().HaveCount(2);
        deserialized.LibraryPaths[0].Should().Be("/storage/emulated/0/Music");
        deserialized.DatabasePath.Should().Be("/data/data/com.fsmp.app/files/fsmp.db");
        deserialized.DefaultVolume.Should().Be(80);
    }

    [Fact]
    public void Configuration_ShouldSerializeAndDeserialize_WithMixedPaths()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string>
            {
                @"C:\Music",
                "/storage/emulated/0/Music"
            },
            DatabasePath = "/data/user/0/com.fsmp/files/fsmp.db"
        };

        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<Configuration>(json);

        deserialized.Should().NotBeNull();
        deserialized!.LibraryPaths.Should().Contain(@"C:\Music");
        deserialized.LibraryPaths.Should().Contain("/storage/emulated/0/Music");
    }

    [Fact]
    public void Configuration_ThemeAndCustomTheme_ShouldRoundTrip()
    {
        var config = new Configuration
        {
            Theme = "Custom",
            CustomThemeColors = new Dictionary<string, string>
            {
                ["ThemeBackground"] = "#121212",
                ["ThemeText"] = "#FFFFFF"
            },
            SavedCustomThemes = new List<NamedCustomTheme>
            {
                new() { Name = "My Theme", Colors = new Dictionary<string, string> { ["ThemeBackground"] = "#000000" } }
            }
        };

        var json = JsonSerializer.Serialize(config);
        var deserialized = JsonSerializer.Deserialize<Configuration>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Theme.Should().Be("Custom");
        deserialized.CustomThemeColors.Should().ContainKey("ThemeBackground");
        deserialized.SavedCustomThemes.Should().HaveCount(1);
        deserialized.SavedCustomThemes[0].Name.Should().Be("My Theme");
    }

    [Fact]
    public void NamedCustomTheme_ShouldInitializeWithDefaults()
    {
        var theme = new NamedCustomTheme();

        theme.Name.Should().BeEmpty();
        theme.Colors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Configuration_AllowUnsaveFromTagList_ShouldDefaultToFalse()
    {
        var config = new Configuration();

        config.AllowUnsaveFromTagList.Should().BeFalse();
    }

    [Fact]
    public void Configuration_SavedCustomThemes_ShouldDefaultToEmptyList()
    {
        var config = new Configuration();

        config.SavedCustomThemes.Should().NotBeNull().And.BeEmpty();
    }
}
