using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Repositories;
using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Repositories;

public class TrackRepositoryTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly TrackRepository _repository;

    public TrackRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new TrackRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task SeedTracksAsync()
    {
        var tracks = new List<Track>
        {
            new Track
            {
                Title = "Song A", FilePath = @"C:\Music\a.mp3", FileHash = "hashA",
                PlayCount = 10, IsFavorite = true,
                LastPlayedAt = DateTime.UtcNow.AddHours(-1)
            },
            new Track
            {
                Title = "Song B", FilePath = @"C:\Music\b.mp3", FileHash = "hashB",
                PlayCount = 50, IsFavorite = false,
                LastPlayedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Track
            {
                Title = "Song C", FilePath = @"C:\Music\c.wav", FileHash = "hashC",
                PlayCount = 25, IsFavorite = true,
                LastPlayedAt = DateTime.UtcNow
            },
            new Track
            {
                Title = "Song D", FilePath = @"C:\Music\d.wma", FileHash = "hashD",
                PlayCount = 5, IsFavorite = false,
                LastPlayedAt = null
            }
        };
        _context.Tracks.AddRange(tracks);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByFilePathAsync_ShouldFindTrack()
    {
        await SeedTracksAsync();

        var result = await _repository.GetByFilePathAsync(@"C:\Music\b.mp3");

        result.Should().NotBeNull();
        result!.Title.Should().Be("Song B");
    }

    [Fact]
    public async Task GetByFilePathAsync_ShouldReturnNull_WhenNotFound()
    {
        await SeedTracksAsync();

        var result = await _repository.GetByFilePathAsync(@"C:\Music\nonexistent.mp3");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetFavoritesAsync_ShouldReturnOnlyFavorites()
    {
        await SeedTracksAsync();

        var result = (await _repository.GetFavoritesAsync()).ToList();

        result.Should().HaveCount(2);
        result.All(t => t.IsFavorite).Should().BeTrue();
        result.Select(t => t.Title).Should().Contain("Song A").And.Contain("Song C");
    }

    [Fact]
    public async Task GetMostPlayedAsync_ShouldOrderByPlayCountDesc()
    {
        await SeedTracksAsync();

        var result = (await _repository.GetMostPlayedAsync(3)).ToList();

        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Song B");   // 50 plays
        result[1].Title.Should().Be("Song C");   // 25 plays
        result[2].Title.Should().Be("Song A");   // 10 plays
    }

    [Fact]
    public async Task GetMostPlayedAsync_ShouldRespectCount()
    {
        await SeedTracksAsync();

        var result = (await _repository.GetMostPlayedAsync(1)).ToList();

        result.Should().HaveCount(1);
        result[0].Title.Should().Be("Song B");
    }

    [Fact]
    public async Task GetRecentlyPlayedAsync_ShouldOrderByLastPlayedAtDesc()
    {
        await SeedTracksAsync();

        var result = (await _repository.GetRecentlyPlayedAsync(3)).ToList();

        result.Should().HaveCount(3);
        result[0].Title.Should().Be("Song C");   // now
        result[1].Title.Should().Be("Song A");   // 1 hour ago
        result[2].Title.Should().Be("Song B");   // 1 day ago
    }

    [Fact]
    public async Task GetRecentlyPlayedAsync_ShouldExcludeNeverPlayed()
    {
        await SeedTracksAsync();

        var result = (await _repository.GetRecentlyPlayedAsync(10)).ToList();

        result.Should().HaveCount(3);
        result.Select(t => t.Title).Should().NotContain("Song D");
    }

    [Fact]
    public async Task GetByFileHashAsync_ShouldFindByHash()
    {
        await SeedTracksAsync();

        var result = await _repository.GetByFileHashAsync("hashC");

        result.Should().NotBeNull();
        result!.Title.Should().Be("Song C");
    }

    [Fact]
    public async Task GetByFileHashAsync_ShouldReturnNull_WhenNotFound()
    {
        await SeedTracksAsync();

        var result = await _repository.GetByFileHashAsync("nonexistent");

        result.Should().BeNull();
    }
}
