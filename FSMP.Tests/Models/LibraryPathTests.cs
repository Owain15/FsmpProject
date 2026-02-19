using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.Models;

public class LibraryPathTests
{
    [Fact]
    public void LibraryPath_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var libraryPath = new LibraryPath();

        // Assert
        libraryPath.LibraryPathId.Should().Be(0);
        libraryPath.Path.Should().BeEmpty();
        libraryPath.IsActive.Should().BeTrue(); // Default is active
        libraryPath.AddedAt.Should().Be(default);
        libraryPath.LastScannedAt.Should().BeNull();
        libraryPath.TrackCount.Should().Be(0);
    }

    [Fact]
    public void LibraryPath_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var libraryPath = new LibraryPath();
        var addedAt = DateTime.Now;
        var lastScannedAt = addedAt.AddHours(1);

        // Act
        libraryPath.LibraryPathId = 1;
        libraryPath.Path = @"C:\Users\Admin\Music";
        libraryPath.IsActive = true;
        libraryPath.AddedAt = addedAt;
        libraryPath.LastScannedAt = lastScannedAt;
        libraryPath.TrackCount = 150;

        // Assert
        libraryPath.LibraryPathId.Should().Be(1);
        libraryPath.Path.Should().Be(@"C:\Users\Admin\Music");
        libraryPath.IsActive.Should().BeTrue();
        libraryPath.AddedAt.Should().Be(addedAt);
        libraryPath.LastScannedAt.Should().Be(lastScannedAt);
        libraryPath.TrackCount.Should().Be(150);
    }

    [Fact]
    public void LibraryPath_LastScannedAt_ShouldBeNullable()
    {
        // Arrange - newly added library hasn't been scanned yet
        var libraryPath = new LibraryPath
        {
            Path = @"D:\Music",
            AddedAt = DateTime.Now
        };

        // Assert - default is null
        libraryPath.LastScannedAt.Should().BeNull();

        // Act - set a scan time
        var scanTime = DateTime.Now;
        libraryPath.LastScannedAt = scanTime;

        // Assert
        libraryPath.LastScannedAt.Should().Be(scanTime);

        // Act - clear it
        libraryPath.LastScannedAt = null;

        // Assert
        libraryPath.LastScannedAt.Should().BeNull();
    }

    [Fact]
    public void LibraryPath_IsActive_ShouldDefaultToTrue()
    {
        // Arrange & Act
        var libraryPath = new LibraryPath();

        // Assert
        libraryPath.IsActive.Should().BeTrue();
    }

    [Fact]
    public void LibraryPath_IsActive_ShouldBeSettableToFalse()
    {
        // Arrange
        var libraryPath = new LibraryPath();

        // Act
        libraryPath.IsActive = false;

        // Assert
        libraryPath.IsActive.Should().BeFalse();
    }

    [Fact]
    public void LibraryPath_TrackCount_ShouldStoreCachedCount()
    {
        // Arrange
        var libraryPath = new LibraryPath();

        // Act
        libraryPath.TrackCount = 500;

        // Assert
        libraryPath.TrackCount.Should().Be(500);
    }

    [Fact]
    public void LibraryPath_TrackCount_ShouldAllowZero()
    {
        // Arrange - empty library
        var libraryPath = new LibraryPath
        {
            Path = @"E:\EmptyMusicFolder",
            TrackCount = 0
        };

        // Assert
        libraryPath.TrackCount.Should().Be(0);
    }

    [Fact]
    public void LibraryPath_Path_ShouldStoreWindowsPathCorrectly()
    {
        // Arrange
        var libraryPath = new LibraryPath();
        var windowsPath = @"C:\Users\Test User\Music\My Collection";

        // Act
        libraryPath.Path = windowsPath;

        // Assert
        libraryPath.Path.Should().Be(windowsPath);
        libraryPath.Path.Should().Contain("Test User");
    }

    [Fact]
    public void LibraryPath_Path_ShouldStoreNetworkPathCorrectly()
    {
        // Arrange
        var libraryPath = new LibraryPath();
        var networkPath = @"\\NAS\Music\Shared";

        // Act
        libraryPath.Path = networkPath;

        // Assert
        libraryPath.Path.Should().Be(networkPath);
        libraryPath.Path.Should().StartWith(@"\\");
    }

    [Fact]
    public void LibraryPath_AddedAt_ShouldStoreDateTimeCorrectly()
    {
        // Arrange
        var libraryPath = new LibraryPath();
        var addedAt = new DateTime(2024, 1, 15, 10, 30, 0);

        // Act
        libraryPath.AddedAt = addedAt;

        // Assert
        libraryPath.AddedAt.Should().Be(addedAt);
        libraryPath.AddedAt.Year.Should().Be(2024);
    }
}
