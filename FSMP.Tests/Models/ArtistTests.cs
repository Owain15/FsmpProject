using FluentAssertions;
using FSMP.Core.Models;

namespace FSMP.Tests.Models;

public class ArtistTests
{
    [Fact]
    public void Artist_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var artist = new Artist();

        // Assert
        artist.ArtistId.Should().Be(0);
        artist.Name.Should().BeEmpty();
        artist.SortName.Should().BeNull();
        artist.Biography.Should().BeNull();
        artist.CreatedAt.Should().Be(default);
        artist.UpdatedAt.Should().Be(default);
        artist.Albums.Should().NotBeNull().And.BeEmpty();
        artist.Tracks.Should().NotBeNull().And.BeEmpty();
        artist.Genres.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Artist_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var artist = new Artist();
        var testDate = DateTime.Now;

        // Act
        artist.ArtistId = 1;
        artist.Name = "The Beatles";
        artist.SortName = "Beatles, The";
        artist.Biography = "English rock band formed in Liverpool in 1960.";
        artist.CreatedAt = testDate;
        artist.UpdatedAt = testDate;

        // Assert
        artist.ArtistId.Should().Be(1);
        artist.Name.Should().Be("The Beatles");
        artist.SortName.Should().Be("Beatles, The");
        artist.Biography.Should().Be("English rock band formed in Liverpool in 1960.");
        artist.CreatedAt.Should().Be(testDate);
        artist.UpdatedAt.Should().Be(testDate);
    }

    [Fact]
    public void Artist_SortName_ShouldBeNullable()
    {
        // Arrange
        var artist = new Artist { SortName = "Initial Sort Name" };

        // Act
        artist.SortName = null;

        // Assert
        artist.SortName.Should().BeNull();
    }

    [Fact]
    public void Artist_SortName_ShouldAllowDifferentFromName()
    {
        // Arrange & Act
        var artist = new Artist
        {
            Name = "The Rolling Stones",
            SortName = "Rolling Stones, The"
        };

        // Assert
        artist.Name.Should().Be("The Rolling Stones");
        artist.SortName.Should().Be("Rolling Stones, The");
        artist.SortName.Should().NotBe(artist.Name);
    }

    [Fact]
    public void Artist_Albums_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var artist = new Artist();

        // Assert
        artist.Albums.Should().NotBeNull();
        artist.Albums.Should().BeEmpty();
        artist.Albums.Should().BeAssignableTo<ICollection<Album>>();
    }

    [Fact]
    public void Artist_Albums_ShouldAllowAddingAlbums()
    {
        // Arrange
        var artist = new Artist { ArtistId = 1, Name = "Test Artist" };
        var album1 = new Album { AlbumId = 1, Title = "Album One", ArtistId = 1 };
        var album2 = new Album { AlbumId = 2, Title = "Album Two", ArtistId = 1 };

        // Act
        artist.Albums.Add(album1);
        artist.Albums.Add(album2);

        // Assert
        artist.Albums.Should().HaveCount(2);
        artist.Albums.Should().Contain(album1);
        artist.Albums.Should().Contain(album2);
    }

    [Fact]
    public void Artist_Tracks_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var artist = new Artist();

        // Assert
        artist.Tracks.Should().NotBeNull();
        artist.Tracks.Should().BeEmpty();
        artist.Tracks.Should().BeAssignableTo<ICollection<Track>>();
    }

    [Fact]
    public void Artist_Tracks_ShouldAllowAddingTracks()
    {
        // Arrange
        var artist = new Artist { ArtistId = 1, Name = "Test Artist" };
        var track1 = new Track { TrackId = 1, Title = "Song One", ArtistId = 1 };
        var track2 = new Track { TrackId = 2, Title = "Song Two", ArtistId = 1 };

        // Act
        artist.Tracks.Add(track1);
        artist.Tracks.Add(track2);

        // Assert
        artist.Tracks.Should().HaveCount(2);
        artist.Tracks.Should().Contain(track1);
        artist.Tracks.Should().Contain(track2);
    }

    [Fact]
    public void Artist_Genres_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange & Act
        var artist = new Artist();

        // Assert
        artist.Genres.Should().NotBeNull();
        artist.Genres.Should().BeEmpty();
        artist.Genres.Should().BeAssignableTo<ICollection<Genre>>();
    }

    [Fact]
    public void Artist_Genres_ShouldAllowAddingMultipleGenres()
    {
        // Arrange
        var artist = new Artist { ArtistId = 1, Name = "Versatile Artist" };
        var rock = new Genre { GenreId = 1, Name = "Rock" };
        var pop = new Genre { GenreId = 2, Name = "Pop" };
        var jazz = new Genre { GenreId = 3, Name = "Jazz" };

        // Act
        artist.Genres.Add(rock);
        artist.Genres.Add(pop);
        artist.Genres.Add(jazz);

        // Assert
        artist.Genres.Should().HaveCount(3);
        artist.Genres.Should().Contain(g => g.Name == "Rock");
        artist.Genres.Should().Contain(g => g.Name == "Pop");
        artist.Genres.Should().Contain(g => g.Name == "Jazz");
    }

    [Fact]
    public void Artist_Biography_ShouldBeNullable()
    {
        // Arrange
        var artist = new Artist { Biography = "Some biography text" };

        // Act
        artist.Biography = null;

        // Assert
        artist.Biography.Should().BeNull();
    }

    [Fact]
    public void Artist_Biography_ShouldStoreLongText()
    {
        // Arrange
        var artist = new Artist();
        var longBiography = new string('A', 5000); // 5000 character biography

        // Act
        artist.Biography = longBiography;

        // Assert
        artist.Biography.Should().HaveLength(5000);
        artist.Biography.Should().Be(longBiography);
    }

    [Fact]
    public void Artist_CreatedAt_ShouldStoreDateTimeCorrectly()
    {
        // Arrange
        var artist = new Artist();
        var createdAt = new DateTime(2024, 3, 15, 10, 30, 0);

        // Act
        artist.CreatedAt = createdAt;

        // Assert
        artist.CreatedAt.Should().Be(createdAt);
        artist.CreatedAt.Year.Should().Be(2024);
        artist.CreatedAt.Month.Should().Be(3);
        artist.CreatedAt.Day.Should().Be(15);
    }

    [Fact]
    public void Artist_UpdatedAt_ShouldStoreDateTimeCorrectly()
    {
        // Arrange
        var artist = new Artist();
        var updatedAt = new DateTime(2024, 6, 20, 14, 45, 30);

        // Act
        artist.UpdatedAt = updatedAt;

        // Assert
        artist.UpdatedAt.Should().Be(updatedAt);
        artist.UpdatedAt.Year.Should().Be(2024);
        artist.UpdatedAt.Month.Should().Be(6);
        artist.UpdatedAt.Day.Should().Be(20);
    }
}
