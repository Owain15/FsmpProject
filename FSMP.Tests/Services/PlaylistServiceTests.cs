using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Services;

public class PlaylistServiceTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly PlaylistService _service;

    public PlaylistServiceTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
        _service = new PlaylistService(_unitOfWork);
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

    private async Task<Playlist> CreatePlaylistViaServiceAsync(string name = "Test Playlist", string? description = null)
    {
        return await _service.CreatePlaylistAsync(name, description);
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new PlaylistService(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("unitOfWork");
    }

    // ========== CreatePlaylistAsync Tests ==========

    [Fact]
    public async Task CreatePlaylistAsync_ShouldCreatePlaylist()
    {
        var result = await _service.CreatePlaylistAsync("My Playlist", "A description");

        result.Should().NotBeNull();
        result.Name.Should().Be("My Playlist");
        result.Description.Should().Be("A description");
        result.PlaylistId.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreatePlaylistAsync_ShouldPersistToDatabase()
    {
        var result = await _service.CreatePlaylistAsync("Persisted Playlist");

        var fromDb = await _context.Playlists.FirstOrDefaultAsync(p => p.Name == "Persisted Playlist");
        fromDb.Should().NotBeNull();
        fromDb!.PlaylistId.Should().Be(result.PlaylistId);
    }

    [Fact]
    public async Task CreatePlaylistAsync_ShouldTrimName()
    {
        var result = await _service.CreatePlaylistAsync("  Trimmed Name  ");

        result.Name.Should().Be("Trimmed Name");
    }

    [Fact]
    public async Task CreatePlaylistAsync_ShouldTrimDescription()
    {
        var result = await _service.CreatePlaylistAsync("Playlist", "  Trimmed Desc  ");

        result.Description.Should().Be("Trimmed Desc");
    }

    [Fact]
    public async Task CreatePlaylistAsync_WithNullDescription_ShouldSucceed()
    {
        var result = await _service.CreatePlaylistAsync("No Desc");

        result.Description.Should().BeNull();
    }

    [Fact]
    public async Task CreatePlaylistAsync_WithEmptyName_ShouldThrow()
    {
        var act = () => _service.CreatePlaylistAsync("");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public async Task CreatePlaylistAsync_WithWhitespaceName_ShouldThrow()
    {
        var act = () => _service.CreatePlaylistAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public async Task CreatePlaylistAsync_WithNullName_ShouldThrow()
    {
        var act = () => _service.CreatePlaylistAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("name");
    }

    // ========== GetAllPlaylistsAsync Tests ==========

    [Fact]
    public async Task GetAllPlaylistsAsync_ShouldReturnAllPlaylists()
    {
        await CreatePlaylistViaServiceAsync("Playlist 1");
        await CreatePlaylistViaServiceAsync("Playlist 2");
        await CreatePlaylistViaServiceAsync("Playlist 3");

        var result = (await _service.GetAllPlaylistsAsync()).ToList();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllPlaylistsAsync_ShouldReturnEmpty_WhenNoPlaylists()
    {
        var result = (await _service.GetAllPlaylistsAsync()).ToList();

        result.Should().BeEmpty();
    }

    // ========== GetPlaylistWithTracksAsync Tests ==========

    [Fact]
    public async Task GetPlaylistWithTracksAsync_ShouldReturnPlaylistWithTracks()
    {
        var playlist = await CreatePlaylistViaServiceAsync("With Tracks");
        var track = await CreateTrackAsync();
        await _service.AddTrackAsync(playlist.PlaylistId, track.TrackId);

        var result = await _service.GetPlaylistWithTracksAsync(playlist.PlaylistId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("With Tracks");
        result.PlaylistTracks.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetPlaylistWithTracksAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _service.GetPlaylistWithTracksAsync(9999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPlaylistWithTracksAsync_ShouldReturnEmptyTracks_WhenPlaylistHasNone()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Empty Playlist");

        var result = await _service.GetPlaylistWithTracksAsync(playlist.PlaylistId);

        result.Should().NotBeNull();
        result!.PlaylistTracks.Should().BeEmpty();
    }

    // ========== RenamePlaylistAsync Tests ==========

    [Fact]
    public async Task RenamePlaylistAsync_ShouldRenamePlaylist()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Old Name");

        await _service.RenamePlaylistAsync(playlist.PlaylistId, "New Name");

        var result = await _context.Playlists.FindAsync(playlist.PlaylistId);
        result!.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task RenamePlaylistAsync_ShouldTrimNewName()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Original");

        await _service.RenamePlaylistAsync(playlist.PlaylistId, "  Trimmed  ");

        var result = await _context.Playlists.FindAsync(playlist.PlaylistId);
        result!.Name.Should().Be("Trimmed");
    }

    [Fact]
    public async Task RenamePlaylistAsync_ShouldUpdateTimestamp()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Timestamped");
        var originalUpdatedAt = playlist.UpdatedAt;
        await Task.Delay(10);

        await _service.RenamePlaylistAsync(playlist.PlaylistId, "Renamed");

        var result = await _context.Playlists.FindAsync(playlist.PlaylistId);
        result!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task RenamePlaylistAsync_WithEmptyName_ShouldThrow()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Valid");

        var act = () => _service.RenamePlaylistAsync(playlist.PlaylistId, "");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("newName");
    }

    [Fact]
    public async Task RenamePlaylistAsync_WithNonexistentPlaylist_ShouldThrow()
    {
        var act = () => _service.RenamePlaylistAsync(9999, "New Name");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========== DeletePlaylistAsync Tests ==========

    [Fact]
    public async Task DeletePlaylistAsync_ShouldRemovePlaylist()
    {
        var playlist = await CreatePlaylistViaServiceAsync("To Delete");

        await _service.DeletePlaylistAsync(playlist.PlaylistId);

        var result = await _context.Playlists.FindAsync(playlist.PlaylistId);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeletePlaylistAsync_WithNonexistentPlaylist_ShouldThrow()
    {
        var act = () => _service.DeletePlaylistAsync(9999);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========== AddTrackAsync Tests ==========

    [Fact]
    public async Task AddTrackAsync_ShouldAddTrackToPlaylist()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Add Track Test");
        var track = await CreateTrackAsync();

        await _service.AddTrackAsync(playlist.PlaylistId, track.TrackId);

        var result = await _service.GetPlaylistWithTracksAsync(playlist.PlaylistId);
        result!.PlaylistTracks.Should().HaveCount(1);
        result.PlaylistTracks.First().TrackId.Should().Be(track.TrackId);
        result.PlaylistTracks.First().Position.Should().Be(0);
    }

    [Fact]
    public async Task AddTrackAsync_ShouldAutoPositionAtEnd()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Position Test");
        var track1 = await CreateTrackAsync("Track 1");
        var track2 = await CreateTrackAsync("Track 2");
        var track3 = await CreateTrackAsync("Track 3");

        await _service.AddTrackAsync(playlist.PlaylistId, track1.TrackId);
        await _service.AddTrackAsync(playlist.PlaylistId, track2.TrackId);
        await _service.AddTrackAsync(playlist.PlaylistId, track3.TrackId);

        var result = await _service.GetPlaylistWithTracksAsync(playlist.PlaylistId);
        var tracks = result!.PlaylistTracks.OrderBy(pt => pt.Position).ToList();
        tracks.Should().HaveCount(3);
        tracks[0].Position.Should().Be(0);
        tracks[1].Position.Should().Be(1);
        tracks[2].Position.Should().Be(2);
    }

    [Fact]
    public async Task AddTrackAsync_ShouldUpdatePlaylistTimestamp()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Timestamp Test");
        var track = await CreateTrackAsync();
        var originalUpdatedAt = playlist.UpdatedAt;
        await Task.Delay(10);

        await _service.AddTrackAsync(playlist.PlaylistId, track.TrackId);

        var result = await _context.Playlists.FindAsync(playlist.PlaylistId);
        result!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task AddTrackAsync_WithNonexistentPlaylist_ShouldThrow()
    {
        var track = await CreateTrackAsync();

        var act = () => _service.AddTrackAsync(9999, track.TrackId);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task AddTrackAsync_WithNonexistentTrack_ShouldThrow()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Valid Playlist");

        var act = () => _service.AddTrackAsync(playlist.PlaylistId, 9999);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========== RemoveTrackAtPositionAsync Tests ==========

    [Fact]
    public async Task RemoveTrackAtPositionAsync_ShouldRemoveTrack()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Remove Test");
        var track = await CreateTrackAsync();
        await _service.AddTrackAsync(playlist.PlaylistId, track.TrackId);

        await _service.RemoveTrackAtPositionAsync(playlist.PlaylistId, 0);

        var result = await _service.GetPlaylistWithTracksAsync(playlist.PlaylistId);
        result!.PlaylistTracks.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveTrackAtPositionAsync_ShouldReorderRemainingTracks()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Reorder Test");
        var track1 = await CreateTrackAsync("Track 1");
        var track2 = await CreateTrackAsync("Track 2");
        var track3 = await CreateTrackAsync("Track 3");
        await _service.AddTrackAsync(playlist.PlaylistId, track1.TrackId);
        await _service.AddTrackAsync(playlist.PlaylistId, track2.TrackId);
        await _service.AddTrackAsync(playlist.PlaylistId, track3.TrackId);

        // Remove the middle track (position 1)
        await _service.RemoveTrackAtPositionAsync(playlist.PlaylistId, 1);

        var result = await _service.GetPlaylistWithTracksAsync(playlist.PlaylistId);
        var remaining = result!.PlaylistTracks.OrderBy(pt => pt.Position).ToList();
        remaining.Should().HaveCount(2);
        remaining[0].Position.Should().Be(0);
        remaining[0].TrackId.Should().Be(track1.TrackId);
        remaining[1].Position.Should().Be(1);
        remaining[1].TrackId.Should().Be(track3.TrackId);
    }

    [Fact]
    public async Task RemoveTrackAtPositionAsync_ShouldUpdatePlaylistTimestamp()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Timestamp Remove");
        var track = await CreateTrackAsync();
        await _service.AddTrackAsync(playlist.PlaylistId, track.TrackId);
        var originalUpdatedAt = (await _context.Playlists.FindAsync(playlist.PlaylistId))!.UpdatedAt;
        await Task.Delay(10);

        await _service.RemoveTrackAtPositionAsync(playlist.PlaylistId, 0);

        var result = await _context.Playlists.FindAsync(playlist.PlaylistId);
        result!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task RemoveTrackAtPositionAsync_WithNonexistentPlaylist_ShouldThrow()
    {
        var act = () => _service.RemoveTrackAtPositionAsync(9999, 0);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RemoveTrackAtPositionAsync_WithInvalidPosition_ShouldThrow()
    {
        var playlist = await CreatePlaylistViaServiceAsync("Invalid Position");
        var track = await CreateTrackAsync();
        await _service.AddTrackAsync(playlist.PlaylistId, track.TrackId);

        var act = () => _service.RemoveTrackAtPositionAsync(playlist.PlaylistId, 5);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ========== SearchPlaylistsAsync Tests ==========

    [Fact]
    public async Task SearchPlaylistsAsync_ShouldFindByPartialName()
    {
        await CreatePlaylistViaServiceAsync("Road Trip");
        await CreatePlaylistViaServiceAsync("Workout Mix");
        await CreatePlaylistViaServiceAsync("Chill Vibes");

        var result = (await _service.SearchPlaylistsAsync("Mix")).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Workout Mix");
    }

    [Fact]
    public async Task SearchPlaylistsAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        await CreatePlaylistViaServiceAsync("Road Trip");

        var result = (await _service.SearchPlaylistsAsync("Nonexistent")).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchPlaylistsAsync_WithEmptyTerm_ShouldReturnAll()
    {
        await CreatePlaylistViaServiceAsync("Playlist 1");
        await CreatePlaylistViaServiceAsync("Playlist 2");

        var result = (await _service.SearchPlaylistsAsync("")).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchPlaylistsAsync_WithWhitespaceTerm_ShouldReturnAll()
    {
        await CreatePlaylistViaServiceAsync("Playlist 1");
        await CreatePlaylistViaServiceAsync("Playlist 2");

        var result = (await _service.SearchPlaylistsAsync("   ")).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchPlaylistsAsync_WithNullTerm_ShouldReturnAll()
    {
        await CreatePlaylistViaServiceAsync("Playlist 1");
        await CreatePlaylistViaServiceAsync("Playlist 2");

        var result = (await _service.SearchPlaylistsAsync(null!)).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchPlaylistsAsync_ShouldTrimSearchTerm()
    {
        await CreatePlaylistViaServiceAsync("Road Trip");

        var result = (await _service.SearchPlaylistsAsync("  Road  ")).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Road Trip");
    }
}