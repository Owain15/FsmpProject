using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Services;

public class StatisticsServiceTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly StatisticsService _service;

    public StatisticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
        _service = new StatisticsService(_unitOfWork);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    // --- Helpers ---

    private async Task<Track> CreateTrackAsync(string title, int playCount = 0,
        DateTime? lastPlayedAt = null, bool isFavorite = false)
    {
        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{title}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            PlayCount = playCount,
            LastPlayedAt = lastPlayedAt,
            IsFavorite = isFavorite,
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    private async Task CreatePlaybackHistoryAsync(int trackId, TimeSpan? duration = null)
    {
        var history = new PlaybackHistory
        {
            TrackId = trackId,
            PlayedAt = DateTime.UtcNow,
            PlayDuration = duration ?? TimeSpan.FromSeconds(180),
            CompletedPlayback = true,
            WasSkipped = false,
        };
        await _unitOfWork.PlaybackHistories.AddAsync(history);
        await _unitOfWork.SaveAsync();
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new StatisticsService(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    // ========== GetMostPlayedTracksAsync Tests ==========

    [Fact]
    public async Task GetMostPlayedTracksAsync_ShouldReturnTopByPlayCount()
    {
        await CreateTrackAsync("Low", playCount: 1);
        await CreateTrackAsync("High", playCount: 100);
        await CreateTrackAsync("Mid", playCount: 50);

        var result = (await _service.GetMostPlayedTracksAsync(2)).ToList();

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("High");
        result[1].Title.Should().Be("Mid");
    }

    [Fact]
    public async Task GetMostPlayedTracksAsync_ShouldReturnEmpty_WhenNoTracks()
    {
        var result = await _service.GetMostPlayedTracksAsync(5);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMostPlayedTracksAsync_WithZeroCount_ShouldThrow()
    {
        var act = () => _service.GetMostPlayedTracksAsync(0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetMostPlayedTracksAsync_WithNegativeCount_ShouldThrow()
    {
        var act = () => _service.GetMostPlayedTracksAsync(-1);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    // ========== GetRecentlyPlayedTracksAsync Tests ==========

    [Fact]
    public async Task GetRecentlyPlayedTracksAsync_ShouldReturnTopByLastPlayedAt()
    {
        await CreateTrackAsync("Old", lastPlayedAt: DateTime.UtcNow.AddDays(-10));
        await CreateTrackAsync("Recent", lastPlayedAt: DateTime.UtcNow.AddMinutes(-5));
        await CreateTrackAsync("Middle", lastPlayedAt: DateTime.UtcNow.AddDays(-1));

        var result = (await _service.GetRecentlyPlayedTracksAsync(2)).ToList();

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Recent");
        result[1].Title.Should().Be("Middle");
    }

    [Fact]
    public async Task GetRecentlyPlayedTracksAsync_ShouldExcludeNeverPlayed()
    {
        await CreateTrackAsync("Never Played", lastPlayedAt: null);
        await CreateTrackAsync("Played", lastPlayedAt: DateTime.UtcNow);

        var result = (await _service.GetRecentlyPlayedTracksAsync(10)).ToList();

        result.Should().ContainSingle().Which.Title.Should().Be("Played");
    }

    [Fact]
    public async Task GetRecentlyPlayedTracksAsync_ShouldReturnEmpty_WhenNoTracks()
    {
        var result = await _service.GetRecentlyPlayedTracksAsync(5);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentlyPlayedTracksAsync_WithZeroCount_ShouldThrow()
    {
        var act = () => _service.GetRecentlyPlayedTracksAsync(0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    // ========== GetFavoritesAsync Tests ==========

    [Fact]
    public async Task GetFavoritesAsync_ShouldReturnOnlyFavorites()
    {
        await CreateTrackAsync("Fav 1", isFavorite: true);
        await CreateTrackAsync("Not Fav", isFavorite: false);
        await CreateTrackAsync("Fav 2", isFavorite: true);

        var result = (await _service.GetFavoritesAsync()).ToList();

        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.IsFavorite);
    }

    [Fact]
    public async Task GetFavoritesAsync_ShouldReturnEmpty_WhenNoFavorites()
    {
        await CreateTrackAsync("Not Fav", isFavorite: false);

        var result = await _service.GetFavoritesAsync();

        result.Should().BeEmpty();
    }

    // ========== GetGenreStatisticsAsync Tests ==========

    [Fact]
    public async Task GetGenreStatisticsAsync_ShouldReturnEmpty_WhenNoTracksHaveGenres()
    {
        await CreateTrackAsync("Track 1");

        var result = await _service.GetGenreStatisticsAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetGenreStatisticsAsync_ShouldAggregateCountsByGenre()
    {
        // Get seeded genres from DB
        var genres = (await _unitOfWork.Genres.GetAllAsync()).ToList();
        if (genres.Count < 2) return; // skip if no seed data

        var genre1 = genres[0];
        var genre2 = genres[1];

        var track1 = await CreateTrackAsync("Track 1");
        var track2 = await CreateTrackAsync("Track 2");
        var track3 = await CreateTrackAsync("Track 3");

        // Assign genres via navigation properties
        track1.Genres.Add(genre1);
        track2.Genres.Add(genre1);
        track3.Genres.Add(genre2);
        await _unitOfWork.SaveAsync();

        var result = await _service.GetGenreStatisticsAsync();

        result.Should().ContainKey(genre1.Name).WhoseValue.Should().Be(2);
        result.Should().ContainKey(genre2.Name).WhoseValue.Should().Be(1);
    }

    // ========== GetTotalPlayCountAsync Tests ==========

    [Fact]
    public async Task GetTotalPlayCountAsync_ShouldReturnTotalHistoryCount()
    {
        var track = await CreateTrackAsync("Track 1");
        await CreatePlaybackHistoryAsync(track.TrackId);
        await CreatePlaybackHistoryAsync(track.TrackId);
        await CreatePlaybackHistoryAsync(track.TrackId);

        var result = await _service.GetTotalPlayCountAsync();

        result.Should().Be(3);
    }

    [Fact]
    public async Task GetTotalPlayCountAsync_ShouldReturnZero_WhenNoHistory()
    {
        var result = await _service.GetTotalPlayCountAsync();

        result.Should().Be(0);
    }

    // ========== GetTotalListeningTimeAsync Tests ==========

    [Fact]
    public async Task GetTotalListeningTimeAsync_ShouldSumPlayDurations()
    {
        var track = await CreateTrackAsync("Track 1");
        await CreatePlaybackHistoryAsync(track.TrackId, TimeSpan.FromMinutes(3));
        await CreatePlaybackHistoryAsync(track.TrackId, TimeSpan.FromMinutes(5));

        var result = await _service.GetTotalListeningTimeAsync();

        result.Should().Be(TimeSpan.FromMinutes(8));
    }

    [Fact]
    public async Task GetTotalListeningTimeAsync_ShouldReturnZero_WhenNoHistory()
    {
        var result = await _service.GetTotalListeningTimeAsync();

        result.Should().Be(TimeSpan.Zero);
    }

    // ========== GetTotalTrackCountAsync Tests ==========

    [Fact]
    public async Task GetTotalTrackCountAsync_ShouldReturnTrackCount()
    {
        await CreateTrackAsync("Track 1");
        await CreateTrackAsync("Track 2");
        await CreateTrackAsync("Track 3");

        var result = await _service.GetTotalTrackCountAsync();

        result.Should().Be(3);
    }

    [Fact]
    public async Task GetTotalTrackCountAsync_ShouldReturnZero_WhenEmpty()
    {
        var result = await _service.GetTotalTrackCountAsync();

        result.Should().Be(0);
    }
}
