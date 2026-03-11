using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Interfaces.EventArgs;
using FSMP.Core.Models;
using FSMP.Core.ViewModels;
using FluentAssertions;
using Moq;

namespace FSMP.Tests.ViewModels;

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

        _vm = new NowPlayingViewModel(
            _playbackMock.Object,
            _audioServiceMock.Object,
            action => action(),
            func => func());
    }

    private async Task SubscribeViaLoadAsync()
    {
        _playbackMock.Setup(p => p.GetCurrentTrackAsync())
            .ReturnsAsync(Result.Failure<Track?>("No track"));
        _playbackMock.Setup(p => p.GetQueueItemsAsync(true))
            .ReturnsAsync(Result.Success(new List<QueueItem>()));
        await _vm.LoadAsync();
    }

    [Fact]
    public void Constructor_SetsDefaults()
    {
        _vm.TrackTitle.Should().Be("No track loaded");
        _vm.TrackArtist.Should().BeEmpty();
        _vm.TrackAlbum.Should().BeEmpty();
        _vm.QueueItems.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_ThrowsOnNullPlaybackController()
    {
        var act = () => new NowPlayingViewModel(
            null!, _audioServiceMock.Object, action => action(), func => func());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullAudioService()
    {
        var act = () => new NowPlayingViewModel(
            _playbackMock.Object, null!, action => action(), func => func());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullDispatchToUI()
    {
        var act = () => new NowPlayingViewModel(
            _playbackMock.Object, _audioServiceMock.Object, null!, func => func());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ThrowsOnNullDispatchToUIAsync()
    {
        var act = () => new NowPlayingViewModel(
            _playbackMock.Object, _audioServiceMock.Object, action => action(), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task LoadAsync_WithCurrentTrack_PopulatesTrackInfo()
    {
        var track = new Track
        {
            Title = "Test Song",
            Artist = new Artist { Name = "Test Artist" },
            Album = new Album { Title = "Test Album" }
        };
        _playbackMock.Setup(p => p.GetCurrentTrackAsync())
            .ReturnsAsync(Result.Success<Track?>(track));
        _playbackMock.Setup(p => p.IsShuffled).Returns(true);
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.All);
        _playbackMock.Setup(p => p.GetQueueItemsAsync(true))
            .ReturnsAsync(Result.Success(new List<QueueItem>()));
        _playerMock.Setup(p => p.State).Returns(PlaybackState.Playing);
        _playerMock.Setup(p => p.Position).Returns(TimeSpan.FromSeconds(30));
        _playerMock.Setup(p => p.Duration).Returns(TimeSpan.FromMinutes(3));

        await _vm.LoadAsync();

        _vm.TrackTitle.Should().Be("Test Song");
        _vm.TrackArtist.Should().Be("Test Artist");
        _vm.TrackAlbum.Should().Be("Test Album");
        _vm.PlaybackState.Should().Be(PlaybackState.Playing);
        _vm.IsShuffled.Should().BeTrue();
        _vm.RepeatModeText.Should().Be("Repeat: All");
    }

    [Fact]
    public async Task LoadAsync_WithNoTrack_ShowsDefaults()
    {
        _playbackMock.Setup(p => p.GetCurrentTrackAsync())
            .ReturnsAsync(Result.Failure<Track?>("No track"));
        _playbackMock.Setup(p => p.GetQueueItemsAsync(true))
            .ReturnsAsync(Result.Success(new List<QueueItem>()));

        await _vm.LoadAsync();

        _vm.TrackTitle.Should().Be("No track loaded");
        _vm.TrackArtist.Should().BeEmpty();
        _vm.TrackAlbum.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadAsync_WithNullTrackFields_ShowsUnknown()
    {
        var track = new Track { Title = null, Artist = null, Album = null };
        _playbackMock.Setup(p => p.GetCurrentTrackAsync())
            .ReturnsAsync(Result.Success<Track?>(track));
        _playbackMock.Setup(p => p.GetQueueItemsAsync(true))
            .ReturnsAsync(Result.Success(new List<QueueItem>()));

        await _vm.LoadAsync();

        _vm.TrackTitle.Should().Be("Unknown Title");
        _vm.TrackArtist.Should().Be("Unknown Artist");
        _vm.TrackAlbum.Should().Be("Unknown Album");
    }

    [Fact]
    public async Task LoadAsync_PopulatesQueueItems()
    {
        var items = new List<QueueItem>
        {
            new() { Index = 0, Title = "Track 1", IsCurrent = true },
            new() { Index = 1, Title = "Track 2", IsCurrent = false }
        };
        _playbackMock.Setup(p => p.GetCurrentTrackAsync())
            .ReturnsAsync(Result.Failure<Track?>("No track"));
        _playbackMock.Setup(p => p.GetQueueItemsAsync(true))
            .ReturnsAsync(Result.Success(items));

        await _vm.LoadAsync();

        _vm.QueueItems.Should().HaveCount(2);
        _vm.QueueItems[0].Title.Should().Be("Track 1");
    }

    [Fact]
    public async Task PlayPauseCommand_CallsTogglePause()
    {
        _playbackMock.Setup(p => p.TogglePauseAsync()).ReturnsAsync(Result.Success());

        _vm.PlayPauseCommand.Execute(null);
        await Task.Delay(50);

        _playbackMock.Verify(p => p.TogglePauseAsync(), Times.Once);
    }

    [Fact]
    public async Task NextCommand_CallsNextTrack()
    {
        _playbackMock.Setup(p => p.NextTrackAsync()).ReturnsAsync(Result.Success());

        _vm.NextCommand.Execute(null);
        await Task.Delay(50);

        _playbackMock.Verify(p => p.NextTrackAsync(), Times.Once);
    }

    [Fact]
    public async Task PreviousCommand_CallsPreviousTrack()
    {
        _playbackMock.Setup(p => p.PreviousTrackAsync()).ReturnsAsync(Result.Success());

        _vm.PreviousCommand.Execute(null);
        await Task.Delay(50);

        _playbackMock.Verify(p => p.PreviousTrackAsync(), Times.Once);
    }

    [Fact]
    public async Task StopCommand_CallsStop()
    {
        _playbackMock.Setup(p => p.StopAsync()).ReturnsAsync(Result.Success());

        _vm.StopCommand.Execute(null);
        await Task.Delay(50);

        _playbackMock.Verify(p => p.StopAsync(), Times.Once);
    }

    [Fact]
    public void ToggleRepeatCommand_CyclesRepeatMode()
    {
        _playbackMock.Setup(p => p.ToggleRepeatMode()).Returns(Result.Success());
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.One);

        _vm.ToggleRepeatCommand.Execute(null);

        _playbackMock.Verify(p => p.ToggleRepeatMode(), Times.Once);
        _vm.RepeatModeText.Should().Be("Repeat: One");
    }

    [Fact]
    public void ToggleShuffleCommand_TogglesShuffle()
    {
        _playbackMock.Setup(p => p.ToggleShuffle()).Returns(Result.Success());
        _playbackMock.Setup(p => p.IsShuffled).Returns(true);

        _vm.ToggleShuffleCommand.Execute(null);

        _playbackMock.Verify(p => p.ToggleShuffle(), Times.Once);
        _vm.IsShuffled.Should().BeTrue();
    }

    [Fact]
    public async Task JumpToCommand_JumpsToQueueIndex()
    {
        var item = new QueueItem { Index = 3, Title = "Track 4" };
        _playbackMock.Setup(p => p.JumpToAsync(3)).ReturnsAsync(Result.Success());

        _vm.JumpToCommand.Execute(item);
        await Task.Delay(50);

        _playbackMock.Verify(p => p.JumpToAsync(3), Times.Once);
    }

    [Fact]
    public async Task JumpToCommand_IgnoresNull()
    {
        _vm.JumpToCommand.Execute(null);
        await Task.Delay(50);

        _playbackMock.Verify(p => p.JumpToAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void Volume_SetUpdatesAudioService()
    {
        _vm.Volume = 0.5f;

        _audioServiceMock.VerifySet(a => a.Volume = 0.5f, Times.Once());
        _vm.Volume.Should().Be(0.5f);
    }

    [Fact]
    public async Task StateChanged_Event_UpdatesPlaybackState()
    {
        await SubscribeViaLoadAsync();
        _playerMock.Raise(p => p.StateChanged += null,
            _playerMock.Object,
            new PlaybackStateChangedEventArgs { NewState = PlaybackState.Playing });

        _vm.PlaybackState.Should().Be(PlaybackState.Playing);
    }

    [Fact]
    public async Task PositionChanged_Event_UpdatesPositionAndDuration()
    {
        await SubscribeViaLoadAsync();
        var pos = TimeSpan.FromSeconds(45);
        var dur = TimeSpan.FromMinutes(3);

        _playerMock.Raise(p => p.PositionChanged += null,
            _playerMock.Object,
            new PositionChangedEventArgs { Position = pos, Duration = dur });

        _vm.Position.Should().Be(pos);
        _vm.Duration.Should().Be(dur);
    }

    [Fact]
    public async Task TrackChanged_Event_UpdatesTrackInfo()
    {
        await SubscribeViaLoadAsync();
        var track = new Track
        {
            Title = "New Song",
            Artist = new Artist { Name = "New Artist" },
            Album = new Album { Title = "New Album" }
        };
        _playbackMock.Setup(p => p.GetQueueItemsAsync(true))
            .ReturnsAsync(Result.Success(new List<QueueItem>()));

        _audioServiceMock.Raise(a => a.TrackChanged += null,
            _audioServiceMock.Object,
            new TrackChangedEventArgs { NewTrack = track });

        await Task.Delay(50);

        _vm.TrackTitle.Should().Be("New Song");
        _vm.TrackArtist.Should().Be("New Artist");
        _vm.TrackAlbum.Should().Be("New Album");
    }

    [Fact]
    public async Task TrackChanged_Event_WithNull_ShowsDefaults()
    {
        _playbackMock.Setup(p => p.GetQueueItemsAsync(true))
            .ReturnsAsync(Result.Success(new List<QueueItem>()));

        _audioServiceMock.Raise(a => a.TrackChanged += null,
            _audioServiceMock.Object,
            new TrackChangedEventArgs { NewTrack = null });

        await Task.Delay(50);

        _vm.TrackTitle.Should().Be("No track loaded");
        _vm.TrackArtist.Should().BeEmpty();
        _vm.TrackAlbum.Should().BeEmpty();
    }

    [Fact]
    public async Task Progress_CalculatesCorrectly()
    {
        await SubscribeViaLoadAsync();
        var pos = TimeSpan.FromSeconds(90);
        var dur = TimeSpan.FromSeconds(180);

        _playerMock.Raise(p => p.PositionChanged += null,
            _playerMock.Object,
            new PositionChangedEventArgs { Position = pos, Duration = dur });

        _vm.Progress.Should().BeApproximately(0.5, 0.001);
    }

    [Fact]
    public void Progress_ZeroDuration_ReturnsZero()
    {
        _vm.Progress.Should().Be(0);
    }

    [Fact]
    public async Task PositionText_FormatsCorrectly()
    {
        await SubscribeViaLoadAsync();
        _playerMock.Raise(p => p.PositionChanged += null,
            _playerMock.Object,
            new PositionChangedEventArgs
            {
                Position = TimeSpan.FromSeconds(125),
                Duration = TimeSpan.FromSeconds(300)
            });

        _vm.PositionText.Should().Be("2:05");
        _vm.DurationText.Should().Be("5:00");
    }

    [Fact]
    public async Task PositionText_WithHours_IncludesHours()
    {
        await SubscribeViaLoadAsync();
        _playerMock.Raise(p => p.PositionChanged += null,
            _playerMock.Object,
            new PositionChangedEventArgs
            {
                Position = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(5)),
                Duration = TimeSpan.FromHours(2)
            });

        _vm.PositionText.Should().Be("1:05:00");
        _vm.DurationText.Should().Be("2:00:00");
    }

    [Fact]
    public async Task UnsubscribeFromEvents_RemovesHandlers()
    {
        await SubscribeViaLoadAsync();
        _vm.UnsubscribeFromEvents();

        // Raise events after unsubscribe — state should not change
        _playerMock.Raise(p => p.StateChanged += null,
            _playerMock.Object,
            new PlaybackStateChangedEventArgs { NewState = PlaybackState.Playing });

        _vm.PlaybackState.Should().Be(PlaybackState.Stopped);
    }

    [Fact]
    public async Task PropertyChanged_RaisedForTrackTitle()
    {
        await SubscribeViaLoadAsync();
        var raised = new List<string>();
        _vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName!);

        _playbackMock.Setup(p => p.GetQueueItemsAsync(true))
            .ReturnsAsync(Result.Success(new List<QueueItem>()));

        _audioServiceMock.Raise(a => a.TrackChanged += null,
            _audioServiceMock.Object,
            new TrackChangedEventArgs
            {
                NewTrack = new Track
                {
                    Title = "Changed",
                    Artist = new Artist { Name = "A" },
                    Album = new Album { Title = "B" }
                }
            });

        await Task.Delay(50);

        raised.Should().Contain("TrackTitle");
        raised.Should().Contain("TrackArtist");
        raised.Should().Contain("TrackAlbum");
    }

    [Fact]
    public void RepeatModeText_AllModes()
    {
        _playbackMock.Setup(p => p.ToggleRepeatMode()).Returns(Result.Success());

        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.None);
        _vm.ToggleRepeatCommand.Execute(null);
        _vm.RepeatModeText.Should().Be("Repeat: Off");

        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.One);
        _vm.ToggleRepeatCommand.Execute(null);
        _vm.RepeatModeText.Should().Be("Repeat: One");

        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.All);
        _vm.ToggleRepeatCommand.Execute(null);
        _vm.RepeatModeText.Should().Be("Repeat: All");
    }

    [Fact]
    public async Task SetProperty_DoesNotRaise_WhenValueUnchanged()
    {
        await SubscribeViaLoadAsync();
        var raised = false;
        _vm.PropertyChanged += (_, _) => raised = true;

        // Volume is already 0.75f from LoadAsync
        _vm.Volume = 0.75f;

        raised.Should().BeFalse();
    }
}
