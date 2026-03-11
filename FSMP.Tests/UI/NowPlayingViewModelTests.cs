using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Interfaces.EventArgs;
using FSMP.Core.Models;
using FSMP.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace FSMP.Tests.UI;

public class NowPlayingViewModelTests
{
    private readonly Mock<IPlaybackController> _playbackMock;
    private readonly Mock<IAudioService> _audioServiceMock;
    private readonly Mock<IAudioPlayer> _playerMock;
    private readonly NowPlayingViewModel _vm;

    public NowPlayingViewModelTests()
    {
        _playbackMock = new Mock<IPlaybackController>();
        _audioServiceMock = new Mock<IAudioService>();
        _playerMock = new Mock<IAudioPlayer>();

        _audioServiceMock.Setup(a => a.Player).Returns(_playerMock.Object);
        _audioServiceMock.Setup(a => a.Volume).Returns(0.75f);
        _playerMock.Setup(p => p.State).Returns(PlaybackState.Stopped);
        _playerMock.Setup(p => p.Position).Returns(TimeSpan.Zero);
        _playerMock.Setup(p => p.Duration).Returns(TimeSpan.Zero);

        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.None);
        _playbackMock.Setup(p => p.IsShuffled).Returns(false);
        _playbackMock.Setup(p => p.GetCurrentTrackAsync())
            .ReturnsAsync(Result.Success<Track?>(null));
        _playbackMock.Setup(p => p.GetQueueItemsAsync(It.IsAny<bool>()))
            .ReturnsAsync(Result.Success(new List<QueueItem>()));

        _vm = new NowPlayingViewModel(
            _playbackMock.Object,
            _audioServiceMock.Object,
            action => action(), // synchronous dispatcher for tests
            async func => await func());
    }

    [Fact]
    public void Constructor_SetsDefaults()
    {
        _vm.TrackTitle.Should().Be("No track loaded");
        _vm.TrackArtist.Should().BeEmpty();
        _vm.TrackAlbum.Should().BeEmpty();
        _vm.PlaybackState.Should().Be(PlaybackState.Stopped);
        _vm.RepeatModeText.Should().Be("Repeat: Off");
        _vm.IsShuffled.Should().BeFalse();
    }

    [Fact]
    public async Task LoadAsync_PopulatesTrackInfo_WhenCurrentTrackExists()
    {
        var track = new Track
        {
            Title = "Test Song",
            Artist = new Artist { Name = "Test Artist" },
            Album = new Album { Title = "Test Album" }
        };
        _playbackMock.Setup(p => p.GetCurrentTrackAsync())
            .ReturnsAsync(Result.Success<Track?>(track));

        await _vm.LoadAsync();

        _vm.TrackTitle.Should().Be("Test Song");
        _vm.TrackArtist.Should().Be("Test Artist");
        _vm.TrackAlbum.Should().Be("Test Album");
    }

    [Fact]
    public async Task LoadAsync_KeepsDefaults_WhenNoCurrentTrack()
    {
        await _vm.LoadAsync();

        _vm.TrackTitle.Should().Be("No track loaded");
        _vm.TrackArtist.Should().BeEmpty();
        _vm.TrackAlbum.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_RefreshesQueueItems()
    {
        var items = new List<QueueItem>
        {
            new() { Index = 0, Title = "Song 1", Artist = "Artist 1", IsCurrent = true },
            new() { Index = 1, Title = "Song 2", Artist = "Artist 2", IsCurrent = false }
        };
        _playbackMock.Setup(p => p.GetQueueItemsAsync(It.IsAny<bool>()))
            .ReturnsAsync(Result.Success(items));

        await _vm.LoadAsync();

        _vm.QueueItems.Should().HaveCount(2);
        _vm.QueueItems[0].Title.Should().Be("Song 1");
    }

    [Fact]
    public void PlayPauseCommand_InvokesTogglePauseAsync()
    {
        _playbackMock.Setup(p => p.TogglePauseAsync()).ReturnsAsync(Result.Success());

        _vm.PlayPauseCommand.Execute(null);

        _playbackMock.Verify(p => p.TogglePauseAsync(), Times.Once);
    }

    [Fact]
    public void NextCommand_InvokesNextTrackAsync()
    {
        _playbackMock.Setup(p => p.NextTrackAsync()).ReturnsAsync(Result.Success());

        _vm.NextCommand.Execute(null);

        _playbackMock.Verify(p => p.NextTrackAsync(), Times.Once);
    }

    [Fact]
    public void PreviousCommand_InvokesPreviousTrackAsync()
    {
        _playbackMock.Setup(p => p.PreviousTrackAsync()).ReturnsAsync(Result.Success());

        _vm.PreviousCommand.Execute(null);

        _playbackMock.Verify(p => p.PreviousTrackAsync(), Times.Once);
    }

    [Fact]
    public void StopCommand_InvokesStopAsync()
    {
        _playbackMock.Setup(p => p.StopAsync()).ReturnsAsync(Result.Success());

        _vm.StopCommand.Execute(null);

        _playbackMock.Verify(p => p.StopAsync(), Times.Once);
    }

    [Fact]
    public void ToggleRepeatCommand_CyclesModeAndUpdatesText()
    {
        _playbackMock.Setup(p => p.ToggleRepeatMode()).Returns(Result.Success());
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.One);

        _vm.ToggleRepeatCommand.Execute(null);

        _vm.RepeatModeText.Should().Be("Repeat: One");
        _playbackMock.Verify(p => p.ToggleRepeatMode(), Times.Once);
    }

    [Fact]
    public void ToggleShuffleCommand_TogglesAndUpdatesProperty()
    {
        _playbackMock.Setup(p => p.ToggleShuffle()).Returns(Result.Success());
        _playbackMock.Setup(p => p.IsShuffled).Returns(true);

        _vm.ToggleShuffleCommand.Execute(null);

        _vm.IsShuffled.Should().BeTrue();
        _playbackMock.Verify(p => p.ToggleShuffle(), Times.Once);
    }

    [Fact]
    public void Volume_Setter_DelegatesToAudioService()
    {
        _vm.Volume = 0.5f;

        _audioServiceMock.VerifySet(a => a.Volume = 0.5f, Times.Once);
    }

    [Fact]
    public void Progress_CalculatesCorrectly()
    {
        // Progress is computed from Position/Duration, which are set via LoadAsync
        // With default zero duration, progress should be 0
        _vm.Progress.Should().Be(0);
    }

    [Fact]
    public async Task LoadAsync_SetsPlaybackStateFromPlayer()
    {
        _playerMock.Setup(p => p.State).Returns(PlaybackState.Playing);
        _playerMock.Setup(p => p.Position).Returns(TimeSpan.FromSeconds(30));
        _playerMock.Setup(p => p.Duration).Returns(TimeSpan.FromMinutes(3));

        await _vm.LoadAsync();

        _vm.PlaybackState.Should().Be(PlaybackState.Playing);
        _vm.PositionText.Should().Be("0:30");
        _vm.DurationText.Should().Be("3:00");
        _vm.Progress.Should().BeApproximately(30.0 / 180.0, 0.001);
    }

    [Fact]
    public async Task StateChangedEvent_UpdatesPlaybackState()
    {
        await _vm.LoadAsync();
        _playerMock.Raise(p => p.StateChanged += null,
            _playerMock.Object,
            new PlaybackStateChangedEventArgs { NewState = PlaybackState.Playing });

        _vm.PlaybackState.Should().Be(PlaybackState.Playing);
    }

    [Fact]
    public async Task PositionChangedEvent_UpdatesPositionAndDuration()
    {
        await _vm.LoadAsync();
        _playerMock.Raise(p => p.PositionChanged += null,
            _playerMock.Object,
            new PositionChangedEventArgs
            {
                Position = TimeSpan.FromSeconds(45),
                Duration = TimeSpan.FromMinutes(4)
            });

        _vm.Position.Should().Be(TimeSpan.FromSeconds(45));
        _vm.Duration.Should().Be(TimeSpan.FromMinutes(4));
    }

    [Fact]
    public async Task LoadAsync_SubscribesToTrackEnd()
    {
        await _vm.LoadAsync();
        _playbackMock.Verify(p => p.SubscribeToTrackEnd(It.IsAny<Action>()), Times.Once);
    }

    [Fact]
    public void JumpToCommand_CallsJumpToAsync()
    {
        var item = new QueueItem { Index = 2, Title = "Track 3", Artist = "Artist" };
        _playbackMock.Setup(p => p.JumpToAsync(2)).ReturnsAsync(Result.Success());

        _vm.JumpToCommand.Execute(item);

        _playbackMock.Verify(p => p.JumpToAsync(2), Times.Once);
    }
}
