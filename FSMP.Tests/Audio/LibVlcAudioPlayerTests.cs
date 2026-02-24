using FluentAssertions;
using FSMP.Core.Audio;
using FsmpLibrary.Audio;
using FSMP.Core.Interfaces;
using FSMP.Core.Interfaces.EventArgs;
using FSMP.Tests.TestHelpers;

namespace FSMP.Tests.Audio;

public class LibVlcAudioPlayerTests : IDisposable
{
    private readonly MockMediaPlayerAdapter _mockAdapter;
    private readonly LibVlcAudioPlayer _player;
    private readonly string _tempDir;

    public LibVlcAudioPlayerTests()
    {
        _mockAdapter = new MockMediaPlayerAdapter();
        _player = new LibVlcAudioPlayer(_mockAdapter);
        _tempDir = Path.Combine(Path.GetTempPath(), $"fsmp_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        _player.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTempFile(string name = "test.mp3")
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllBytes(path, new byte[] { 0xFF, 0xFB, 0x90, 0x00 });
        return path;
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullAdapter_ShouldThrowArgumentNullException()
    {
        var act = () => new LibVlcAudioPlayer((IMediaPlayerAdapter)null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("adapter");
    }

    [Fact]
    public void Constructor_ShouldInitializeStateToStopped()
    {
        _player.State.Should().Be(PlaybackState.Stopped);
    }

    [Fact]
    public void Constructor_ShouldSubscribeToAdapterEvents()
    {
        // Verify by triggering adapter event and checking state changes
        _mockAdapter.SimulatePlaying();

        _player.State.Should().Be(PlaybackState.Playing);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void Position_ShouldReturnAdapterTimeMsAsTimeSpan()
    {
        _mockAdapter.TimeMs = 5000;

        _player.Position.Should().Be(TimeSpan.FromMilliseconds(5000));
    }

    [Fact]
    public void Duration_WhenPositive_ShouldReturnAsTimeSpan()
    {
        _mockAdapter.DurationMs = 180000;

        _player.Duration.Should().Be(TimeSpan.FromMilliseconds(180000));
    }

    [Fact]
    public void Duration_WhenZero_ShouldReturnTimeSpanZero()
    {
        _mockAdapter.DurationMs = 0;

        _player.Duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Duration_WhenNegative_ShouldReturnTimeSpanZero()
    {
        _mockAdapter.DurationMs = -1;

        _player.Duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Volume_Get_ShouldReturnAdapterVolumeAsFloat()
    {
        _mockAdapter.Volume = 75;

        _player.Volume.Should().Be(0.75f);
    }

    [Fact]
    public void Volume_Set_ShouldConvertFloatToIntAndSetOnAdapter()
    {
        _player.Volume = 0.5f;

        _mockAdapter.Volume.Should().Be(50);
    }

    [Fact]
    public void Volume_Set_ShouldClampToZero()
    {
        _player.Volume = -0.5f;

        _mockAdapter.Volume.Should().Be(0);
    }

    [Fact]
    public void Volume_Set_ShouldClampToOne()
    {
        _player.Volume = 1.5f;

        _mockAdapter.Volume.Should().Be(100);
    }

    [Fact]
    public void IsMuted_Get_ShouldReturnAdapterMute()
    {
        _mockAdapter.Mute = true;

        _player.IsMuted.Should().BeTrue();
    }

    [Fact]
    public void IsMuted_Set_ShouldSetAdapterMute()
    {
        _player.IsMuted = true;

        _mockAdapter.Mute.Should().BeTrue();
    }

    #endregion

    #region LoadAsync Tests

    [Fact]
    public async Task LoadAsync_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        _player.Dispose();

        var act = () => _player.LoadAsync("test.mp3");

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task LoadAsync_WithNullPath_ShouldThrowArgumentException()
    {
        var act = () => _player.LoadAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public async Task LoadAsync_WithEmptyPath_ShouldThrowArgumentException()
    {
        var act = () => _player.LoadAsync("");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public async Task LoadAsync_WithWhitespacePath_ShouldThrowArgumentException()
    {
        var act = () => _player.LoadAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("filePath");
    }

    [Fact]
    public async Task LoadAsync_WithNonexistentFile_ShouldThrowFileNotFoundException()
    {
        var act = () => _player.LoadAsync(@"C:\nonexistent\file.mp3");

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task LoadAsync_WhenPlaying_ShouldStopBeforeLoading()
    {
        var tempFile = CreateTempFile();
        _mockAdapter.SimulatePlaying();

        await _player.LoadAsync(tempFile);

        _mockAdapter.StopCallCount.Should().Be(1);
    }

    [Fact]
    public async Task LoadAsync_WhenPaused_ShouldStopBeforeLoading()
    {
        var tempFile = CreateTempFile();
        _mockAdapter.SimulatePaused();

        await _player.LoadAsync(tempFile);

        _mockAdapter.StopCallCount.Should().Be(1);
    }

    [Fact]
    public async Task LoadAsync_ShouldDisposeExistingMedia()
    {
        var tempFile = CreateTempFile();

        await _player.LoadAsync(tempFile);

        _mockAdapter.DisposeCurrentMediaCallCount.Should().Be(1);
    }

    [Fact]
    public async Task LoadAsync_ShouldCallAdapterLoadMedia()
    {
        var tempFile = CreateTempFile();

        await _player.LoadAsync(tempFile);

        _mockAdapter.LoadCallCount.Should().Be(1);
        _mockAdapter.LastLoadedFilePath.Should().Be(tempFile);
    }

    [Fact]
    public async Task LoadAsync_ShouldEndInStoppedState()
    {
        var tempFile = CreateTempFile();

        await _player.LoadAsync(tempFile);

        _player.State.Should().Be(PlaybackState.Stopped);
    }

    #endregion

    #region PlayAsync Tests

    [Fact]
    public async Task PlayAsync_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        _player.Dispose();

        var act = () => _player.PlayAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task PlayAsync_WithNoMediaLoaded_ShouldThrowInvalidOperationException()
    {
        _mockAdapter.HasMedia = false;

        var act = () => _player.PlayAsync();

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PlayAsync_ShouldCallAdapterSetMediaAndPlay()
    {
        _mockAdapter.HasMedia = true;

        await _player.PlayAsync();

        _mockAdapter.PlayCallCount.Should().Be(1);
    }

    #endregion

    #region PauseAsync Tests

    [Fact]
    public async Task PauseAsync_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        _player.Dispose();

        var act = () => _player.PauseAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task PauseAsync_ShouldCallAdapterPause()
    {
        await _player.PauseAsync();

        _mockAdapter.PauseCallCount.Should().Be(1);
    }

    #endregion

    #region StopAsync Tests

    [Fact]
    public async Task StopAsync_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        _player.Dispose();

        var act = () => _player.StopAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task StopAsync_ShouldCallAdapterStop()
    {
        await _player.StopAsync();

        _mockAdapter.StopCallCount.Should().Be(1);
    }

    #endregion

    #region SeekAsync Tests

    [Fact]
    public async Task SeekAsync_WhenDisposed_ShouldThrowObjectDisposedException()
    {
        _player.Dispose();

        var act = () => _player.SeekAsync(TimeSpan.FromSeconds(30));

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task SeekAsync_ShouldSetAdapterTimeMs()
    {
        var position = TimeSpan.FromSeconds(30);

        await _player.SeekAsync(position);

        _mockAdapter.TimeMs.Should().Be(30000);
    }

    [Fact]
    public async Task SeekAsync_WithZeroPosition_ShouldSetToZero()
    {
        await _player.SeekAsync(TimeSpan.Zero);

        _mockAdapter.TimeMs.Should().Be(0);
    }

    #endregion

    #region Event Handler Tests

    [Fact]
    public void OnPlaying_ShouldSetStateToPlaying()
    {
        _mockAdapter.SimulatePlaying();

        _player.State.Should().Be(PlaybackState.Playing);
    }

    [Fact]
    public void OnPlaying_ShouldRaiseStateChangedEvent()
    {
        PlaybackStateChangedEventArgs? received = null;
        _player.StateChanged += (s, e) => received = e;

        _mockAdapter.SimulatePlaying();

        received.Should().NotBeNull();
        received!.OldState.Should().Be(PlaybackState.Stopped);
        received.NewState.Should().Be(PlaybackState.Playing);
    }

    [Fact]
    public void OnPaused_ShouldSetStateToPaused()
    {
        _mockAdapter.SimulatePaused();

        _player.State.Should().Be(PlaybackState.Paused);
    }

    [Fact]
    public void OnStopped_ShouldSetStateToStopped()
    {
        _mockAdapter.SimulatePlaying(); // Change from default Stopped first
        _mockAdapter.SimulateStopped();

        _player.State.Should().Be(PlaybackState.Stopped);
    }

    [Fact]
    public void SetState_WhenStateUnchanged_ShouldNotRaiseEvent()
    {
        int eventCount = 0;
        _player.StateChanged += (s, e) => eventCount++;

        // State is already Stopped, simulating Stopped should not raise event
        _mockAdapter.SimulateStopped();

        eventCount.Should().Be(0);
    }

    [Fact]
    public void OnEndReached_ShouldSetStateToStopped()
    {
        _mockAdapter.SimulatePlaying(); // Change from default Stopped
        _mockAdapter.SimulateEndReached();

        _player.State.Should().Be(PlaybackState.Stopped);
    }

    [Fact]
    public void OnEndReached_ShouldRaisePlaybackCompletedEvent()
    {
        PlaybackCompletedEventArgs? received = null;
        _player.PlaybackCompleted += (s, e) => received = e;

        _mockAdapter.SimulateEndReached();

        received.Should().NotBeNull();
        received!.CompletedSuccessfully.Should().BeTrue();
    }

    [Fact]
    public async Task OnEndReached_ShouldIncludeFilePathInEvent()
    {
        var tempFile = CreateTempFile();
        await _player.LoadAsync(tempFile);

        PlaybackCompletedEventArgs? received = null;
        _player.PlaybackCompleted += (s, e) => received = e;

        _mockAdapter.SimulateEndReached();

        received.Should().NotBeNull();
        received!.FilePath.Should().Be(tempFile);
    }

    [Fact]
    public void OnError_ShouldSetStateToError()
    {
        _mockAdapter.SimulateError();

        _player.State.Should().Be(PlaybackState.Error);
    }

    [Fact]
    public void OnError_ShouldRaisePlaybackErrorEvent()
    {
        PlaybackErrorEventArgs? received = null;
        _player.PlaybackError += (s, e) => received = e;

        _mockAdapter.SimulateError();

        received.Should().NotBeNull();
        received!.ErrorMessage.Should().Contain("LibVLC");
    }

    [Fact]
    public async Task OnError_ShouldIncludeFilePathInEvent()
    {
        var tempFile = CreateTempFile();
        await _player.LoadAsync(tempFile);

        PlaybackErrorEventArgs? received = null;
        _player.PlaybackError += (s, e) => received = e;

        _mockAdapter.SimulateError();

        received.Should().NotBeNull();
        received!.FilePath.Should().Be(tempFile);
    }

    [Fact]
    public void OnTimeChanged_ShouldRaisePositionChangedEvent()
    {
        PositionChangedEventArgs? received = null;
        _player.PositionChanged += (s, e) => received = e;

        _mockAdapter.SimulateTimeChanged(5000);

        received.Should().NotBeNull();
        received!.Position.Should().Be(TimeSpan.FromMilliseconds(5000));
    }

    [Fact]
    public void OnTimeChanged_ShouldIncludeCorrectDuration()
    {
        _mockAdapter.DurationMs = 180000;
        PositionChangedEventArgs? received = null;
        _player.PositionChanged += (s, e) => received = e;

        _mockAdapter.SimulateTimeChanged(5000);

        received.Should().NotBeNull();
        received!.Duration.Should().Be(TimeSpan.FromMilliseconds(180000));
    }

    #endregion

    #region Dispose Tests

    [Fact]
    public void Dispose_ShouldDisposeAdapter()
    {
        _player.Dispose();

        _mockAdapter.IsDisposed.Should().BeTrue();
    }

    [Fact]
    public void Dispose_WhenCalledMultipleTimes_ShouldNotThrow()
    {
        var act = () =>
        {
            _player.Dispose();
            _player.Dispose();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromAdapterEvents()
    {
        _player.Dispose();

        // After dispose, simulating events should not change state
        var stateBefore = _player.State;
        _mockAdapter.SimulatePlaying();

        _player.State.Should().Be(stateBefore);
    }

    [Fact]
    public void AfterDispose_AllMethodsShouldThrowObjectDisposedException()
    {
        _player.Dispose();

        var actLoad = () => _player.LoadAsync("test.mp3");
        var actPlay = () => _player.PlayAsync();
        var actPause = () => _player.PauseAsync();
        var actStop = () => _player.StopAsync();
        var actSeek = () => _player.SeekAsync(TimeSpan.Zero);

        actLoad.Should().ThrowAsync<ObjectDisposedException>();
        actPlay.Should().ThrowAsync<ObjectDisposedException>();
        actPause.Should().ThrowAsync<ObjectDisposedException>();
        actStop.Should().ThrowAsync<ObjectDisposedException>();
        actSeek.Should().ThrowAsync<ObjectDisposedException>();
    }

    #endregion
}
