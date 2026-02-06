using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Repositories;
using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Repositories;

public class PlaybackHistoryRepositoryTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly PlaybackHistoryRepository _repository;

    public PlaybackHistoryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new PlaybackHistoryRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<(Track track1, Track track2)> SeedDataAsync()
    {
        var track1 = new Track { Title = "Song A", FilePath = @"C:\a.mp3", FileHash = "ha" };
        var track2 = new Track { Title = "Song B", FilePath = @"C:\b.mp3", FileHash = "hb" };
        _context.Tracks.AddRange(track1, track2);
        await _context.SaveChangesAsync();

        var histories = new List<PlaybackHistory>
        {
            new PlaybackHistory
            {
                TrackId = track1.TrackId, PlayedAt = DateTime.UtcNow.AddHours(-3),
                PlayDuration = TimeSpan.FromMinutes(3), CompletedPlayback = true
            },
            new PlaybackHistory
            {
                TrackId = track1.TrackId, PlayedAt = DateTime.UtcNow.AddHours(-1),
                PlayDuration = TimeSpan.FromMinutes(4), CompletedPlayback = true
            },
            new PlaybackHistory
            {
                TrackId = track2.TrackId, PlayedAt = DateTime.UtcNow,
                PlayDuration = TimeSpan.FromMinutes(5), CompletedPlayback = true
            },
            new PlaybackHistory
            {
                TrackId = track2.TrackId, PlayedAt = DateTime.UtcNow.AddDays(-1),
                PlayDuration = null, WasSkipped = true
            },
        };
        _context.PlaybackHistories.AddRange(histories);
        await _context.SaveChangesAsync();

        return (track1, track2);
    }

    [Fact]
    public async Task GetRecentPlaysAsync_ShouldOrderByPlayedAtDesc()
    {
        await SeedDataAsync();

        var result = (await _repository.GetRecentPlaysAsync(3)).ToList();

        result.Should().HaveCount(3);
        result[0].PlayedAt.Should().BeAfter(result[1].PlayedAt);
        result[1].PlayedAt.Should().BeAfter(result[2].PlayedAt);
    }

    [Fact]
    public async Task GetRecentPlaysAsync_ShouldRespectCount()
    {
        await SeedDataAsync();

        var result = (await _repository.GetRecentPlaysAsync(2)).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByTrackAsync_ShouldFilterByTrackId()
    {
        var (track1, _) = await SeedDataAsync();

        var result = (await _repository.GetByTrackAsync(track1.TrackId)).ToList();

        result.Should().HaveCount(2);
        result.All(ph => ph.TrackId == track1.TrackId).Should().BeTrue();
    }

    [Fact]
    public async Task GetByTrackAsync_ShouldReturnEmpty_WhenNoHistory()
    {
        await SeedDataAsync();

        var result = await _repository.GetByTrackAsync(999);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTotalPlayCountAsync_ShouldCountAllPlays()
    {
        await SeedDataAsync();

        var result = await _repository.GetTotalPlayCountAsync();

        result.Should().Be(4);
    }

    [Fact]
    public async Task GetTotalListeningTimeAsync_ShouldSumPlayDuration()
    {
        await SeedDataAsync();

        var result = await _repository.GetTotalListeningTimeAsync();

        // 3 + 4 + 5 = 12 minutes (the null-duration entry is excluded)
        result.Should().Be(TimeSpan.FromMinutes(12));
    }

    [Fact]
    public async Task GetTotalListeningTimeAsync_ShouldReturnZero_WhenEmpty()
    {
        var result = await _repository.GetTotalListeningTimeAsync();

        result.Should().Be(TimeSpan.Zero);
    }
}
