using FluentAssertions;
using FsmpLibrary.Models;

namespace FSMP.Tests.Models;

public class TrackTests
{
    [Fact]
    public void Track_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var track = new Track();

        // Assert
        track.TrackId.Should().Be(0);
        track.Title.Should().BeEmpty();
        track.FilePath.Should().BeEmpty();
        track.FileFormat.Should().BeEmpty();
        track.FileSizeBytes.Should().Be(0);
        track.TrackNumber.Should().BeNull();
        track.DiscNumber.Should().BeNull();
        track.Duration.Should().BeNull();
        track.BitRate.Should().BeNull();
        track.SampleRate.Should().BeNull();
        track.CustomTitle.Should().BeNull();
        track.CustomArtist.Should().BeNull();
        track.CustomAlbum.Should().BeNull();
        track.CustomYear.Should().BeNull();
        track.CustomGenre.Should().BeNull();
        track.Comment.Should().BeNull();
        track.ArtistId.Should().BeNull();
        track.AlbumId.Should().BeNull();
        track.PlayCount.Should().Be(0);
        track.SkipCount.Should().Be(0);
        track.LastPlayedAt.Should().BeNull();
        track.IsFavorite.Should().BeFalse();
        track.Rating.Should().BeNull();
        track.ImportedAt.Should().Be(default);
        track.UpdatedAt.Should().Be(default);
        track.FileHash.Should().BeEmpty();
        track.Artist.Should().BeNull();
        track.Album.Should().BeNull();
        track.PlaybackHistories.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Track_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var track = new Track();
        var testDate = DateTime.Now;

        // Act
        track.TrackId = 1;
        track.Title = "Test Track";
        track.FilePath = @"C:\Music\test.mp3";
        track.FileFormat = "MP3";
        track.FileSizeBytes = 5242880; // 5 MB
        track.TrackNumber = 3;
        track.DiscNumber = 1;
        track.Duration = TimeSpan.FromMinutes(4);
        track.BitRate = 320;
        track.SampleRate = 44100;
        track.CustomTitle = "Custom Test Track";
        track.CustomArtist = "Custom Artist";
        track.CustomAlbum = "Custom Album";
        track.CustomYear = 2024;
        track.CustomGenre = "Rock";
        track.Comment = "Great song!";
        track.ArtistId = 10;
        track.AlbumId = 20;
        track.PlayCount = 42;
        track.SkipCount = 5;
        track.LastPlayedAt = testDate;
        track.IsFavorite = true;
        track.Rating = 5;
        track.ImportedAt = testDate;
        track.UpdatedAt = testDate;
        track.FileHash = "abc123def456";

        // Assert
        track.TrackId.Should().Be(1);
        track.Title.Should().Be("Test Track");
        track.FilePath.Should().Be(@"C:\Music\test.mp3");
        track.FileFormat.Should().Be("MP3");
        track.FileSizeBytes.Should().Be(5242880);
        track.TrackNumber.Should().Be(3);
        track.DiscNumber.Should().Be(1);
        track.Duration.Should().Be(TimeSpan.FromMinutes(4));
        track.BitRate.Should().Be(320);
        track.SampleRate.Should().Be(44100);
        track.CustomTitle.Should().Be("Custom Test Track");
        track.CustomArtist.Should().Be("Custom Artist");
        track.CustomAlbum.Should().Be("Custom Album");
        track.CustomYear.Should().Be(2024);
        track.CustomGenre.Should().Be("Rock");
        track.Comment.Should().Be("Great song!");
        track.ArtistId.Should().Be(10);
        track.AlbumId.Should().Be(20);
        track.PlayCount.Should().Be(42);
        track.SkipCount.Should().Be(5);
        track.LastPlayedAt.Should().Be(testDate);
        track.IsFavorite.Should().BeTrue();
        track.Rating.Should().Be(5);
        track.ImportedAt.Should().Be(testDate);
        track.UpdatedAt.Should().Be(testDate);
        track.FileHash.Should().Be("abc123def456");
    }

    [Fact]
    public void DisplayTitle_ShouldReturnCustomTitle_WhenCustomTitleIsSet()
    {
        // Arrange
        var track = new Track
        {
            Title = "Original Title",
            CustomTitle = "Custom Title"
        };

        // Act
        var displayTitle = track.DisplayTitle;

        // Assert
        displayTitle.Should().Be("Custom Title");
    }

    [Fact]
    public void DisplayTitle_ShouldReturnTitle_WhenCustomTitleIsNull()
    {
        // Arrange
        var track = new Track
        {
            Title = "Original Title",
            CustomTitle = null
        };

        // Act
        var displayTitle = track.DisplayTitle;

        // Assert
        displayTitle.Should().Be("Original Title");
    }

    [Fact]
    public void DisplayArtist_ShouldReturnCustomArtist_WhenCustomArtistIsSet()
    {
        // Arrange
        var track = new Track
        {
            CustomArtist = "Custom Artist"
        };

        // Act
        var displayArtist = track.DisplayArtist;

        // Assert
        displayArtist.Should().Be("Custom Artist");
    }

    [Fact]
    public void DisplayArtist_ShouldReturnUnknownArtist_WhenCustomArtistAndArtistAreNull()
    {
        // Arrange
        var track = new Track
        {
            CustomArtist = null,
            Artist = null
        };

        // Act
        var displayArtist = track.DisplayArtist;

        // Assert
        displayArtist.Should().Be("Unknown Artist");
    }

    [Fact]
    public void DisplayAlbum_ShouldReturnCustomAlbum_WhenCustomAlbumIsSet()
    {
        // Arrange
        var track = new Track
        {
            CustomAlbum = "Custom Album"
        };

        // Act
        var displayAlbum = track.DisplayAlbum;

        // Assert
        displayAlbum.Should().Be("Custom Album");
    }

    [Fact]
    public void DisplayAlbum_ShouldReturnUnknownAlbum_WhenCustomAlbumAndAlbumAreNull()
    {
        // Arrange
        var track = new Track
        {
            CustomAlbum = null,
            Album = null
        };

        // Act
        var displayAlbum = track.DisplayAlbum;

        // Assert
        displayAlbum.Should().Be("Unknown Album");
    }

    [Fact]
    public void PlaybackHistories_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var track = new Track();

        // Assert
        track.PlaybackHistories.Should().NotBeNull();
        track.PlaybackHistories.Should().BeEmpty();
        track.PlaybackHistories.Should().BeAssignableTo<ICollection<PlaybackHistory>>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Rating_ShouldAcceptValidRatings(int rating)
    {
        // Arrange
        var track = new Track();

        // Act
        track.Rating = rating;

        // Assert
        track.Rating.Should().Be(rating);
    }

    [Fact]
    public void Rating_ShouldAcceptNull()
    {
        // Arrange
        var track = new Track { Rating = 5 };

        // Act
        track.Rating = null;

        // Assert
        track.Rating.Should().BeNull();
    }

    [Fact]
    public void FileFormat_ShouldStoreUppercaseExtensions()
    {
        // Arrange
        var track = new Track();

        // Act
        track.FileFormat = "MP3";

        // Assert
        track.FileFormat.Should().Be("MP3");
    }

    [Fact]
    public void Duration_ShouldStoreTimeSpanCorrectly()
    {
        // Arrange
        var track = new Track();
        var duration = new TimeSpan(0, 3, 45); // 3 minutes 45 seconds

        // Act
        track.Duration = duration;

        // Assert
        track.Duration.Should().Be(duration);
        track.Duration.Value.TotalSeconds.Should().Be(225);
    }
}
