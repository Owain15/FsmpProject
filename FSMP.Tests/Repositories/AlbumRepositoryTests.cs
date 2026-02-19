using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Repositories;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Repositories;

public class AlbumRepositoryTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly AlbumRepository _repository;

    public AlbumRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new AlbumRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<(Artist artist1, Artist artist2)> SeedDataAsync()
    {
        var artist1 = new Artist { Name = "Artist One" };
        var artist2 = new Artist { Name = "Artist Two" };
        _context.Artists.AddRange(artist1, artist2);
        await _context.SaveChangesAsync();

        var albums = new List<Album>
        {
            new Album { Title = "Album A", ArtistId = artist1.ArtistId, Year = 2020 },
            new Album { Title = "Album B", ArtistId = artist1.ArtistId, Year = 2022 },
            new Album { Title = "Album C", ArtistId = artist2.ArtistId, Year = 2020 },
        };
        _context.Albums.AddRange(albums);
        await _context.SaveChangesAsync();

        // Add tracks to Album A
        var albumA = await _context.Albums.FirstAsync(a => a.Title == "Album A");
        _context.Tracks.AddRange(
            new Track { Title = "Track 1", FilePath = @"C:\t1.mp3", FileHash = "h1", AlbumId = albumA.AlbumId },
            new Track { Title = "Track 2", FilePath = @"C:\t2.mp3", FileHash = "h2", AlbumId = albumA.AlbumId }
        );
        await _context.SaveChangesAsync();

        return (artist1, artist2);
    }

    [Fact]
    public async Task GetByArtistAsync_ShouldFilterByArtistId()
    {
        var (artist1, _) = await SeedDataAsync();

        var result = (await _repository.GetByArtistAsync(artist1.ArtistId)).ToList();

        result.Should().HaveCount(2);
        result.Select(a => a.Title).Should().Contain("Album A").And.Contain("Album B");
    }

    [Fact]
    public async Task GetByArtistAsync_ShouldReturnEmpty_WhenNoAlbums()
    {
        await SeedDataAsync();

        var result = await _repository.GetByArtistAsync(999);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByYearAsync_ShouldFilterByYear()
    {
        await SeedDataAsync();

        var result = (await _repository.GetByYearAsync(2020)).ToList();

        result.Should().HaveCount(2);
        result.Select(a => a.Title).Should().Contain("Album A").And.Contain("Album C");
    }

    [Fact]
    public async Task GetByYearAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        await SeedDataAsync();

        var result = await _repository.GetByYearAsync(1999);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetWithTracksAsync_ShouldIncludeTracksNavigation()
    {
        await SeedDataAsync();
        var albumA = await _context.Albums.FirstAsync(a => a.Title == "Album A");

        var result = await _repository.GetWithTracksAsync(albumA.AlbumId);

        result.Should().NotBeNull();
        result!.Tracks.Should().HaveCount(2);
        result.Tracks.Select(t => t.Title).Should().Contain("Track 1").And.Contain("Track 2");
    }

    [Fact]
    public async Task GetWithTracksAsync_ShouldReturnNull_WhenNotFound()
    {
        await SeedDataAsync();

        var result = await _repository.GetWithTracksAsync(999);

        result.Should().BeNull();
    }
}
