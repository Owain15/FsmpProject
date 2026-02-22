using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FSMP.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.UI;

public class StatisticsViewerTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly StatisticsService _statsService;

    public StatisticsViewerTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _unitOfWork = new UnitOfWork(_context);
        _statsService = new StatisticsService(_unitOfWork);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    // --- Helpers ---

    private (StatisticsViewer viewer, StringWriter output) CreateViewerWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var viewer = new StatisticsViewer(_statsService, input, output);
        return (viewer, output);
    }

    private async Task<Track> CreateTrackAsync(string title, string artistName = "Artist", int playCount = 0, bool isFavorite = false, int? rating = null, DateTime? lastPlayed = null)
    {
        var artist = _context.Artists.Local.FirstOrDefault(a => a.Name == artistName);
        if (artist == null)
        {
            artist = new Artist { Name = artistName };
            await _unitOfWork.Artists.AddAsync(artist);
            await _unitOfWork.SaveAsync();
        }

        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{Guid.NewGuid()}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PlayCount = playCount,
            IsFavorite = isFavorite,
            Rating = rating,
            LastPlayedAt = lastPlayed,
            ArtistId = artist.ArtistId,
            Artist = artist,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    private async Task CreatePlaybackHistoryAsync(Track track, TimeSpan duration)
    {
        var history = new PlaybackHistory
        {
            TrackId = track.TrackId,
            PlayedAt = DateTime.UtcNow,
            PlayDuration = duration,
            CompletedPlayback = true,
        };
        await _unitOfWork.PlaybackHistories.AddAsync(history);
        await _unitOfWork.SaveAsync();
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullStatsService_ShouldThrow()
    {
        var act = () => new StatisticsViewer(null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("statsService");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new StatisticsViewer(_statsService, null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new StatisticsViewer(_statsService, TextReader.Null, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== DisplayTotalStatisticsAsync Tests ==========

    [Fact]
    public async Task DisplayTotalStatisticsAsync_EmptyLibrary_ShouldShowZeros()
    {
        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayTotalStatisticsAsync();

        var text = output.ToString();
        text.Should().Contain("Library Overview");
        text.Should().Contain("Total tracks:   0");
        text.Should().Contain("Total plays:    0");
        text.Should().Contain("Listening time:");
    }

    [Fact]
    public async Task DisplayTotalStatisticsAsync_WithData_ShouldShowCounts()
    {
        var track = await CreateTrackAsync("Song1", playCount: 5);
        await CreatePlaybackHistoryAsync(track, TimeSpan.FromMinutes(3));
        await CreatePlaybackHistoryAsync(track, TimeSpan.FromMinutes(4));

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayTotalStatisticsAsync();

        var text = output.ToString();
        text.Should().Contain("Total tracks:   1");
        text.Should().Contain("Total plays:    2");
        text.Should().Contain("7m");
    }

    [Fact]
    public async Task DisplayTotalStatisticsAsync_LargeListeningTime_ShouldShowHours()
    {
        var track = await CreateTrackAsync("Song1");
        await CreatePlaybackHistoryAsync(track, TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(30)));

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayTotalStatisticsAsync();

        var text = output.ToString();
        text.Should().Contain("2h 30m");
    }

    // ========== DisplayMostPlayedAsync Tests ==========

    [Fact]
    public async Task DisplayMostPlayedAsync_EmptyLibrary_ShouldShowMessage()
    {
        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayMostPlayedAsync();

        var text = output.ToString();
        text.Should().Contain("Most Played");
        text.Should().Contain("No playback history yet.");
    }

    [Fact]
    public async Task DisplayMostPlayedAsync_WithTracks_ShouldFormatCorrectly()
    {
        await CreateTrackAsync("Hit Song", "Pop Star", playCount: 42, lastPlayed: DateTime.UtcNow);
        await CreateTrackAsync("B-Side", "Pop Star", playCount: 5, lastPlayed: DateTime.UtcNow);

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayMostPlayedAsync();

        var text = output.ToString();
        text.Should().Contain("Most Played");
        text.Should().Contain("Pop Star - Hit Song");
        text.Should().Contain("42 plays");
        text.Should().Contain("Pop Star - B-Side");
        text.Should().Contain("5 plays");
    }

    [Fact]
    public async Task DisplayMostPlayedAsync_ShouldOrderByPlayCount()
    {
        await CreateTrackAsync("Low", "A", playCount: 1, lastPlayed: DateTime.UtcNow);
        await CreateTrackAsync("High", "A", playCount: 100, lastPlayed: DateTime.UtcNow);
        await CreateTrackAsync("Mid", "A", playCount: 50, lastPlayed: DateTime.UtcNow);

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayMostPlayedAsync();

        var text = output.ToString();
        var highIdx = text.IndexOf("High");
        var midIdx = text.IndexOf("Mid");
        var lowIdx = text.IndexOf("Low");

        highIdx.Should().BeLessThan(midIdx);
        midIdx.Should().BeLessThan(lowIdx);
    }

    // ========== DisplayRecentlyPlayedAsync Tests ==========

    [Fact]
    public async Task DisplayRecentlyPlayedAsync_EmptyLibrary_ShouldShowMessage()
    {
        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayRecentlyPlayedAsync();

        var text = output.ToString();
        text.Should().Contain("Recently Played");
        text.Should().Contain("No playback history yet.");
    }

    [Fact]
    public async Task DisplayRecentlyPlayedAsync_WithTracks_ShouldShowLastPlayed()
    {
        var lastPlayed = new DateTime(2025, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        await CreateTrackAsync("Recent Hit", "Band", playCount: 3, lastPlayed: lastPlayed);

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayRecentlyPlayedAsync();

        var text = output.ToString();
        text.Should().Contain("Recently Played");
        text.Should().Contain("Band - Recent Hit");
        text.Should().Contain("2025-06-15 14:30");
    }

    [Fact]
    public async Task DisplayRecentlyPlayedAsync_ShouldOrderByMostRecent()
    {
        await CreateTrackAsync("Oldest", "A", playCount: 1, lastPlayed: new DateTime(2024, 1, 1));
        await CreateTrackAsync("Newest", "A", playCount: 1, lastPlayed: new DateTime(2025, 12, 31));
        await CreateTrackAsync("Middle", "A", playCount: 1, lastPlayed: new DateTime(2025, 6, 1));

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayRecentlyPlayedAsync();

        var text = output.ToString();
        var newestIdx = text.IndexOf("Newest");
        var middleIdx = text.IndexOf("Middle");
        var oldestIdx = text.IndexOf("Oldest");

        newestIdx.Should().BeLessThan(middleIdx);
        middleIdx.Should().BeLessThan(oldestIdx);
    }

    // ========== DisplayFavoritesAsync Tests ==========

    [Fact]
    public async Task DisplayFavoritesAsync_NoFavorites_ShouldShowMessage()
    {
        await CreateTrackAsync("Not Fav", isFavorite: false);

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayFavoritesAsync();

        var text = output.ToString();
        text.Should().Contain("Favorites");
        text.Should().Contain("No favorites yet.");
    }

    [Fact]
    public async Task DisplayFavoritesAsync_WithFavorites_ShouldListThem()
    {
        await CreateTrackAsync("Loved It", "Great Band", isFavorite: true, rating: 5);
        await CreateTrackAsync("Also Good", "Nice Band", isFavorite: true, rating: 3);
        await CreateTrackAsync("Meh", "OK Band", isFavorite: false);

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayFavoritesAsync();

        var text = output.ToString();
        text.Should().Contain("Favorites");
        text.Should().Contain("Great Band - Loved It");
        text.Should().Contain("*****");
        text.Should().Contain("Nice Band - Also Good");
        text.Should().Contain("***");
        text.Should().NotContain("OK Band");
    }

    [Fact]
    public async Task DisplayFavoritesAsync_FavoriteWithoutRating_ShouldOmitRating()
    {
        await CreateTrackAsync("No Rating Fav", "Band", isFavorite: true);

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayFavoritesAsync();

        var text = output.ToString();
        text.Should().Contain("Band - No Rating Fav");
        text.Should().NotContain("|");
    }

    // ========== DisplayGenreBreakdownAsync Tests ==========

    [Fact]
    public async Task DisplayGenreBreakdownAsync_NoGenreData_ShouldShowMessage()
    {
        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayGenreBreakdownAsync();

        var text = output.ToString();
        text.Should().Contain("Genre Breakdown");
        text.Should().Contain("No genre data available.");
    }

    [Fact]
    public async Task DisplayGenreBreakdownAsync_WithTags_ShouldShowCounts()
    {
        // Get seed tags from DB
        var rock = _context.Tags.First(g => g.Name == "Rock");
        var jazz = _context.Tags.First(g => g.Name == "Jazz");

        var track1 = await CreateTrackAsync("Rock Song 1");
        track1.Tags.Add(rock);
        var track2 = await CreateTrackAsync("Rock Song 2");
        track2.Tags.Add(rock);
        var track3 = await CreateTrackAsync("Jazz Song");
        track3.Tags.Add(jazz);
        await _unitOfWork.SaveAsync();

        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.DisplayGenreBreakdownAsync();

        var text = output.ToString();
        text.Should().Contain("Genre Breakdown");
        text.Should().Contain("Rock");
        text.Should().Contain("2 tracks");
        text.Should().Contain("Jazz");
        text.Should().Contain("1 tracks");
    }

    // ========== RunAsync Integration Tests ==========

    [Fact]
    public async Task RunAsync_BackOption_ShouldExit()
    {
        var (viewer, output) = CreateViewerWithOutput("0\n");

        await viewer.RunAsync();

        output.ToString().Should().Contain("Statistics");
    }

    [Fact]
    public async Task RunAsync_EmptyInput_ShouldExit()
    {
        var (viewer, output) = CreateViewerWithOutput("");

        await viewer.RunAsync();

        output.ToString().Should().Contain("Statistics");
    }

    [Fact]
    public async Task RunAsync_OverviewOption_ShouldShowOverview()
    {
        var (viewer, output) = CreateViewerWithOutput("1\n0\n");

        await viewer.RunAsync();

        output.ToString().Should().Contain("Library Overview");
    }

    [Fact]
    public async Task RunAsync_MostPlayedOption_ShouldShowMostPlayed()
    {
        var (viewer, output) = CreateViewerWithOutput("2\n0\n");

        await viewer.RunAsync();

        output.ToString().Should().Contain("Most Played");
    }

    [Fact]
    public async Task RunAsync_RecentlyPlayedOption_ShouldShowRecent()
    {
        var (viewer, output) = CreateViewerWithOutput("3\n0\n");

        await viewer.RunAsync();

        output.ToString().Should().Contain("Recently Played");
    }

    [Fact]
    public async Task RunAsync_FavoritesOption_ShouldShowFavorites()
    {
        var (viewer, output) = CreateViewerWithOutput("4\n0\n");

        await viewer.RunAsync();

        output.ToString().Should().Contain("Favorites");
    }

    [Fact]
    public async Task RunAsync_GenreOption_ShouldShowGenres()
    {
        var (viewer, output) = CreateViewerWithOutput("5\n0\n");

        await viewer.RunAsync();

        output.ToString().Should().Contain("Genre Breakdown");
    }

    [Fact]
    public async Task RunAsync_InvalidOption_ShouldShowError()
    {
        var (viewer, output) = CreateViewerWithOutput("x\n0\n");

        await viewer.RunAsync();

        output.ToString().Should().Contain("Invalid option");
    }
}
