using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.Models;

public class PlaylistTrackTests
{
    [Fact]
    public void PlaylistTrack_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var pt = new PlaylistTrack();

        // Assert
        pt.PlaylistTrackId.Should().Be(0);
        pt.PlaylistId.Should().Be(0);
        pt.TrackId.Should().Be(0);
        pt.Position.Should().Be(0);
        pt.AddedAt.Should().Be(default);
        pt.Playlist.Should().BeNull();
        pt.Track.Should().BeNull();
    }

    [Fact]
    public void PlaylistTrack_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var pt = new PlaylistTrack();
        var testDate = DateTime.Now;

        // Act
        pt.PlaylistTrackId = 1;
        pt.PlaylistId = 5;
        pt.TrackId = 42;
        pt.Position = 3;
        pt.AddedAt = testDate;

        // Assert
        pt.PlaylistTrackId.Should().Be(1);
        pt.PlaylistId.Should().Be(5);
        pt.TrackId.Should().Be(42);
        pt.Position.Should().Be(3);
        pt.AddedAt.Should().Be(testDate);
    }

    [Fact]
    public void PlaylistTrack_Playlist_ShouldBeNullable()
    {
        // Arrange
        var pt = new PlaylistTrack();

        // Assert - default is null
        pt.Playlist.Should().BeNull();

        // Act - set playlist
        var playlist = new Playlist { PlaylistId = 1, Name = "Test" };
        pt.Playlist = playlist;
        pt.PlaylistId = playlist.PlaylistId;

        // Assert
        pt.Playlist.Should().NotBeNull();
        pt.Playlist!.Name.Should().Be("Test");
        pt.PlaylistId.Should().Be(1);

        // Act - set back to null
        pt.Playlist = null;

        // Assert
        pt.Playlist.Should().BeNull();
    }

    [Fact]
    public void PlaylistTrack_Track_ShouldBeNullable()
    {
        // Arrange
        var pt = new PlaylistTrack();

        // Assert - default is null
        pt.Track.Should().BeNull();

        // Act - set track
        var track = new Track { TrackId = 10, Title = "Test Track" };
        pt.Track = track;
        pt.TrackId = track.TrackId;

        // Assert
        pt.Track.Should().NotBeNull();
        pt.Track!.Title.Should().Be("Test Track");
        pt.TrackId.Should().Be(10);

        // Act - set back to null
        pt.Track = null;

        // Assert
        pt.Track.Should().BeNull();
    }

    [Fact]
    public void PlaylistTrack_AddedAt_ShouldStoreDateTimeCorrectly()
    {
        // Arrange
        var pt = new PlaylistTrack();
        var addedAt = new DateTime(2026, 2, 12, 8, 15, 0);

        // Act
        pt.AddedAt = addedAt;

        // Assert
        pt.AddedAt.Should().Be(addedAt);
        pt.AddedAt.Year.Should().Be(2026);
        pt.AddedAt.Month.Should().Be(2);
        pt.AddedAt.Day.Should().Be(12);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(99)]
    [InlineData(500)]
    public void PlaylistTrack_Position_ShouldAcceptValidValues(int position)
    {
        // Arrange
        var pt = new PlaylistTrack();

        // Act
        pt.Position = position;

        // Assert
        pt.Position.Should().Be(position);
    }

    [Fact]
    public void PlaylistTrack_ShouldLinkToPlaylistAndTrack()
    {
        // Arrange
        var playlist = new Playlist { PlaylistId = 1, Name = "Road Trip" };
        var track = new Track { TrackId = 10, Title = "Highway to Hell" };

        // Act
        var pt = new PlaylistTrack
        {
            PlaylistTrackId = 1,
            PlaylistId = playlist.PlaylistId,
            TrackId = track.TrackId,
            Position = 0,
            AddedAt = DateTime.Now,
            Playlist = playlist,
            Track = track
        };

        // Assert
        pt.Playlist.Should().BeSameAs(playlist);
        pt.Track.Should().BeSameAs(track);
        pt.PlaylistId.Should().Be(playlist.PlaylistId);
        pt.TrackId.Should().Be(track.TrackId);
    }
}
