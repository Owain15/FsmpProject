using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Repositories;
using FsmpLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Repositories;

public class PlaylistRepositoryTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly PlaylistRepository _repository;

    public PlaylistRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new PlaylistRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private async Task<List<Track>> SeedTracksAsync()
    {
        var tracks = new List<Track>
        {
            new Track { Title = "Track A", FilePath = @"C:\Music\a.mp3", FileHash = "hashA" },
            new Track { Title = "Track B", FilePath = @"C:\Music\b.mp3", FileHash = "hashB" },
            new Track { Title = "Track C", FilePath = @"C:\Music\c.mp3", FileHash = "hashC" }
        };
        _context.Tracks.AddRange(tracks);
        await _context.SaveChangesAsync();
        return tracks;
    }

    private async Task SeedPlaylistsAsync()
    {
        var tracks = await SeedTracksAsync();

        var playlists = new List<Playlist>
        {
            new Playlist
            {
                Name = "Road Trip",
                Description = "Songs for the road",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Playlist
            {
                Name = "Workout Mix",
                Description = "High energy tracks",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Playlist
            {
                Name = "Chill Vibes",
                Description = null,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            }
        };
        _context.Playlists.AddRange(playlists);
        await _context.SaveChangesAsync();

        // Add tracks to the first playlist
        var playlistTracks = new List<PlaylistTrack>
        {
            new PlaylistTrack
            {
                PlaylistId = playlists[0].PlaylistId,
                TrackId = tracks[0].TrackId,
                Position = 0,
                AddedAt = DateTime.UtcNow
            },
            new PlaylistTrack
            {
                PlaylistId = playlists[0].PlaylistId,
                TrackId = tracks[1].TrackId,
                Position = 1,
                AddedAt = DateTime.UtcNow
            }
        };
        _context.PlaylistTracks.AddRange(playlistTracks);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetWithTracksAsync_ShouldReturnPlaylistWithTracks()
    {
        await SeedPlaylistsAsync();
        var playlist = await _context.Playlists.FirstAsync(p => p.Name == "Road Trip");

        var result = await _repository.GetWithTracksAsync(playlist.PlaylistId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Road Trip");
        result.PlaylistTracks.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetWithTracksAsync_ShouldReturnNull_WhenNotFound()
    {
        await SeedPlaylistsAsync();

        var result = await _repository.GetWithTracksAsync(9999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithTracksAsync_ShouldReturnEmptyTracks_WhenPlaylistHasNone()
    {
        await SeedPlaylistsAsync();
        var playlist = await _context.Playlists.FirstAsync(p => p.Name == "Chill Vibes");

        var result = await _repository.GetWithTracksAsync(playlist.PlaylistId);

        result.Should().NotBeNull();
        result!.PlaylistTracks.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldFindPlaylist()
    {
        await SeedPlaylistsAsync();

        var result = await _repository.GetByNameAsync("Workout Mix");

        result.Should().NotBeNull();
        result!.Description.Should().Be("High energy tracks");
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull_WhenNotFound()
    {
        await SeedPlaylistsAsync();

        var result = await _repository.GetByNameAsync("Nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchAsync_ShouldFindByPartialName()
    {
        await SeedPlaylistsAsync();

        var result = (await _repository.SearchAsync("Mix")).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Workout Mix");
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmpty_WhenNoMatch()
    {
        await SeedPlaylistsAsync();

        var result = (await _repository.SearchAsync("Nonexistent")).ToList();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnMultipleMatches()
    {
        await SeedPlaylistsAsync();

        // "i" appears in "Road Trip", "Workout Mix", "Chill Vibes"
        var result = (await _repository.SearchAsync("i")).ToList();

        result.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetRecentAsync_ShouldOrderByUpdatedAtDesc()
    {
        await SeedPlaylistsAsync();

        var result = (await _repository.GetRecentAsync(3)).ToList();

        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Chill Vibes");    // updated now
        result[1].Name.Should().Be("Workout Mix");    // updated 1 day ago
        result[2].Name.Should().Be("Road Trip");      // updated 5 days ago
    }

    [Fact]
    public async Task GetRecentAsync_ShouldRespectCount()
    {
        await SeedPlaylistsAsync();

        var result = (await _repository.GetRecentAsync(1)).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Chill Vibes");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPlaylists()
    {
        await SeedPlaylistsAsync();

        var result = (await _repository.GetAllAsync()).ToList();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistPlaylist()
    {
        var playlist = new Playlist
        {
            Name = "New Playlist",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(playlist);
        await _context.SaveChangesAsync();

        var result = await _context.Playlists.FirstAsync(p => p.Name == "New Playlist");
        result.PlaylistId.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Remove_ShouldDeletePlaylist()
    {
        await SeedPlaylistsAsync();
        var playlist = await _context.Playlists.FirstAsync(p => p.Name == "Road Trip");

        _repository.Remove(playlist);
        await _context.SaveChangesAsync();

        var result = await _context.Playlists.FirstOrDefaultAsync(p => p.Name == "Road Trip");
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldModifyPlaylist()
    {
        await SeedPlaylistsAsync();
        var playlist = await _context.Playlists.FirstAsync(p => p.Name == "Road Trip");

        playlist.Name = "Updated Playlist";
        _repository.Update(playlist);
        await _context.SaveChangesAsync();

        var result = await _context.Playlists.FirstAsync(p => p.Name == "Updated Playlist");
        result.Should().NotBeNull();
    }
}