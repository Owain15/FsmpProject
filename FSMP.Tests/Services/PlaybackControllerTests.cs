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
    private readonly ActivePlaylistService _activePlaylist;
    private readonly Mock<ITrackRepository> _trackRepoMock;
    private readonly PlaybackController _controller;
    private readonly MockAudioPlayer _mockPlayer;

    public PlaybackControllerTests()
    {
        _audioServiceMock = new Mock<IAudioService>();
        _mockPlayer = new MockAudioPlayer();
        _audioServiceMock.Setup(a => a.Player).Returns(_mockPlayer);
        _activePlaylist = new ActivePlaylistService();
        _trackRepoMock = new Mock<ITrackRepository>();
        _controller = new PlaybackController(_audioServiceMock.Object, _activePlaylist, _trackRepoMock.Object);
    }

    [Fact]
    public void Constructor_ThrowsOnNullAudioService()
    {
        var act = () => new PlaybackController(null!, _activePlaylist, _trackRepoMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullActivePlaylist()
    {
        var act = () => new PlaybackController(_audioServiceMock.Object, null!, _trackRepoMock.Object);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullTrackRepository()
    {
        var act = () => new PlaybackController(_audioServiceMock.Object, _activePlaylist, null!);
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
        var result = await _controller.NextTrackAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("End of queue");
    }

    [Fact]
    public async Task NextTrackAsync_PlaysNextTrack()
    {
        var track1 = new Track { TrackId = 1, Title = "T1", FilePath = "t1.mp3", FileHash = "a" };
        var track2 = new Track { TrackId = 2, Title = "T2", FilePath = "t2.mp3", FileHash = "b" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track1);
        _trackRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(track2);
        _activePlaylist.SetQueue(new[] { 1, 2 });

        var result = await _controller.NextTrackAsync();

        result.IsSuccess.Should().BeTrue();
        _audioServiceMock.Verify(a => a.PlayTrackAsync(track2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PreviousTrackAsync_ReturnsFailure_WhenAtStart()
    {
        var result = await _controller.PreviousTrackAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Beginning of queue");
    }

    [Fact]
    public async Task TogglePlayStopAsync_StopsWhenPlaying()
    {
        // Put player into Playing state by calling PlayAsync
        await _mockPlayer.PlayAsync();
        _mockPlayer.State.Should().Be(PlaybackState.Playing);

        var result = await _controller.TogglePlayStopAsync();

        result.IsSuccess.Should().BeTrue();
        _audioServiceMock.Verify(a => a.StopAsync(), Times.Once);
    }

    [Fact]
    public async Task TogglePlayStopAsync_PlaysCurrentTrackWhenStopped()
    {
        var track = new Track { TrackId = 1, Title = "T", FilePath = "t.mp3", FileHash = "a" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);
        _activePlaylist.SetQueue(new[] { 1 });

        var result = await _controller.TogglePlayStopAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TogglePlayStopAsync_ReturnsFailure_WhenNoTrackSelected()
    {
        var result = await _controller.TogglePlayStopAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RestartTrackAsync_ReturnsSuccess_WhenTrackSelected()
    {
        _activePlaylist.SetQueue(new[] { 1 });

        var result = await _controller.RestartTrackAsync();

        result.IsSuccess.Should().BeTrue();
        _audioServiceMock.Verify(a => a.SeekAsync(TimeSpan.Zero), Times.Once);
        _audioServiceMock.Verify(a => a.ResumeAsync(), Times.Once);
    }

    [Fact]
    public async Task RestartTrackAsync_ReturnsFailure_WhenNoTrackSelected()
    {
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
    public void ToggleRepeatMode_CyclesThroughModes()
    {
        _controller.RepeatMode.Should().Be(RepeatMode.None);

        _controller.ToggleRepeatMode();
        _controller.RepeatMode.Should().Be(RepeatMode.One);

        _controller.ToggleRepeatMode();
        _controller.RepeatMode.Should().Be(RepeatMode.All);

        _controller.ToggleRepeatMode();
        _controller.RepeatMode.Should().Be(RepeatMode.None);
    }

    [Fact]
    public void ToggleShuffle_ReturnsFailure_WhenQueueEmpty()
    {
        var result = _controller.ToggleShuffle();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ToggleShuffle_ReturnsSuccess_WhenQueueNotEmpty()
    {
        _activePlaylist.SetQueue(new[] { 1, 2, 3 });
        var result = _controller.ToggleShuffle();
        result.IsSuccess.Should().BeTrue();
        _controller.IsShuffled.Should().BeTrue();
    }

    [Fact]
    public async Task JumpToAsync_ReturnsSuccess_WhenValidIndex()
    {
        var track = new Track { TrackId = 2, Title = "T2", FilePath = "t2.mp3", FileHash = "b" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(track);
        _activePlaylist.SetQueue(new[] { 1, 2, 3 });

        var result = await _controller.JumpToAsync(1);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task JumpToAsync_ReturnsFailure_WhenInvalidIndex()
    {
        _activePlaylist.SetQueue(new[] { 1 });

        var result = await _controller.JumpToAsync(99);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetCurrentTrackAsync_ReturnsNull_WhenNoTrack()
    {
        var result = await _controller.GetCurrentTrackAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetCurrentTrackAsync_ReturnsTrack_WhenQueueHasTrack()
    {
        var track = new Track { TrackId = 1, Title = "T", FilePath = "t.mp3", FileHash = "a" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);
        _activePlaylist.SetQueue(new[] { 1 });

        var result = await _controller.GetCurrentTrackAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.TrackId.Should().Be(1);
    }

    [Fact]
    public async Task GetQueueItemsAsync_ReturnsEmptyList_WhenQueueEmpty()
    {
        var result = await _controller.GetQueueItemsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetQueueItemsAsync_ReturnsItems_WhenQueueHasTracks()
    {
        var track = new Track { TrackId = 1, Title = "T", FilePath = "t.mp3", FileHash = "a" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(track);
        _activePlaylist.SetQueue(new[] { 1 });

        var result = await _controller.GetQueueItemsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value![0].IsCurrent.Should().BeTrue();
    }

    [Fact]
    public async Task AutoAdvanceAsync_ReturnsFailure_WhenNoNext()
    {
        var result = await _controller.AutoAdvanceAsync();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AutoAdvanceAsync_PlaysNext_WhenAvailable()
    {
        var track = new Track { TrackId = 2, Title = "T2", FilePath = "t2.mp3", FileHash = "b" };
        _trackRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(track);
        _activePlaylist.SetQueue(new[] { 1, 2 });

        var result = await _controller.AutoAdvanceAsync();

        result.IsSuccess.Should().BeTrue();
    }
}
