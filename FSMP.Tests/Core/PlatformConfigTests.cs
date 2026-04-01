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
    public void Configuration_AllowUnsaveFromTagList_ShouldDefaultToFalse()
    {
        var config = new Configuration();

        config.AllowUnsaveFromTagList.Should().BeFalse();
    }

}
