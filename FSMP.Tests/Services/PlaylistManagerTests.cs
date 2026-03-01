using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FsmpLibrary.Services;
using Moq;
using FluentAssertions;

namespace FSMP.Tests.Services;

public class PlaylistManagerTests
{
    private readonly Mock<IPlaylistService> _playlistServiceMock;
    private readonly ActivePlaylistService _activePlaylist;
    private readonly PlaylistManager _manager;

    public PlaylistManagerTests()
    {
        _playlistServiceMock = new Mock<IPlaylistService>();
        _activePlaylist = new ActivePlaylistService();
        _manager = new PlaylistManager(_playlistServiceMock.Object, _activePlaylist);
    }

    [Fact]
    public void Constructor_ThrowsOnNullPlaylistService()
    {
        var act = () => new PlaylistManager(null!, _activePlaylist);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullActivePlaylist()
    {
        var act = () => new PlaylistManager(_playlistServiceMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAllPlaylistsAsync_ReturnsPlaylists()
    {
        var playlists = new List<Playlist> { new() { PlaylistId = 1, Name = "Test" } };
        _playlistServiceMock.Setup(s => s.GetAllPlaylistsAsync()).ReturnsAsync(playlists);

        var result = await _manager.GetAllPlaylistsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetAllPlaylistsAsync_ReturnsFailure_OnException()
    {
        _playlistServiceMock.Setup(s => s.GetAllPlaylistsAsync()).ThrowsAsync(new Exception("fail"));

        var result = await _manager.GetAllPlaylistsAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetPlaylistWithTracksAsync_ReturnsPlaylist()
    {
        var playlist = new Playlist { PlaylistId = 1, Name = "Test" };
        _playlistServiceMock.Setup(s => s.GetPlaylistWithTracksAsync(1)).ReturnsAsync(playlist);

        var result = await _manager.GetPlaylistWithTracksAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Test");
    }

    [Fact]
    public async Task CreatePlaylistAsync_ReturnsCreatedPlaylist()
    {
        var playlist = new Playlist { PlaylistId = 1, Name = "New" };
        _playlistServiceMock.Setup(s => s.CreatePlaylistAsync("New", null)).ReturnsAsync(playlist);

        var result = await _manager.CreatePlaylistAsync("New");

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("New");
    }

    [Fact]
    public async Task CreatePlaylistAsync_ReturnsFailure_OnException()
    {
        _playlistServiceMock.Setup(s => s.CreatePlaylistAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ThrowsAsync(new ArgumentException("empty name"));

        var result = await _manager.CreatePlaylistAsync("");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeletePlaylistAsync_ReturnsSuccess()
    {
        var result = await _manager.DeletePlaylistAsync(1);

        result.IsSuccess.Should().BeTrue();
        _playlistServiceMock.Verify(s => s.DeletePlaylistAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeletePlaylistAsync_ReturnsFailure_OnException()
    {
        _playlistServiceMock.Setup(s => s.DeletePlaylistAsync(99))
            .ThrowsAsync(new InvalidOperationException("not found"));

        var result = await _manager.DeletePlaylistAsync(99);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task LoadPlaylistIntoQueueAsync_SetsQueue()
    {
        var playlist = new Playlist
        {
            PlaylistId = 1,
            Name = "Test",
            PlaylistTracks = new List<PlaylistTrack>
            {
                new() { TrackId = 10, Position = 0 },
                new() { TrackId = 20, Position = 1 }
            }
        };
        _playlistServiceMock.Setup(s => s.GetPlaylistWithTracksAsync(1)).ReturnsAsync(playlist);

        var result = await _manager.LoadPlaylistIntoQueueAsync(1);

        result.IsSuccess.Should().BeTrue();
        _activePlaylist.Count.Should().Be(2);
        _activePlaylist.PlayOrder.Should().BeEquivalentTo(new[] { 10, 20 });
    }

    [Fact]
    public async Task LoadPlaylistIntoQueueAsync_ReturnsFailure_WhenNotFound()
    {
        _playlistServiceMock.Setup(s => s.GetPlaylistWithTracksAsync(99)).ReturnsAsync((Playlist?)null);

        var result = await _manager.LoadPlaylistIntoQueueAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task LoadPlaylistIntoQueueAsync_ReturnsFailure_WhenEmpty()
    {
        var playlist = new Playlist
        {
            PlaylistId = 1,
            Name = "Empty",
            PlaylistTracks = new List<PlaylistTrack>()
        };
        _playlistServiceMock.Setup(s => s.GetPlaylistWithTracksAsync(1)).ReturnsAsync(playlist);

        var result = await _manager.LoadPlaylistIntoQueueAsync(1);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("No tracks");
    }
}
