using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Repositories;
using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Repositories;

public class ArtistRepositoryTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly ArtistRepository _repository;

    public ArtistRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new ArtistRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task SeedDataAsync()
    {
        var artist1 = new Artist { Name = "The Rock Band" };
        var artist2 = new Artist { Name = "Jazz Ensemble" };
        var artist3 = new Artist { Name = "Rock Orchestra" };
        _context.Artists.AddRange(artist1, artist2, artist3);
        await _context.SaveChangesAsync();

        // Albums for artist1
        _context.Albums.AddRange(
            new Album { Title = "First Album", ArtistId = artist1.ArtistId },
            new Album { Title = "Second Album", ArtistId = artist1.ArtistId }
        );
        await _context.SaveChangesAsync();

        // Tracks for artist1
        _context.Tracks.AddRange(
            new Track { Title = "Song X", FilePath = @"C:\x.mp3", FileHash = "hx", ArtistId = artist1.ArtistId },
            new Track { Title = "Song Y", FilePath = @"C:\y.mp3", FileHash = "hy", ArtistId = artist1.ArtistId },
            new Track { Title = "Song Z", FilePath = @"C:\z.mp3", FileHash = "hz", ArtistId = artist1.ArtistId }
        );
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetWithAlbumsAsync_ShouldIncludeAlbumsNavigation()
    {
        await SeedDataAsync();
        var artist = await _context.Artists.FirstAsync(a => a.Name == "The Rock Band");

        var result = await _repository.GetWithAlbumsAsync(artist.ArtistId);

        result.Should().NotBeNull();
        result!.Albums.Should().HaveCount(2);
        result.Albums.Select(a => a.Title).Should().Contain("First Album").And.Contain("Second Album");
    }

    [Fact]
    public async Task GetWithAlbumsAsync_ShouldReturnNull_WhenNotFound()
    {
        await SeedDataAsync();

        var result = await _repository.GetWithAlbumsAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithTracksAsync_ShouldIncludeTracksNavigation()
    {
        await SeedDataAsync();
        var artist = await _context.Artists.FirstAsync(a => a.Name == "The Rock Band");

        var result = await _repository.GetWithTracksAsync(artist.ArtistId);

        result.Should().NotBeNull();
        result!.Tracks.Should().HaveCount(3);
        result.Tracks.Select(t => t.Title).Should().Contain("Song X").And.Contain("Song Y").And.Contain("Song Z");
    }

    [Fact]
    public async Task GetWithTracksAsync_ShouldReturnNull_WhenNotFound()
    {
        await SeedDataAsync();

        var result = await _repository.GetWithTracksAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ShouldFilterByNameContaining()
    {
        await SeedDataAsync();

        var result = (await _repository.SearchAsync("Rock")).ToList();

        result.Should().HaveCount(2);
        result.Select(a => a.Name).Should().Contain("The Rock Band").And.Contain("Rock Orchestra");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        await SeedDataAsync();

        var result = await _repository.SearchAsync("Classical");

        result.Should().BeEmpty();
    }
}
