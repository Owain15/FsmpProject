using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.Models;

public class PlaybackHistoryTests
{
    [Fact]
    public void PlaybackHistory_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var history = new PlaybackHistory();

        // Assert
        history.PlaybackHistoryId.Should().Be(0);
        history.TrackId.Should().Be(0);
        history.PlayedAt.Should().Be(default);
        history.PlayDuration.Should().BeNull();
        history.CompletedPlayback.Should().BeFalse();
        history.WasSkipped.Should().BeFalse();
        // Track navigation property uses null! so accessing it would throw
    }

    [Fact]
    public void PlaybackHistory_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var history = new PlaybackHistory();
        var playedAt = DateTime.Now;
        var duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(45));
        var track = new Track { TrackId = 1, Title = "Test Track" };

        // Act
        history.PlaybackHistoryId = 1;
        history.TrackId = 1;
        history.PlayedAt = playedAt;
        history.PlayDuration = duration;
        history.CompletedPlayback = true;
        history.WasSkipped = false;
        history.Track = track;

        // Assert
        history.PlaybackHistoryId.Should().Be(1);
        history.TrackId.Should().Be(1);
        history.PlayedAt.Should().Be(playedAt);
        history.PlayDuration.Should().Be(duration);
        history.CompletedPlayback.Should().BeTrue();
        history.WasSkipped.Should().BeFalse();
        history.Track.Should().Be(track);
    }

    [Fact]
    public void PlaybackHistory_PlayedAt_ShouldStoreTimestampCorrectly()
    {
        // Arrange
        var history = new PlaybackHistory();
        var playedAt = new DateTime(2024, 5, 15, 14, 30, 45);

        // Act
        history.PlayedAt = playedAt;

        // Assert
        history.PlayedAt.Should().Be(playedAt);
        history.PlayedAt.Year.Should().Be(2024);
        history.PlayedAt.Month.Should().Be(5);
        history.PlayedAt.Day.Should().Be(15);
        history.PlayedAt.Hour.Should().Be(14);
        history.PlayedAt.Minute.Should().Be(30);
        history.PlayedAt.Second.Should().Be(45);
    }

    [Fact]
    public void PlaybackHistory_PlayDuration_ShouldBeNullable()
    {
        // Arrange
        var history = new PlaybackHistory
        {
            PlayDuration = TimeSpan.FromMinutes(2)
        };

        // Assert default is null via new instance
        var newHistory = new PlaybackHistory();
        newHistory.PlayDuration.Should().BeNull();

        // Act - set to null
        history.PlayDuration = null;

        // Assert
        history.PlayDuration.Should().BeNull();
    }

    [Fact]
    public void PlaybackHistory_PlayDuration_ShouldStoreTimeSpanCorrectly()
    {
        // Arrange
        var history = new PlaybackHistory();
        var duration = new TimeSpan(0, 4, 32); // 4 minutes 32 seconds

        // Act
        history.PlayDuration = duration;

        // Assert
        history.PlayDuration.Should().Be(duration);
        history.PlayDuration!.Value.TotalSeconds.Should().Be(272);
        history.PlayDuration!.Value.Minutes.Should().Be(4);
        history.PlayDuration!.Value.Seconds.Should().Be(32);
    }

    [Fact]
    public void PlaybackHistory_CompletedPlayback_ShouldDefaultToFalse()
    {
        // Arrange & Act
        var history = new PlaybackHistory();

        // Assert
        history.CompletedPlayback.Should().BeFalse();
    }

    [Fact]
    public void PlaybackHistory_CompletedPlayback_ShouldBeSettableToTrue()
    {
        // Arrange
        var history = new PlaybackHistory();

        // Act
        history.CompletedPlayback = true;

        // Assert
        history.CompletedPlayback.Should().BeTrue();
    }

    [Fact]
    public void PlaybackHistory_WasSkipped_ShouldDefaultToFalse()
    {
        // Arrange & Act
        var history = new PlaybackHistory();

        // Assert
        history.WasSkipped.Should().BeFalse();
    }

    [Fact]
    public void PlaybackHistory_WasSkipped_ShouldBeSettableToTrue()
    {
        // Arrange
        var history = new PlaybackHistory();

        // Act
        history.WasSkipped = true;

        // Assert
        history.WasSkipped.Should().BeTrue();
    }

    [Fact]
    public void PlaybackHistory_Track_ShouldStoreNavigationProperty()
    {
        // Arrange
        var track = new Track
        {
            TrackId = 42,
            Title = "My Favorite Song",
            FilePath = @"C:\Music\song.mp3"
        };
        var history = new PlaybackHistory();

        // Act
        history.TrackId = track.TrackId;
        history.Track = track;

        // Assert
        history.Track.Should().NotBeNull();
        history.Track.TrackId.Should().Be(42);
        history.Track.Title.Should().Be("My Favorite Song");
        history.TrackId.Should().Be(history.Track.TrackId);
    }

    [Fact]
    public void PlaybackHistory_CompletedAndSkipped_CanBothBeTrue()
    {
        // Arrange - edge case: started playing, skipped partway, but technically "completed" (reached threshold)
        var history = new PlaybackHistory();

        // Act
        history.CompletedPlayback = true;
        history.WasSkipped = true;

        // Assert - both flags can be set (unusual but valid edge case)
        history.CompletedPlayback.Should().BeTrue();
        history.WasSkipped.Should().BeTrue();
    }

    [Fact]
    public void PlaybackHistory_ShortPlayDuration_ShouldBeStorable()
    {
        // Arrange - user skipped after just 5 seconds
        var history = new PlaybackHistory();
        var shortDuration = TimeSpan.FromSeconds(5);

        // Act
        history.PlayDuration = shortDuration;
        history.WasSkipped = true;
        history.CompletedPlayback = false;

        // Assert
        history.PlayDuration!.Value.TotalSeconds.Should().Be(5);
        history.WasSkipped.Should().BeTrue();
        history.CompletedPlayback.Should().BeFalse();
    }

    [Fact]
    public void PlaybackHistory_LongPlayDuration_ShouldBeStorable()
    {
        // Arrange - very long track (e.g., live recording or audiobook chapter)
        var history = new PlaybackHistory();
        var longDuration = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30));

        // Act
        history.PlayDuration = longDuration;

        // Assert
        history.PlayDuration!.Value.TotalHours.Should().Be(2.5);
    }
}
