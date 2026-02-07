using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Services;

public class PlaybackTrackingServiceTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly PlaybackTrackingService _service;

    public PlaybackTrackingServiceTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
        _service = new PlaybackTrackingService(_unitOfWork);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    // --- Helpers ---

    private async Task<Track> CreateTrackAsync(string title = "Test Track")
    {
        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{title}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new PlaybackTrackingService(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    // ========== RecordPlaybackAsync Tests ==========

    [Fact]
    public async Task RecordPlaybackAsync_WithNullTrack_ShouldThrow()
    {
        var act = () => _service.RecordPlaybackAsync(null!, TimeSpan.FromSeconds(60), true, false);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RecordPlaybackAsync_ShouldCreatePlaybackHistoryRecord()
    {
        var track = await CreateTrackAsync();

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(180), true, false);

        var histories = await _unitOfWork.PlaybackHistories.GetByTrackAsync(track.TrackId);
        var history = histories.Should().ContainSingle().Subject;
        history.TrackId.Should().Be(track.TrackId);
        history.CompletedPlayback.Should().BeTrue();
        history.WasSkipped.Should().BeFalse();
        history.PlayDuration.Should().Be(TimeSpan.FromSeconds(180));
        history.PlayedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RecordPlaybackAsync_ShouldIncrementPlayCount_WhenCompleted()
    {
        var track = await CreateTrackAsync();
        track.PlayCount.Should().Be(0);

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(180), completed: true, skipped: false);

        track.PlayCount.Should().Be(1);
    }

    [Fact]
    public async Task RecordPlaybackAsync_ShouldNotIncrementPlayCount_WhenNotCompleted()
    {
        var track = await CreateTrackAsync();

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(30), completed: false, skipped: false);

        track.PlayCount.Should().Be(0);
    }

    [Fact]
    public async Task RecordPlaybackAsync_ShouldIncrementSkipCount_WhenSkipped()
    {
        var track = await CreateTrackAsync();
        track.SkipCount.Should().Be(0);

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(10), completed: false, skipped: true);

        track.SkipCount.Should().Be(1);
    }

    [Fact]
    public async Task RecordPlaybackAsync_ShouldNotIncrementSkipCount_WhenNotSkipped()
    {
        var track = await CreateTrackAsync();

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(180), completed: true, skipped: false);

        track.SkipCount.Should().Be(0);
    }

    [Fact]
    public async Task RecordPlaybackAsync_ShouldUpdateLastPlayedAt()
    {
        var track = await CreateTrackAsync();
        track.LastPlayedAt.Should().BeNull();

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(60), completed: false, skipped: false);

        track.LastPlayedAt.Should().NotBeNull();
        track.LastPlayedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RecordPlaybackAsync_ShouldUpdateUpdatedAt()
    {
        var track = await CreateTrackAsync();
        var originalUpdatedAt = track.UpdatedAt;

        // Small delay to ensure time difference
        await Task.Delay(10);

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(60), completed: true, skipped: false);

        track.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task RecordPlaybackAsync_ShouldAccumulateMultiplePlays()
    {
        var track = await CreateTrackAsync();

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(180), completed: true, skipped: false);
        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(180), completed: true, skipped: false);
        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(10), completed: false, skipped: true);

        track.PlayCount.Should().Be(2);
        track.SkipCount.Should().Be(1);

        var histories = await _unitOfWork.PlaybackHistories.GetByTrackAsync(track.TrackId);
        histories.Should().HaveCount(3);
    }

    [Fact]
    public async Task RecordPlaybackAsync_WithCompletedAndSkipped_ShouldIncrementBoth()
    {
        var track = await CreateTrackAsync();

        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(180), completed: true, skipped: true);

        track.PlayCount.Should().Be(1);
        track.SkipCount.Should().Be(1);
    }

    // ========== GetTrackHistoryAsync Tests ==========

    [Fact]
    public async Task GetTrackHistoryAsync_ShouldReturnHistoryForTrack()
    {
        var track = await CreateTrackAsync();
        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(60), true, false);
        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(120), true, false);

        var history = await _service.GetTrackHistoryAsync(track.TrackId);

        history.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTrackHistoryAsync_ShouldReturnEmpty_ForTrackWithNoHistory()
    {
        var track = await CreateTrackAsync();

        var history = await _service.GetTrackHistoryAsync(track.TrackId);

        history.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTrackHistoryAsync_ShouldNotReturnOtherTracksHistory()
    {
        var track1 = await CreateTrackAsync("Track 1");
        var track2 = await CreateTrackAsync("Track 2");
        await _service.RecordPlaybackAsync(track1, TimeSpan.FromSeconds(60), true, false);
        await _service.RecordPlaybackAsync(track2, TimeSpan.FromSeconds(60), true, false);

        var history = await _service.GetTrackHistoryAsync(track1.TrackId);

        history.Should().ContainSingle();
        history.First().TrackId.Should().Be(track1.TrackId);
    }

    // ========== GetRecentPlaysAsync Tests ==========

    [Fact]
    public async Task GetRecentPlaysAsync_ShouldReturnMostRecent()
    {
        var track = await CreateTrackAsync();
        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(60), true, false);
        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(120), true, false);
        await _service.RecordPlaybackAsync(track, TimeSpan.FromSeconds(180), true, false);

        var recent = await _service.GetRecentPlaysAsync(2);

        recent.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentPlaysAsync_ShouldReturnEmpty_WhenNoHistory()
    {
        var recent = await _service.GetRecentPlaysAsync(5);

        recent.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentPlaysAsync_WithZeroCount_ShouldThrow()
    {
        var act = () => _service.GetRecentPlaysAsync(0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task GetRecentPlaysAsync_WithNegativeCount_ShouldThrow()
    {
        var act = () => _service.GetRecentPlaysAsync(-1);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
