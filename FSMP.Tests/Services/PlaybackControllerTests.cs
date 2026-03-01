using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FsmpLibrary.Services;
using Moq;
using FluentAssertions;
using FSMP.Tests.TestHelpers;

namespace FSMP.Tests.Services;

public class PlaybackControllerTests
{
    private readonly Mock<IAudioService> _audioServiceMock;
    private readonly Mock<IActivePlaylistService> _activePlaylistMock;
    private readonly Mock<ITrackRepository> _trackRepoMock;
    private readonly PlaybackController _controller;
    private readonly MockAudioPlayer _mockPlayer;

    public PlaybackControllerTests()
    {
        _audioServiceMock = new Mock<IAudioService>();
        _mockPlayer = new MockAudioPlayer();
        _audioServiceMock.Setup(a => a.Player).Returns(_mockPlayer);
        _activePlaylistMock = new Mock<IActivePlaylistService>();
        _trackRepoMock = new Mock<ITrackRepository>();
        _controller = new PlaybackController(_audioServiceMock.Object, _activePlaylistMock.Object, _trackRepoMock.Object);
    }

    [Fact]
    public void Constructor_ThrowsOnNullAudioService()
    {
        var act = () => new PlaybackController(null!, _activePlaylistMock.Object, _trackRepoMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullActivePlaylist()
    {
        var act = () => new PlaybackController(_audioServiceMock.Object, (IActivePlaylistService)null!, _trackRepoMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullTrackRepository()
    {
        var act = () => new PlaybackController(_audioServiceMock.Object, _activePlaylistMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsPlaying_ReturnsFalse_WhenStopped()
    {
        _controller.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public void QueueCount_ReturnsZero_WhenEmpty()
    {
        _activePlaylistMock.Setup(p => p.Count).Returns(0);
        _controller.QueueCount.Should().Be(0);
    }

    [Fact]
    public async Task PlayTrackByIdAsync_ReturnsFailure_WhenTrackNotFound()
    {
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Track?)null);

        var result = await _controller.PlayTrackByIdAsync(1);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task PlayTrackByIdAsync_ReturnsSuccess_WhenTrackFound()
    {
        var track = new Track { TrackId = 1, Title = "Test", FilePath = "test.mp3", FileHash = "abc" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);

        var result = await _controller.PlayTrackByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        _audioServiceMock.Verify(a => a.PlayTrackAsync(track, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PlayTrackByIdAsync_ReturnsFailure_WhenPlaybackThrows()
    {
        var track = new Track { TrackId = 1, Title = "Test", FilePath = "test.mp3", FileHash = "abc" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);
        _audioServiceMock.Setup(a => a.PlayTrackAsync(track, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("fail"));

        var result = await _controller.PlayTrackByIdAsync(1);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("fail");
    }

    [Fact]
    public async Task NextTrackAsync_ReturnsFailure_WhenQueueEmpty()
    {
        _activePlaylistMock.Setup(p => p.MoveNext()).Returns((int?)null);

        var result = await _controller.NextTrackAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("End of queue");
    }

    [Fact]
    public async Task NextTrackAsync_PlaysNextTrack()
    {
        var track2 = new Track { TrackId = 2, Title = "T2", FilePath = "t2.mp3", FileHash = "b" };
        _activePlaylistMock.Setup(p => p.MoveNext()).Returns(2);
        _trackRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(track2);

        var result = await _controller.NextTrackAsync();

        result.IsSuccess.Should().BeTrue();
        _audioServiceMock.Verify(a => a.PlayTrackAsync(track2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PreviousTrackAsync_ReturnsFailure_WhenAtStart()
    {
        _activePlaylistMock.Setup(p => p.MovePrevious()).Returns((int?)null);

        var result = await _controller.PreviousTrackAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Beginning of queue");
    }

    [Fact]
    public async Task TogglePlayStopAsync_StopsWhenPlaying()
    {
        _mockPlayer.SetState(PlaybackState.Playing);

        var result = await _controller.TogglePlayStopAsync();

        result.IsSuccess.Should().BeTrue();
        _audioServiceMock.Verify(a => a.StopAsync(), Times.Once);
    }

    [Fact]
    public async Task TogglePlayStopAsync_PlaysCurrentTrackWhenStopped()
    {
        var track = new Track { TrackId = 1, Title = "T", FilePath = "t.mp3", FileHash = "a" };
        _activePlaylistMock.Setup(p => p.CurrentTrackId).Returns(1);
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);

        var result = await _controller.TogglePlayStopAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TogglePlayStopAsync_ReturnsFailure_WhenNoTrackSelected()
    {
        _activePlaylistMock.Setup(p => p.CurrentTrackId).Returns((int?)null);

        var result = await _controller.TogglePlayStopAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RestartTrackAsync_ReturnsSuccess_WhenTrackSelected()
    {
        _activePlaylistMock.Setup(p => p.CurrentTrackId).Returns(1);

        var result = await _controller.RestartTrackAsync();

        result.IsSuccess.Should().BeTrue();
        _audioServiceMock.Verify(a => a.SeekAsync(TimeSpan.Zero), Times.Once);
        _audioServiceMock.Verify(a => a.ResumeAsync(), Times.Once);
    }

    [Fact]
    public async Task RestartTrackAsync_ReturnsFailure_WhenNoTrackSelected()
    {
        _activePlaylistMock.Setup(p => p.CurrentTrackId).Returns((int?)null);

        var result = await _controller.RestartTrackAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task StopAsync_ReturnsSuccess()
    {
        var result = await _controller.StopAsync();

        result.IsSuccess.Should().BeTrue();
        _audioServiceMock.Verify(a => a.StopAsync(), Times.Once);
    }

    [Fact]
    public void ToggleRepeatMode_FromNone_ShouldSetToOne()
    {
        _activePlaylistMock.SetupProperty(p => p.RepeatMode, RepeatMode.None);

        _controller.ToggleRepeatMode();

        _activePlaylistMock.Object.RepeatMode.Should().Be(RepeatMode.One);
    }

    [Fact]
    public void ToggleRepeatMode_FromOne_ShouldSetToAll()
    {
        _activePlaylistMock.SetupProperty(p => p.RepeatMode, RepeatMode.One);

        _controller.ToggleRepeatMode();

        _activePlaylistMock.Object.RepeatMode.Should().Be(RepeatMode.All);
    }

    [Fact]
    public void ToggleRepeatMode_FromAll_ShouldSetToNone()
    {
        _activePlaylistMock.SetupProperty(p => p.RepeatMode, RepeatMode.All);

        _controller.ToggleRepeatMode();

        _activePlaylistMock.Object.RepeatMode.Should().Be(RepeatMode.None);
    }

    [Fact]
    public void ToggleShuffle_ReturnsFailure_WhenQueueEmpty()
    {
        _activePlaylistMock.Setup(p => p.Count).Returns(0);

        var result = _controller.ToggleShuffle();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ToggleShuffle_ReturnsSuccess_WhenQueueNotEmpty()
    {
        _activePlaylistMock.Setup(p => p.Count).Returns(3);
        _activePlaylistMock.Setup(p => p.IsShuffled).Returns(true);

        var result = _controller.ToggleShuffle();
        result.IsSuccess.Should().BeTrue();
        _activePlaylistMock.Verify(p => p.ToggleShuffle(), Times.Once);
    }

    [Fact]
    public async Task JumpToAsync_ReturnsSuccess_WhenValidIndex()
    {
        var track = new Track { TrackId = 2, Title = "T2", FilePath = "t2.mp3", FileHash = "b" };
        _activePlaylistMock.Setup(p => p.CurrentTrackId).Returns(2);
        _trackRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(track);

        var result = await _controller.JumpToAsync(1);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task JumpToAsync_ReturnsFailure_WhenInvalidIndex()
    {
        _activePlaylistMock.Setup(p => p.JumpTo(99)).Throws(new ArgumentOutOfRangeException());

        var result = await _controller.JumpToAsync(99);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrentTrackAsync_ReturnsNull_WhenNoTrack()
    {
        _activePlaylistMock.Setup(p => p.CurrentTrackId).Returns((int?)null);

        var result = await _controller.GetCurrentTrackAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentTrackAsync_ReturnsTrack_WhenQueueHasTrack()
    {
        var track = new Track { TrackId = 1, Title = "T", FilePath = "t.mp3", FileHash = "a" };
        _activePlaylistMock.Setup(p => p.CurrentTrackId).Returns(1);
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);

        var result = await _controller.GetCurrentTrackAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TrackId.Should().Be(1);
    }

    [Fact]
    public async Task GetQueueItemsAsync_ReturnsEmptyList_WhenQueueEmpty()
    {
        _activePlaylistMock.Setup(p => p.PlayOrder).Returns(new List<int>().AsReadOnly());
        _activePlaylistMock.Setup(p => p.CurrentIndex).Returns(-1);

        var result = await _controller.GetQueueItemsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQueueItemsAsync_ReturnsItems_WhenQueueHasTracks()
    {
        var track = new Track { TrackId = 1, Title = "T", FilePath = "t.mp3", FileHash = "a" };
        _activePlaylistMock.Setup(p => p.PlayOrder).Returns(new List<int> { 1 }.AsReadOnly());
        _activePlaylistMock.Setup(p => p.CurrentIndex).Returns(0);
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);

        var result = await _controller.GetQueueItemsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].IsCurrent.Should().BeTrue();
    }

    [Fact]
    public async Task AutoAdvanceAsync_ReturnsFailure_WhenNoNext()
    {
        _activePlaylistMock.Setup(p => p.MoveNext()).Returns((int?)null);

        var result = await _controller.AutoAdvanceAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AutoAdvanceAsync_PlaysNext_WhenAvailable()
    {
        var track = new Track { TrackId = 2, Title = "T2", FilePath = "t2.mp3", FileHash = "b" };
        _activePlaylistMock.Setup(p => p.MoveNext()).Returns(2);
        _trackRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(track);

        var result = await _controller.AutoAdvanceAsync();

        result.IsSuccess.Should().BeTrue();
    }
}
