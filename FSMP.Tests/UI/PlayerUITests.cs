using FSMP.Core;
using FSMP.Core.Interfaces;
using FSMP.Core.Models;
using FluentAssertions;
using FsmpConsole;
using Moq;

namespace FSMP.Tests.UI;

public class PlayerUITests
{
    private readonly Mock<IPlaybackController> _playbackMock;
    private readonly Mock<IPlaylistManager> _playlistsMock;
    private readonly Mock<ILibraryManager> _libraryMock;
    private readonly Mock<ILibraryBrowser> _browserMock;

    public PlayerUITests()
    {
        _playbackMock = new Mock<IPlaybackController>();
        _playlistsMock = new Mock<IPlaylistManager>();
        _libraryMock = new Mock<ILibraryManager>();
        _browserMock = new Mock<ILibraryBrowser>();

        // Default setups
        _playbackMock.Setup(p => p.GetCurrentTrackAsync()).ReturnsAsync(Result.Success<Track?>(null));
        _playbackMock.Setup(p => p.GetQueueItemsAsync(It.IsAny<bool>())).ReturnsAsync(Result.Success(new List<QueueItem>()));
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.None);
        _playbackMock.Setup(p => p.IsShuffled).Returns(false);
        _playbackMock.Setup(p => p.IsPlaying).Returns(false);
        _playbackMock.Setup(p => p.QueueCount).Returns(0);
    }

    private (PlayerUI player, StringWriter output) CreatePlayerWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var player = new PlayerUI(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, input, output);
        return (player, output);
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullPlayback_ShouldThrow()
    {
        var act = () => new PlayerUI(null!, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("playback");
    }

    [Fact]
    public void Constructor_WithNullPlaylists_ShouldThrow()
    {
        var act = () => new PlayerUI(_playbackMock.Object, null!, _libraryMock.Object, _browserMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("playlists");
    }

    [Fact]
    public void Constructor_WithNullLibrary_ShouldThrow()
    {
        var act = () => new PlayerUI(_playbackMock.Object, _playlistsMock.Object, null!, _browserMock.Object, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("library");
    }

    [Fact]
    public void Constructor_WithNullBrowser_ShouldThrow()
    {
        var act = () => new PlayerUI(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("browser");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new PlayerUI(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new PlayerUI(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, TextReader.Null, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== RunAsync Tests ==========

    [Fact]
    public async Task RunAsync_ExitImmediately_ShouldExitLoop()
    {
        var (player, output) = CreatePlayerWithOutput("X\n");

        await player.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Queue: (empty)");
        text.Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_EmptyInput_ShouldContinueLoop()
    {
        var (player, output) = CreatePlayerWithOutput("\nX\n");

        await player.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Queue: (empty)");
    }

    [Fact]
    public async Task RunAsync_LowercaseX_ShouldExit()
    {
        var (player, output) = CreatePlayerWithOutput("x\n");

        await player.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Queue: (empty)");
        text.Should().Contain("Goodbye!");
    }

    // ========== DisplayPlayerStateAsync Tests ==========

    [Fact]
    public async Task DisplayPlayerStateAsync_EmptyQueue_ShouldShowEmptyQueue()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        var text = output.ToString();
        text.Should().Contain("Queue: (empty)");
        text.Should().Contain("(none)");
        text.Should().Contain("Stopped");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_WithTrack_ShouldShowTrackInfo()
    {
        var track = new Track { TrackId = 1, Title = "Kerala", FilePath = "k.mp3", FileHash = "a",
            Artist = new Artist { Name = "Bonobo" },
            Album = new Album { Title = "Migration" } };
        _playbackMock.Setup(p => p.GetCurrentTrackAsync()).ReturnsAsync(Result.Success<Track?>(track));
        _playbackMock.Setup(p => p.QueueCount).Returns(1);
        _playbackMock.Setup(p => p.GetQueueItemsAsync(It.IsAny<bool>())).ReturnsAsync(Result.Success(new List<QueueItem>
        {
            new() { Index = 0, Title = "Kerala", Artist = "Bonobo", IsCurrent = true }
        }));

        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        var text = output.ToString();
        text.Should().Contain("Kerala");
        text.Should().Contain("Bonobo");
        text.Should().Contain("Migration");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_NotPlaying_ShouldShowStopped()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Stopped");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_WithQueue_ShouldShowQueueItems()
    {
        _playbackMock.Setup(p => p.QueueCount).Returns(2);
        _playbackMock.Setup(p => p.GetQueueItemsAsync(It.IsAny<bool>())).ReturnsAsync(Result.Success(new List<QueueItem>
        {
            new() { Index = 0, Title = "Track1", Artist = "Artist1", IsCurrent = true },
            new() { Index = 1, Title = "Track2", Artist = "Artist2", IsCurrent = false }
        }));

        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        var text = output.ToString();
        text.Should().Contain("Queue (2 tracks):");
        text.Should().Contain("Track1");
        text.Should().Contain("Track2");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_ShouldShowRepeatModeNone()
    {
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.None);
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Repeat: None");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_ShouldShowShuffleOff()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Shuffle: Off");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_ShouldShowControls()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        var text = output.ToString();
        text.Should().Contain("[N] Next");
        text.Should().Contain("[P] Prev");
        text.Should().Contain("[B] Browse");
        text.Should().Contain("[X] Exit");
    }

    // ========== HandleInputAsync — Next (N) ==========

    [Fact]
    public async Task HandleInputAsync_Next_ShouldCallNextTrackAsync()
    {
        _playbackMock.Setup(p => p.NextTrackAsync()).ReturnsAsync(Result.Success());

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("N");

        _playbackMock.Verify(p => p.NextTrackAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Next_AtEndNoRepeat_ShouldShowEndOfQueue()
    {
        _playbackMock.Setup(p => p.NextTrackAsync()).ReturnsAsync(Result.Failure("End of queue."));

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("N");

        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("End of queue");
    }

    [Fact]
    public async Task HandleInputAsync_Next_EmptyQueue_ShouldNotThrow()
    {
        _playbackMock.Setup(p => p.NextTrackAsync()).ReturnsAsync(Result.Failure("End of queue."));

        var (player, output) = CreatePlayerWithOutput("");

        var act = async () => await player.HandleInputAsync("N");

        await act.Should().NotThrowAsync();
    }

    // ========== HandleInputAsync — Previous (P) ==========

    [Fact]
    public async Task HandleInputAsync_Previous_ShouldCallPreviousTrackAsync()
    {
        _playbackMock.Setup(p => p.PreviousTrackAsync()).ReturnsAsync(Result.Success());

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("P");

        _playbackMock.Verify(p => p.PreviousTrackAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Previous_AtBeginning_ShouldShowBeginningOfQueue()
    {
        _playbackMock.Setup(p => p.PreviousTrackAsync()).ReturnsAsync(Result.Failure("Beginning of queue."));

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("P");

        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("Beginning of queue");
    }

    // ========== HandleInputAsync — Play/Stop Toggle (K) ==========

    [Fact]
    public async Task HandleInputAsync_K_ShouldCallTogglePlayStopAsync()
    {
        _playbackMock.Setup(p => p.TogglePlayStopAsync()).ReturnsAsync(Result.Success());

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("K");

        _playbackMock.Verify(p => p.TogglePlayStopAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_K_WhenNotPlaying_NoCurrentTrack_ShouldShowError()
    {
        _playbackMock.Setup(p => p.TogglePlayStopAsync()).ReturnsAsync(Result.Failure("No track selected."));

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("K");

        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("No track selected");
    }

    // ========== HandleInputAsync — Restart (R) ==========

    [Fact]
    public async Task HandleInputAsync_Restart_ShouldCallRestartTrackAsync()
    {
        _playbackMock.Setup(p => p.RestartTrackAsync()).ReturnsAsync(Result.Success());

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("R");

        _playbackMock.Verify(p => p.RestartTrackAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Restart_NoCurrentTrack_ShouldShowError()
    {
        _playbackMock.Setup(p => p.RestartTrackAsync()).ReturnsAsync(Result.Failure("No track selected."));

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("R");

        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("No track selected");
    }

    // ========== HandleInputAsync — Stop (S) ==========

    [Fact]
    public async Task HandleInputAsync_Stop_ShouldCallStopAsync()
    {
        _playbackMock.Setup(p => p.StopAsync()).ReturnsAsync(Result.Success());

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("S");

        _playbackMock.Verify(p => p.StopAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Stop_ShouldShowStopped()
    {
        _playbackMock.Setup(p => p.StopAsync()).ReturnsAsync(Result.Success());

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("S");

        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("Stopped");
    }

    // ========== HandleInputAsync — Repeat Mode (M) ==========

    [Fact]
    public async Task HandleInputAsync_RepeatMode_ShouldCallToggleRepeatMode()
    {
        _playbackMock.Setup(p => p.ToggleRepeatMode()).Returns(Result.Success());
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.One);

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("M");

        _playbackMock.Verify(p => p.ToggleRepeatMode(), Times.Once);
    }

    // ========== HandleInputAsync — Shuffle (H) ==========

    [Fact]
    public async Task HandleInputAsync_Shuffle_ShouldCallToggleShuffle()
    {
        _playbackMock.Setup(p => p.ToggleShuffle()).Returns(Result.Success());
        _playbackMock.Setup(p => p.IsShuffled).Returns(true);

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("H");

        _playbackMock.Verify(p => p.ToggleShuffle(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Shuffle_EmptyQueue_ShouldShowError()
    {
        _playbackMock.Setup(p => p.ToggleShuffle()).Returns(Result.Failure("No tracks in queue to shuffle."));

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("H");

        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("No tracks in queue to shuffle");
    }

    // ========== HandleInputAsync — Unknown Command ==========

    [Fact]
    public async Task HandleInputAsync_UnknownCommand_ShouldShowInvalidSelection()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("Z");

        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("Invalid selection");
    }

    // ========== Queue Display Tests ==========

    [Fact]
    public async Task DisplayPlayerStateAsync_CurrentTrackHighlighted_ShouldShowArrow()
    {
        _playbackMock.Setup(p => p.QueueCount).Returns(2);
        _playbackMock.Setup(p => p.GetQueueItemsAsync(It.IsAny<bool>())).ReturnsAsync(Result.Success(new List<QueueItem>
        {
            new() { Index = 0, Title = "First", Artist = "ArtistA", IsCurrent = true },
            new() { Index = 1, Title = "Second", Artist = "ArtistB", IsCurrent = false }
        }));

        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        var text = output.ToString();
        text.Should().Contain("> 1) First");
        text.Should().Contain("  2) Second");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_TrackWithDuration_ShouldShowDuration()
    {
        _playbackMock.Setup(p => p.QueueCount).Returns(1);
        _playbackMock.Setup(p => p.GetQueueItemsAsync(It.IsAny<bool>())).ReturnsAsync(Result.Success(new List<QueueItem>
        {
            new() { Index = 0, Title = "Kerala", Duration = TimeSpan.FromSeconds(195), IsCurrent = true }
        }));

        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("[3:15]");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_RepeatOne_ShouldShowInStatus()
    {
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.One);
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Repeat: One");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_RepeatAll_ShouldShowInStatus()
    {
        _playbackMock.Setup(p => p.RepeatMode).Returns(RepeatMode.All);
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Repeat: All");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_ShuffleOn_ShouldShowOn()
    {
        _playbackMock.Setup(p => p.IsShuffled).Returns(true);

        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Shuffle: On");
    }

    // ========== HandleInputAsync — Skip to track number ==========

    [Fact]
    public async Task HandleInputAsync_TrackNumber_ShouldJumpAndPlay()
    {
        _playbackMock.Setup(p => p.QueueCount).Returns(3);
        _playbackMock.Setup(p => p.JumpToAsync(2)).ReturnsAsync(Result.Success());

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("3");

        _playbackMock.Verify(p => p.JumpToAsync(2), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_TrackNumber_Zero_ShouldNotJump()
    {
        _playbackMock.Setup(p => p.QueueCount).Returns(1);

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("0");

        _playbackMock.Verify(p => p.JumpToAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleInputAsync_TrackNumber_OutOfRange_ShouldNotJump()
    {
        _playbackMock.Setup(p => p.QueueCount).Returns(1);

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("5");

        _playbackMock.Verify(p => p.JumpToAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleInputAsync_TrackNumber_EmptyQueue_ShouldNotJump()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("1");

        _playbackMock.Verify(p => p.JumpToAsync(It.IsAny<int>()), Times.Never);
    }

    // ========== PlaybackCompleted — Auto-advance ==========

    [Fact]
    public async Task RunAsync_PlaybackCompleted_ShouldCallAutoAdvance()
    {
        _playbackMock.Setup(p => p.AutoAdvanceAsync()).ReturnsAsync(Result.Success());

        // Use a custom input reader that fires the track-ended callback after first read
        var inputSequence = new PlaybackCompletedInputReader(
            new[] { "1", "X" },
            _playbackMock,
            fireCompletedAfterRead: 0);

        // Setup JumpToAsync for the "1" command
        _playbackMock.Setup(p => p.QueueCount).Returns(2);
        _playbackMock.Setup(p => p.JumpToAsync(0)).ReturnsAsync(Result.Success());

        var output = new StringWriter();
        var player = new PlayerUI(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, inputSequence, output);

        await player.RunAsync();

        _playbackMock.Verify(p => p.AutoAdvanceAsync(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunAsync_PlaybackCompleted_AtEndOfQueue_ShouldShowEndMessage()
    {
        _playbackMock.Setup(p => p.AutoAdvanceAsync()).ReturnsAsync(Result.Failure("End of queue."));

        var inputSequence = new PlaybackCompletedInputReader(
            new[] { "1", "X" },
            _playbackMock,
            fireCompletedAfterRead: 0);

        _playbackMock.Setup(p => p.QueueCount).Returns(1);
        _playbackMock.Setup(p => p.JumpToAsync(0)).ReturnsAsync(Result.Success());

        var output = new StringWriter();
        var player = new PlayerUI(_playbackMock.Object, _playlistsMock.Object, _libraryMock.Object, _browserMock.Object, inputSequence, output);

        await player.RunAsync();

        output.ToString().Should().Contain("End of queue.");
    }

    /// <summary>
    /// Custom TextReader that triggers the SubscribeToTrackEnd callback after a specific ReadLine call.
    /// </summary>
    private class PlaybackCompletedInputReader : TextReader
    {
        private readonly string[] _lines;
        private readonly Mock<IPlaybackController> _playbackMock;
        private readonly int _fireAfterRead;
        private int _readCount;
        private Action? _trackEndedCallback;

        public PlaybackCompletedInputReader(string[] lines, Mock<IPlaybackController> playbackMock, int fireCompletedAfterRead)
        {
            _lines = lines;
            _playbackMock = playbackMock;
            _fireAfterRead = fireCompletedAfterRead;

            // Capture the callback when SubscribeToTrackEnd is called
            _playbackMock.Setup(p => p.SubscribeToTrackEnd(It.IsAny<Action>()))
                .Callback<Action>(callback => _trackEndedCallback = callback);
        }

        public override string? ReadLine()
        {
            if (_readCount >= _lines.Length)
                return null;

            var line = _lines[_readCount];
            var currentRead = _readCount;
            _readCount++;

            if (currentRead == _fireAfterRead)
            {
                _trackEndedCallback?.Invoke();
            }

            return line;
        }
    }
}
