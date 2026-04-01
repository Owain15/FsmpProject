using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Services;

public class LibraryStatsServiceTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly LibraryStatsService _service;

    public LibraryStatsServiceTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _service = new LibraryStatsService(_context);
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public async Task GetDirectoryStatsAsync_ReturnsCorrectCounts()
    {
        var artist1 = new Artist { ArtistId = 1, Name = "Artist1" };
        var artist2 = new Artist { ArtistId = 2, Name = "Artist2" };
        var album1 = new Album { AlbumId = 1, Title = "Album1" };
        var album2 = new Album { AlbumId = 2, Title = "Album2" };

        _context.Artists.AddRange(artist1, artist2);
        _context.Albums.AddRange(album1, album2);
        _context.Tracks.AddRange(
            new Track { TrackId = 1, Title = "T1", FilePath = @"C:\Music\a.mp3", ArtistId = 1, AlbumId = 1 },
            new Track { TrackId = 2, Title = "T2", FilePath = @"C:\Music\b.mp3", ArtistId = 1, AlbumId = 2 },
            new Track { TrackId = 3, Title = "T3", FilePath = @"C:\Music\c.mp3", ArtistId = 2, AlbumId = 2 },
            new Track { TrackId = 4, Title = "T4", FilePath = @"D:\Other\d.mp3", ArtistId = 1, AlbumId = 1 }
        );
        await _context.SaveChangesAsync();

        var stats = await _service.GetDirectoryStatsAsync(@"C:\Music");

        stats.TrackCount.Should().Be(3);
        stats.AlbumCount.Should().Be(2);
        stats.ArtistCount.Should().Be(2);
    }

    [Fact]
    public async Task GetDirectoryStatsAsync_ReturnsZeros_WhenNoTracksInDirectory()
    {
        var stats = await _service.GetDirectoryStatsAsync(@"C:\Empty");

        stats.TrackCount.Should().Be(0);
        stats.AlbumCount.Should().Be(0);
        stats.ArtistCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDirectoryStatsAsync_ThrowsOnNull()
    {
        var act = () => _service.GetDirectoryStatsAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullContext()
    {
        var act = () => new LibraryStatsService(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetTotalStatsAsync_ReturnsCorrectCounts()
    {
        var artist1 = new Artist { ArtistId = 1, Name = "Artist1" };
        var artist2 = new Artist { ArtistId = 2, Name = "Artist2" };
        var album1 = new Album { AlbumId = 1, Title = "Album1" };

        _context.Artists.AddRange(artist1, artist2);
        _context.Albums.Add(album1);
        _context.Tracks.AddRange(
            new Track { TrackId = 1, Title = "T1", FilePath = @"C:\Music\a.mp3", ArtistId = 1, AlbumId = 1 },
            new Track { TrackId = 2, Title = "T2", FilePath = @"D:\Other\b.mp3", ArtistId = 2, AlbumId = 1 }
        );
        await _context.SaveChangesAsync();

        var stats = await _service.GetTotalStatsAsync();

        stats.TrackCount.Should().Be(2);
        stats.AlbumCount.Should().Be(1);
        stats.ArtistCount.Should().Be(2);
    }

    [Fact]
    public async Task GetTotalStatsAsync_ReturnsZeros_WhenEmpty()
    {
        var stats = await _service.GetTotalStatsAsync();

        stats.TrackCount.Should().Be(0);
        stats.AlbumCount.Should().Be(0);
        stats.ArtistCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDirectoryStatsAsync_HandlesTrailingSlash()
    {
        _context.Tracks.Add(new Track { TrackId = 1, Title = "T1", FilePath = @"C:\Music\a.mp3", ArtistId = 1, AlbumId = 1 });
        await _context.SaveChangesAsync();

        var stats = await _service.GetDirectoryStatsAsync(@"C:\Music\");

        stats.TrackCount.Should().Be(1);
    }
}
