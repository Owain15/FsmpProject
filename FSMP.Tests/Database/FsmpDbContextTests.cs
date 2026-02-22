using FluentAssertions;
using FsmpDataAcsses;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Database;

public class FsmpDbContextTests : IDisposable
{
    private readonly FsmpDbContext _context;

    public FsmpDbContextTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public void Tracks_DbSet_ShouldNotBeNull()
    {
        _context.Tracks.Should().NotBeNull();
    }

    [Fact]
    public void Albums_DbSet_ShouldNotBeNull()
    {
        _context.Albums.Should().NotBeNull();
    }

    [Fact]
    public void Artists_DbSet_ShouldNotBeNull()
    {
        _context.Artists.Should().NotBeNull();
    }

    [Fact]
    public void Tags_DbSet_ShouldNotBeNull()
    {
        _context.Tags.Should().NotBeNull();
    }

    [Fact]
    public void FileExtensions_DbSet_ShouldNotBeNull()
    {
        _context.FileExtensions.Should().NotBeNull();
    }

    [Fact]
    public void PlaybackHistories_DbSet_ShouldNotBeNull()
    {
        _context.PlaybackHistories.Should().NotBeNull();
    }

    [Fact]
    public void LibraryPaths_DbSet_ShouldNotBeNull()
    {
        _context.LibraryPaths.Should().NotBeNull();
    }

    [Fact]
    public void InMemoryDatabase_ShouldCreateSuccessfully()
    {
        _context.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public void CanAddAndRetrieve_Track()
    {
        var track = new Track
        {
            Title = "Test Song",
            FilePath = @"C:\Music\test.mp3",
            FileHash = "abc123"
        };

        _context.Tracks.Add(track);
        _context.SaveChanges();

        var retrieved = _context.Tracks.First(t => t.FilePath == @"C:\Music\test.mp3");
        retrieved.Title.Should().Be("Test Song");
        retrieved.TrackId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CanAddAndRetrieve_Album()
    {
        var album = new Album
        {
            Title = "Test Album",
            Year = 2024
        };

        _context.Albums.Add(album);
        _context.SaveChanges();

        var retrieved = _context.Albums.First(a => a.Title == "Test Album");
        retrieved.Year.Should().Be(2024);
        retrieved.AlbumId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CanAddAndRetrieve_Artist()
    {
        var artist = new Artist
        {
            Name = "Test Artist"
        };

        _context.Artists.Add(artist);
        _context.SaveChanges();

        var retrieved = _context.Artists.First(a => a.Name == "Test Artist");
        retrieved.ArtistId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CanAddAndRetrieve_PlaybackHistory()
    {
        var track = new Track { Title = "Song", FilePath = @"C:\song.mp3", FileHash = "hash1" };
        _context.Tracks.Add(track);
        _context.SaveChanges();

        var history = new PlaybackHistory
        {
            TrackId = track.TrackId,
            PlayedAt = DateTime.UtcNow,
            CompletedPlayback = true
        };

        _context.PlaybackHistories.Add(history);
        _context.SaveChanges();

        var retrieved = _context.PlaybackHistories.First();
        retrieved.TrackId.Should().Be(track.TrackId);
        retrieved.CompletedPlayback.Should().BeTrue();
    }

    [Fact]
    public void CanAddAndRetrieve_LibraryPath()
    {
        var libraryPath = new LibraryPath
        {
            Path = @"C:\Music",
            AddedAt = DateTime.UtcNow
        };

        _context.LibraryPaths.Add(libraryPath);
        _context.SaveChanges();

        var retrieved = _context.LibraryPaths.First(lp => lp.Path == @"C:\Music");
        retrieved.LibraryPathId.Should().BeGreaterThan(0);
        retrieved.IsActive.Should().BeTrue();
    }

    [Fact]
    public void TagSeedData_ShouldBePresent()
    {
        var tags = _context.Tags.OrderBy(g => g.TagId).ToList();

        tags.Should().HaveCount(5);
        tags[0].Name.Should().Be("Rock");
        tags[1].Name.Should().Be("Jazz");
        tags[2].Name.Should().Be("Classic");
        tags[3].Name.Should().Be("Metal");
        tags[4].Name.Should().Be("Comedy");
    }

    [Fact]
    public void FileExtensionSeedData_ShouldBePresent()
    {
        var extensions = _context.FileExtensions.OrderBy(fe => fe.FileExtensionId).ToList();

        extensions.Should().HaveCount(3);
        extensions[0].Extension.Should().Be("wav");
        extensions[1].Extension.Should().Be("wma");
        extensions[2].Extension.Should().Be("mp3");
    }
}
