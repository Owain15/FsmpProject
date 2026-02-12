using FluentAssertions;
using FsmpLibrary.Models;

namespace FSMP.Tests.Models;

public class PlaylistTests
{
    [Fact]
    public void Playlist_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var playlist = new Playlist();

        // Assert
        playlist.PlaylistId.Should().Be(0);
        playlist.Name.Should().BeEmpty();
        playlist.Description.Should().BeNull();
        playlist.CreatedAt.Should().Be(default);
        playlist.UpdatedAt.Should().Be(default);
        playlist.PlaylistTracks.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Playlist_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var playlist = new Playlist();
        var testDate = DateTime.Now;

        // Act
        playlist.PlaylistId = 1;
        playlist.Name = "My Favorites";
        playlist.Description = "A collection of my favorite tracks";
        playlist.CreatedAt = testDate;
        playlist.UpdatedAt = testDate;

        // Assert
        playlist.PlaylistId.Should().Be(1);
        playlist.Name.Should().Be("My Favorites");
        playlist.Description.Should().Be("A collection of my favorite tracks");
        playlist.CreatedAt.Should().Be(testDate);
        playlist.UpdatedAt.Should().Be(testDate);
    }

    [Fact]
    public void Playlist_Description_ShouldBeNullable()
    {
        // Arrange
        var playlist = new Playlist { Description = "Some description" };

        // Act
        playlist.Description = null;

        // Assert
        playlist.Description.Should().BeNull();
    }

    [Fact]
    public void Playlist_PlaylistTracks_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var playlist = new Playlist();

        // Assert
        playlist.PlaylistTracks.Should().NotBeNull();
        playlist.PlaylistTracks.Should().BeEmpty();
        playlist.PlaylistTracks.Should().BeAssignableTo<ICollection<PlaylistTrack>>();
    }

    [Fact]
    public void Playlist_PlaylistTracks_ShouldAllowAddingTracks()
    {
        // Arrange
        var playlist = new Playlist { PlaylistId = 1, Name = "Test Playlist" };
        var pt1 = new PlaylistTrack { PlaylistTrackId = 1, PlaylistId = 1, TrackId = 10, Position = 0 };
        var pt2 = new PlaylistTrack { PlaylistTrackId = 2, PlaylistId = 1, TrackId = 20, Position = 1 };

        // Act
        playlist.PlaylistTracks.Add(pt1);
        playlist.PlaylistTracks.Add(pt2);

        // Assert
        playlist.PlaylistTracks.Should().HaveCount(2);
        playlist.PlaylistTracks.Should().Contain(pt1);
        playlist.PlaylistTracks.Should().Contain(pt2);
    }

    [Fact]
    public void Playlist_CreatedAt_ShouldStoreDateTimeCorrectly()
    {
        // Arrange
        var playlist = new Playlist();
        var createdAt = new DateTime(2026, 2, 12, 10, 30, 0);

        // Act
        playlist.CreatedAt = createdAt;

        // Assert
        playlist.CreatedAt.Should().Be(createdAt);
        playlist.CreatedAt.Year.Should().Be(2026);
        playlist.CreatedAt.Month.Should().Be(2);
        playlist.CreatedAt.Day.Should().Be(12);
    }

    [Fact]
    public void Playlist_UpdatedAt_ShouldStoreDateTimeCorrectly()
    {
        // Arrange
        var playlist = new Playlist();
        var updatedAt = new DateTime(2026, 6, 20, 14, 45, 30);

        // Act
        playlist.UpdatedAt = updatedAt;

        // Assert
        playlist.UpdatedAt.Should().Be(updatedAt);
    }
}
