using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FSMP.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace FSMP.Tests.ViewModels;

public class PlaylistsViewModelTests
{
    private readonly Mock<IPlaylistManager> _playlistManagerMock;
    private readonly PlaylistsViewModel _vm;

    public PlaylistsViewModelTests()
    {
        _playlistManagerMock = new Mock<IPlaylistManager>();
        _vm = new PlaylistsViewModel(_playlistManagerMock.Object);
    }

    [Fact]
    public async Task LoadAsync_PopulatesPlaylists()
    {
        var playlists = new List<Playlist>
        {
            new() { PlaylistId = 1, Name = "Rock" },
            new() { PlaylistId = 2, Name = "Chill" }
        };
        _playlistManagerMock.Setup(m => m.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(playlists));

        await _vm.LoadAsync();

        _vm.Playlists.Should().HaveCount(2);
        _vm.Playlists[0].Name.Should().Be("Rock");
        _vm.Playlists[1].Name.Should().Be("Chill");
    }

    [Fact]
    public async Task CreatePlaylistCommand_CreatesAndRefreshes()
    {
        var created = new Playlist { PlaylistId = 1, Name = "New" };
        _playlistManagerMock.Setup(m => m.CreatePlaylistAsync("New", null))
            .ReturnsAsync(Result.Success(created));
        _playlistManagerMock.Setup(m => m.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist> { created }));

        _vm.CreatePlaylistCommand.Execute("New");
        await Task.Delay(50);

        _playlistManagerMock.Verify(m => m.CreatePlaylistAsync("New", null), Times.Once);
        _vm.Playlists.Should().HaveCount(1);
        _vm.StatusMessage.Should().Contain("Created");
    }

    [Fact]
    public async Task CreatePlaylistCommand_IgnoresEmptyName()
    {
        _vm.CreatePlaylistCommand.Execute("");
        await Task.Delay(50);

        _playlistManagerMock.Verify(m => m.CreatePlaylistAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task DeletePlaylistCommand_DeletesAndRefreshes()
    {
        var playlist = new Playlist { PlaylistId = 1, Name = "Old" };
        _playlistManagerMock.Setup(m => m.DeletePlaylistAsync(1))
            .ReturnsAsync(Result.Success());
        _playlistManagerMock.Setup(m => m.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist>()));

        _vm.DeletePlaylistCommand.Execute(playlist);
        await Task.Delay(50);

        _playlistManagerMock.Verify(m => m.DeletePlaylistAsync(1), Times.Once);
        _vm.Playlists.Should().BeEmpty();
        _vm.StatusMessage.Should().Contain("Deleted");
    }

    [Fact]
    public async Task LoadIntoQueueCommand_CallsPlaylistManager()
    {
        var playlist = new Playlist { PlaylistId = 5, Name = "Party" };
        _playlistManagerMock.Setup(m => m.LoadPlaylistIntoQueueAsync(5))
            .ReturnsAsync(Result.Success());

        _vm.LoadIntoQueueCommand.Execute(playlist);
        await Task.Delay(50);

        _playlistManagerMock.Verify(m => m.LoadPlaylistIntoQueueAsync(5), Times.Once);
        _vm.StatusMessage.Should().Contain("Loaded");
    }

    [Fact]
    public async Task LoadIntoQueueCommand_ShowsErrorOnFailure()
    {
        var playlist = new Playlist { PlaylistId = 5, Name = "Party" };
        _playlistManagerMock.Setup(m => m.LoadPlaylistIntoQueueAsync(5))
            .ReturnsAsync(Result.Failure("Playlist is empty"));

        _vm.LoadIntoQueueCommand.Execute(playlist);
        await Task.Delay(50);

        _vm.StatusMessage.Should().Contain("Failed");
    }

    [Fact]
    public void Constructor_ThrowsOnNull()
    {
        var act = () => new PlaylistsViewModel(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task CreatePlaylistCommand_ShowsErrorOnFailure()
    {
        _playlistManagerMock.Setup(m => m.CreatePlaylistAsync("Dup", null))
            .ReturnsAsync(Result.Failure<Playlist>("Already exists"));

        _vm.CreatePlaylistCommand.Execute("Dup");
        await Task.Delay(50);

        _vm.StatusMessage.Should().Contain("Failed");
    }
}
