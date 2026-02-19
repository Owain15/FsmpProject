using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.Models;

public class AlbumTests
{
    [Fact]
    public void Album_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var album = new Album();

        // Assert
        album.AlbumId.Should().Be(0);
        album.Title.Should().BeEmpty();
        album.Year.Should().BeNull();
        album.AlbumArtistName.Should().BeNull();
        album.ArtistId.Should().BeNull();
        album.AlbumArt.Should().BeNull();
        album.AlbumArtPath.Should().BeNull();
        album.CreatedAt.Should().Be(default);
        album.UpdatedAt.Should().Be(default);
        album.Artist.Should().BeNull();
        album.Tracks.Should().NotBeNull().And.BeEmpty();
        album.Genres.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Album_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var album = new Album();
        var testDate = DateTime.Now;
        var albumArtBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header bytes

        // Act
        album.AlbumId = 1;
        album.Title = "Test Album";
        album.Year = 2024;
        album.AlbumArtistName = "Various Artists";
        album.ArtistId = 10;
        album.AlbumArt = albumArtBytes;
        album.AlbumArtPath = @"C:\Music\covers\album.jpg";
        album.CreatedAt = testDate;
        album.UpdatedAt = testDate;

        // Assert
        album.AlbumId.Should().Be(1);
        album.Title.Should().Be("Test Album");
        album.Year.Should().Be(2024);
        album.AlbumArtistName.Should().Be("Various Artists");
        album.ArtistId.Should().Be(10);
        album.AlbumArt.Should().BeEquivalentTo(albumArtBytes);
        album.AlbumArtPath.Should().Be(@"C:\Music\covers\album.jpg");
        album.CreatedAt.Should().Be(testDate);
        album.UpdatedAt.Should().Be(testDate);
    }

    [Fact]
    public void Album_Artist_ShouldBeNullable()
    {
        // Arrange
        var album = new Album();

        // Act & Assert - can be null
        album.Artist.Should().BeNull();

        // Act - set artist
        var artist = new Artist { ArtistId = 1, Name = "Test Artist" };
        album.Artist = artist;
        album.ArtistId = artist.ArtistId;

        // Assert
        album.Artist.Should().NotBeNull();
        album.Artist!.Name.Should().Be("Test Artist");
        album.ArtistId.Should().Be(1);

        // Act - set back to null
        album.Artist = null;
        album.ArtistId = null;

        // Assert
        album.Artist.Should().BeNull();
        album.ArtistId.Should().BeNull();
    }

    [Fact]
    public void Album_Tracks_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var album = new Album();

        // Assert
        album.Tracks.Should().NotBeNull();
        album.Tracks.Should().BeEmpty();
        album.Tracks.Should().BeAssignableTo<ICollection<Track>>();
    }

    [Fact]
    public void Album_Tracks_ShouldAllowAddingTracks()
    {
        // Arrange
        var album = new Album { AlbumId = 1, Title = "Test Album" };
        var track1 = new Track { TrackId = 1, Title = "Track 1", AlbumId = 1 };
        var track2 = new Track { TrackId = 2, Title = "Track 2", AlbumId = 1 };

        // Act
        album.Tracks.Add(track1);
        album.Tracks.Add(track2);

        // Assert
        album.Tracks.Should().HaveCount(2);
        album.Tracks.Should().Contain(track1);
        album.Tracks.Should().Contain(track2);
    }

    [Fact]
    public void Album_AlbumArt_ShouldStoreBytesCorrectly()
    {
        // Arrange
        var album = new Album();
        // Simulate a small image file (PNG header + some data)
        var albumArtBytes = new byte[] {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, // PNG signature
            0x00, 0x00, 0x00, 0x0D, // IHDR chunk length
            0x49, 0x48, 0x44, 0x52  // IHDR chunk type
        };

        // Act
        album.AlbumArt = albumArtBytes;

        // Assert
        album.AlbumArt.Should().NotBeNull();
        album.AlbumArt.Should().HaveCount(16);
        album.AlbumArt.Should().BeEquivalentTo(albumArtBytes);
    }

    [Fact]
    public void Album_AlbumArt_ShouldAcceptNull()
    {
        // Arrange
        var album = new Album
        {
            AlbumArt = new byte[] { 0x01, 0x02, 0x03 }
        };

        // Act
        album.AlbumArt = null;

        // Assert
        album.AlbumArt.Should().BeNull();
    }

    [Fact]
    public void Album_Year_ShouldBeNullable()
    {
        // Arrange
        var album = new Album();

        // Assert - default is null
        album.Year.Should().BeNull();

        // Act - set year
        album.Year = 1985;

        // Assert
        album.Year.Should().Be(1985);

        // Act - set back to null
        album.Year = null;

        // Assert
        album.Year.Should().BeNull();
    }

    [Theory]
    [InlineData(1900)]
    [InlineData(2000)]
    [InlineData(2024)]
    [InlineData(2025)]
    public void Album_Year_ShouldAcceptValidYears(int year)
    {
        // Arrange
        var album = new Album();

        // Act
        album.Year = year;

        // Assert
        album.Year.Should().Be(year);
    }

    [Fact]
    public void Album_Genres_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var album = new Album();

        // Assert
        album.Genres.Should().NotBeNull();
        album.Genres.Should().BeEmpty();
        album.Genres.Should().BeAssignableTo<ICollection<Genre>>();
    }

    [Fact]
    public void Album_Genres_ShouldAllowAddingMultipleGenres()
    {
        // Arrange
        var album = new Album { AlbumId = 1, Title = "Multi-Genre Album" };
        var rock = new Genre { GenreId = 1, Name = "Rock" };
        var metal = new Genre { GenreId = 2, Name = "Metal" };

        // Act
        album.Genres.Add(rock);
        album.Genres.Add(metal);

        // Assert
        album.Genres.Should().HaveCount(2);
        album.Genres.Should().Contain(g => g.Name == "Rock");
        album.Genres.Should().Contain(g => g.Name == "Metal");
    }

    [Fact]
    public void Album_AlbumArtistName_ShouldBeIndependentOfArtist()
    {
        // Arrange - Album artist name can differ from linked Artist entity
        // (e.g., compilation albums have "Various Artists" but individual tracks have their own artists)
        var album = new Album();
        var artist = new Artist { ArtistId = 1, Name = "Some Band" };

        // Act
        album.Artist = artist;
        album.ArtistId = artist.ArtistId;
        album.AlbumArtistName = "Various Artists";

        // Assert
        album.Artist!.Name.Should().Be("Some Band");
        album.AlbumArtistName.Should().Be("Various Artists");
        album.AlbumArtistName.Should().NotBe(album.Artist.Name);
    }

    [Fact]
    public void Album_CreatedAt_ShouldStoreDateTimeCorrectly()
    {
        // Arrange
        var album = new Album();
        var createdAt = new DateTime(2024, 1, 15, 10, 30, 0);

        // Act
        album.CreatedAt = createdAt;

        // Assert
        album.CreatedAt.Should().Be(createdAt);
        album.CreatedAt.Year.Should().Be(2024);
        album.CreatedAt.Month.Should().Be(1);
        album.CreatedAt.Day.Should().Be(15);
    }

    [Fact]
    public void Album_UpdatedAt_ShouldStoreDateTimeCorrectly()
    {
        // Arrange
        var album = new Album();
        var updatedAt = new DateTime(2024, 6, 20, 14, 45, 30);

        // Act
        album.UpdatedAt = updatedAt;

        // Assert
        album.UpdatedAt.Should().Be(updatedAt);
        album.UpdatedAt.Year.Should().Be(2024);
        album.UpdatedAt.Month.Should().Be(6);
        album.UpdatedAt.Day.Should().Be(20);
    }

    [Fact]
    public void Album_AlbumArtPath_ShouldStorePathCorrectly()
    {
        // Arrange
        var album = new Album();
        var artPath = @"C:\Music\Artist\Album\cover.jpg";

        // Act
        album.AlbumArtPath = artPath;

        // Assert
        album.AlbumArtPath.Should().Be(artPath);
    }

    [Fact]
    public void Album_AlbumArtPath_ShouldBeNullable()
    {
        // Arrange
        var album = new Album
        {
            AlbumArtPath = @"C:\some\path.jpg"
        };

        // Act
        album.AlbumArtPath = null;

        // Assert
        album.AlbumArtPath.Should().BeNull();
    }
}
