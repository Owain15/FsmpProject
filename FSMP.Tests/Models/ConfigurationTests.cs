using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.Models;

public class ConfigurationTests
{
    [Fact]
    public void Configuration_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var config = new Configuration();

        // Assert
        config.LibraryPaths.Should().NotBeNull().And.BeEmpty();
        config.DatabasePath.Should().BeEmpty();
        config.AutoScanOnStartup.Should().BeTrue();
        config.DefaultVolume.Should().Be(75);
        config.RememberLastPlayed.Should().BeTrue();
        config.LastPlayedTrackPath.Should().BeNull();
    }

    [Fact]
    public void Configuration_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.LibraryPaths = new List<string> { @"C:\Music", @"D:\Music" };
        config.DatabasePath = @"%AppData%\FSMP\fsmp.db";
        config.AutoScanOnStartup = false;
        config.DefaultVolume = 50;
        config.RememberLastPlayed = false;
        config.LastPlayedTrackPath = @"C:\Music\song.mp3";

        // Assert
        config.LibraryPaths.Should().HaveCount(2);
        config.LibraryPaths.Should().Contain(@"C:\Music");
        config.LibraryPaths.Should().Contain(@"D:\Music");
        config.DatabasePath.Should().Be(@"%AppData%\FSMP\fsmp.db");
        config.AutoScanOnStartup.Should().BeFalse();
        config.DefaultVolume.Should().Be(50);
        config.RememberLastPlayed.Should().BeFalse();
        config.LastPlayedTrackPath.Should().Be(@"C:\Music\song.mp3");
    }

    [Fact]
    public void Configuration_LibraryPaths_ShouldBeInitializedAsEmptyList()
    {
        // Arrange & Act
        var config = new Configuration();

        // Assert
        config.LibraryPaths.Should().NotBeNull();
        config.LibraryPaths.Should().BeEmpty();
        config.LibraryPaths.Should().BeAssignableTo<List<string>>();
    }

    [Fact]
    public void Configuration_LibraryPaths_ShouldAllowAddingPaths()
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.LibraryPaths.Add(@"C:\Users\Admin\Music");
        config.LibraryPaths.Add(@"D:\Shared Music");
        config.LibraryPaths.Add(@"\\NAS\Music");

        // Assert
        config.LibraryPaths.Should().HaveCount(3);
        config.LibraryPaths[0].Should().Be(@"C:\Users\Admin\Music");
        config.LibraryPaths[1].Should().Be(@"D:\Shared Music");
        config.LibraryPaths[2].Should().Be(@"\\NAS\Music");
    }

    [Fact]
    public void Configuration_LibraryPaths_ShouldAllowRemovingPaths()
    {
        // Arrange
        var config = new Configuration
        {
            LibraryPaths = new List<string> { @"C:\Music", @"D:\Music", @"E:\Music" }
        };

        // Act
        config.LibraryPaths.Remove(@"D:\Music");

        // Assert
        config.LibraryPaths.Should().HaveCount(2);
        config.LibraryPaths.Should().NotContain(@"D:\Music");
    }

    [Fact]
    public void Configuration_AutoScanOnStartup_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var config = new Configuration();

        // Assert
        config.AutoScanOnStartup.Should().BeTrue();
    }

    [Fact]
    public void Configuration_AutoScanOnStartup_ShouldBeSettableToFalse()
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.AutoScanOnStartup = false;

        // Assert
        config.AutoScanOnStartup.Should().BeFalse();
    }

    [Fact]
    public void Configuration_DefaultVolume_ShouldDefaultTo75()
    {
        // Arrange & Act
        var config = new Configuration();

        // Assert
        config.DefaultVolume.Should().Be(75);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void Configuration_DefaultVolume_ShouldAcceptValidValues(int volume)
    {
        // Arrange
        var config = new Configuration();

        // Act
        config.DefaultVolume = volume;

        // Assert
        config.DefaultVolume.Should().Be(volume);
    }

    [Fact]
    public void Configuration_RememberLastPlayed_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var config = new Configuration();

        // Assert
        config.RememberLastPlayed.Should().BeTrue();
    }

    [Fact]
    public void Configuration_LastPlayedTrackPath_ShouldBeNullable()
    {
        // Arrange
        var config = new Configuration
        {
            LastPlayedTrackPath = @"C:\Music\song.mp3"
        };

        // Act
        config.LastPlayedTrackPath = null;

        // Assert
        config.LastPlayedTrackPath.Should().BeNull();
    }

    [Fact]
    public void Configuration_LastPlayedTrackPath_ShouldStorePathCorrectly()
    {
        // Arrange
        var config = new Configuration();
        var trackPath = @"C:\Users\Admin\Music\Artist\Album\Track.mp3";

        // Act
        config.LastPlayedTrackPath = trackPath;

        // Assert
        config.LastPlayedTrackPath.Should().Be(trackPath);
    }

    [Fact]
    public void Configuration_DatabasePath_ShouldStoreEnvironmentVariablePath()
    {
        // Arrange
        var config = new Configuration();
        var dbPath = @"%AppData%\FSMP\fsmp.db";

        // Act
        config.DatabasePath = dbPath;

        // Assert
        config.DatabasePath.Should().Be(dbPath);
        config.DatabasePath.Should().Contain("%AppData%");
    }

    [Fact]
    public void Configuration_DatabasePath_ShouldStoreAbsolutePath()
    {
        // Arrange
        var config = new Configuration();
        var dbPath = @"C:\ProgramData\FSMP\database.db";

        // Act
        config.DatabasePath = dbPath;

        // Assert
        config.DatabasePath.Should().Be(dbPath);
    }
}
