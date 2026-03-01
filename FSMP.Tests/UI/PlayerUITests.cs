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

    // ========== ManagePlaylistsAsync Tests (L key) ==========

    [Fact]
    public async Task ManagePlaylists_ShouldCallGetAllPlaylistsAsync()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist>()));

        var (player, output) = CreatePlayerWithOutput("0\n");
        await player.HandleInputAsync("L");

        _playlistsMock.Verify(p => p.GetAllPlaylistsAsync(), Times.Once);
    }

    [Fact]
    public async Task ManagePlaylists_NoPlaylists_ShouldShowEmptyList()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist>()));

        var (player, output) = CreatePlayerWithOutput("0\n");
        await player.HandleInputAsync("L");

        output.ToString().Should().Contain("Playlists");
    }

    [Fact]
    public async Task ManagePlaylists_Create_ShouldCallCreatePlaylistAsync()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist>()));
        _playlistsMock.Setup(p => p.CreatePlaylistAsync("MyList", null))
            .ReturnsAsync(Result.Success(new Playlist { Name = "MyList" }));

        // First loop: C -> name -> empty desc -> back to list; Second loop: 0 -> exit
        var (player, output) = CreatePlayerWithOutput("C\nMyList\n\n0\n");
        await player.HandleInputAsync("L");

        _playlistsMock.Verify(p => p.CreatePlaylistAsync("MyList", null), Times.Once);
        output.ToString().Should().Contain("Created playlist: MyList");
    }

    [Fact]
    public async Task ManagePlaylists_Create_WithDescription_ShouldPassDescription()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist>()));
        _playlistsMock.Setup(p => p.CreatePlaylistAsync("MyList", "A desc"))
            .ReturnsAsync(Result.Success(new Playlist { Name = "MyList" }));

        var (player, output) = CreatePlayerWithOutput("C\nMyList\nA desc\n0\n");
        await player.HandleInputAsync("L");

        _playlistsMock.Verify(p => p.CreatePlaylistAsync("MyList", "A desc"), Times.Once);
    }

    [Fact]
    public async Task ManagePlaylists_Create_EmptyName_ShouldNotCall()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist>()));

        // C -> empty name -> back to list; 0 -> exit
        var (player, output) = CreatePlayerWithOutput("C\n\n0\n");
        await player.HandleInputAsync("L");

        _playlistsMock.Verify(p => p.CreatePlaylistAsync(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public async Task ManagePlaylists_CreateFails_ShouldShowError()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist>()));
        _playlistsMock.Setup(p => p.CreatePlaylistAsync("Dup", null))
            .ReturnsAsync(Result.Failure<Playlist>("Duplicate name"));

        var (player, output) = CreatePlayerWithOutput("C\nDup\n\n0\n");
        await player.HandleInputAsync("L");

        output.ToString().Should().Contain("Error creating playlist: Duplicate name");
    }

    [Fact]
    public async Task ManagePlaylists_SelectPlaylist_ShouldCallGetPlaylistWithTracksAsync()
    {
        var playlist = new Playlist { PlaylistId = 5, Name = "Faves", PlaylistTracks = new List<PlaylistTrack>() };
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist> { playlist }));
        _playlistsMock.Setup(p => p.GetPlaylistWithTracksAsync(5))
            .ReturnsAsync(Result.Success<Playlist?>(new Playlist { PlaylistId = 5, Name = "Faves", PlaylistTracks = new List<PlaylistTrack>() }));

        // Select playlist 1, then back from playlist view, then back from list
        var (player, output) = CreatePlayerWithOutput("1\n0\n0\n");
        await player.HandleInputAsync("L");

        _playlistsMock.Verify(p => p.GetPlaylistWithTracksAsync(5), Times.Once);
    }

    [Fact]
    public async Task ManagePlaylists_SelectPlaylist_InvalidNumber_ShouldShowError()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist> { new() { PlaylistId = 1, Name = "A", PlaylistTracks = new List<PlaylistTrack>() } }));

        // Select 5 (out of range) -> Invalid, then 0 -> exit
        var (player, output) = CreatePlayerWithOutput("5\n0\n");
        await player.HandleInputAsync("L");

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task ManagePlaylists_LoadPlaylist_ShouldCallLoadPlaylistIntoQueueAsync()
    {
        var playlist = new Playlist { PlaylistId = 3, Name = "Rock", PlaylistTracks = new List<PlaylistTrack>
        {
            new() { TrackId = 1, Position = 0 },
            new() { TrackId = 2, Position = 1 }
        }};
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist> { playlist }));
        _playlistsMock.Setup(p => p.GetPlaylistWithTracksAsync(3))
            .ReturnsAsync(Result.Success<Playlist?>(playlist));
        _playlistsMock.Setup(p => p.LoadPlaylistIntoQueueAsync(3))
            .ReturnsAsync(Result.Success());
        _browserMock.Setup(b => b.GetTrackByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(Result.Success<Track?>(new Track { Title = "T", FilePath = "f", FileHash = "h" }));

        // Select playlist 1 -> L to load -> 0 back from playlist -> 0 back from list
        var (player, output) = CreatePlayerWithOutput("1\nl\n0\n0\n");
        await player.HandleInputAsync("L");

        _playlistsMock.Verify(p => p.LoadPlaylistIntoQueueAsync(3), Times.Once);
        output.ToString().Should().Contain("Loaded 2 tracks into player queue");
    }

    [Fact]
    public async Task ManagePlaylists_DeletePlaylist_ShouldCallDeletePlaylistAsync()
    {
        var playlist = new Playlist { PlaylistId = 7, Name = "Old", PlaylistTracks = new List<PlaylistTrack>() };
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist> { playlist }));
        _playlistsMock.Setup(p => p.GetPlaylistWithTracksAsync(7))
            .ReturnsAsync(Result.Success<Playlist?>(playlist));
        _playlistsMock.Setup(p => p.DeletePlaylistAsync(7))
            .ReturnsAsync(Result.Success());

        // Select playlist 1 -> D to delete -> returns to list -> 0 back
        var (player, output) = CreatePlayerWithOutput("1\nd\n0\n");
        await player.HandleInputAsync("L");

        _playlistsMock.Verify(p => p.DeletePlaylistAsync(7), Times.Once);
        output.ToString().Should().Contain("Playlist deleted");
    }

    [Fact]
    public async Task ManagePlaylists_DeletePlaylist_Failure_ShouldShowError()
    {
        var playlist = new Playlist { PlaylistId = 7, Name = "Old", PlaylistTracks = new List<PlaylistTrack>() };
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist> { playlist }));
        _playlistsMock.Setup(p => p.GetPlaylistWithTracksAsync(7))
            .ReturnsAsync(Result.Success<Playlist?>(playlist));
        _playlistsMock.Setup(p => p.DeletePlaylistAsync(7))
            .ReturnsAsync(Result.Failure("DB error"));

        var (player, output) = CreatePlayerWithOutput("1\nd\n0\n");
        await player.HandleInputAsync("L");

        output.ToString().Should().Contain("Error deleting playlist: DB error");
    }

    [Fact]
    public async Task ManagePlaylists_Back_ShouldReturn()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Success(new List<Playlist>()));

        var (player, output) = CreatePlayerWithOutput("0\n");
        await player.HandleInputAsync("L");

        // Should not throw and should only call GetAllPlaylistsAsync once
        _playlistsMock.Verify(p => p.GetAllPlaylistsAsync(), Times.Once);
    }

    [Fact]
    public async Task ManagePlaylists_GetAllFails_ShouldShowError()
    {
        _playlistsMock.Setup(p => p.GetAllPlaylistsAsync())
            .ReturnsAsync(Result.Failure<List<Playlist>>("Connection failed"));

        var (player, output) = CreatePlayerWithOutput("");
        await player.HandleInputAsync("L");

        output.ToString().Should().Contain("Error: Connection failed");
    }

    // ========== ManageDirectoriesAsync Tests (D key) ==========

    [Fact]
    public async Task ManageDirectories_ShouldCallLoadConfigurationAsync()
    {
        _libraryMock.Setup(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }));

        var (player, output) = CreatePlayerWithOutput("0\n");
        await player.HandleInputAsync("D");

        _libraryMock.Verify(l => l.LoadConfigurationAsync(), Times.Once);
    }

    [Fact]
    public async Task ManageDirectories_NoPaths_ShouldShowNoneConfigured()
    {
        _libraryMock.Setup(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }));

        var (player, output) = CreatePlayerWithOutput("0\n");
        await player.HandleInputAsync("D");

        output.ToString().Should().Contain("(none configured)");
    }

    [Fact]
    public async Task ManageDirectories_WithPaths_ShouldDisplayThem()
    {
        _libraryMock.Setup(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music", @"D:\Songs" } }));

        var (player, output) = CreatePlayerWithOutput("0\n");
        await player.HandleInputAsync("D");

        var text = output.ToString();
        text.Should().Contain(@"C:\Music");
        text.Should().Contain(@"D:\Songs");
    }

    [Fact]
    public async Task ManageDirectories_AddPath_ShouldCallAddLibraryPathAsync()
    {
        _libraryMock.Setup(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }));
        _libraryMock.Setup(l => l.AddLibraryPathAsync(@"C:\NewMusic"))
            .ReturnsAsync(Result.Success());

        // A -> enter path -> back to menu (need second LoadConfig call) -> 0
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\NewMusic" } }));

        var (player, output) = CreatePlayerWithOutput("a\nC:\\NewMusic\n0\n");
        await player.HandleInputAsync("D");

        _libraryMock.Verify(l => l.AddLibraryPathAsync(@"C:\NewMusic"), Times.Once);
        output.ToString().Should().Contain("Path added");
    }

    [Fact]
    public async Task ManageDirectories_AddPath_EmptyInput_ShouldNotCall()
    {
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }));

        var (player, output) = CreatePlayerWithOutput("a\n\n0\n");
        await player.HandleInputAsync("D");

        _libraryMock.Verify(l => l.AddLibraryPathAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ManageDirectories_AddPath_Failure_ShouldShowError()
    {
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }));
        _libraryMock.Setup(l => l.AddLibraryPathAsync("bad"))
            .ReturnsAsync(Result.Failure("Path not found"));

        var (player, output) = CreatePlayerWithOutput("a\nbad\n0\n");
        await player.HandleInputAsync("D");

        output.ToString().Should().Contain("Path not found");
    }

    [Fact]
    public async Task ManageDirectories_RemovePath_ShouldCallRemoveLibraryPathAsync()
    {
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }));
        _libraryMock.Setup(l => l.RemoveLibraryPathAsync(@"C:\Music"))
            .ReturnsAsync(Result.Success());

        var (player, output) = CreatePlayerWithOutput("r\n1\n0\n");
        await player.HandleInputAsync("D");

        _libraryMock.Verify(l => l.RemoveLibraryPathAsync(@"C:\Music"), Times.Once);
        output.ToString().Should().Contain("Path removed");
    }

    [Fact]
    public async Task ManageDirectories_RemovePath_InvalidNumber_ShouldNotCall()
    {
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }));

        var (player, output) = CreatePlayerWithOutput("r\n5\n0\n");
        await player.HandleInputAsync("D");

        _libraryMock.Verify(l => l.RemoveLibraryPathAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ManageDirectories_RemovePath_Failure_ShouldShowError()
    {
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }));
        _libraryMock.Setup(l => l.RemoveLibraryPathAsync(@"C:\Music"))
            .ReturnsAsync(Result.Failure("Permission denied"));

        var (player, output) = CreatePlayerWithOutput("r\n1\n0\n");
        await player.HandleInputAsync("D");

        output.ToString().Should().Contain("Permission denied");
    }

    [Fact]
    public async Task ManageDirectories_Scan_ShouldCallScanAllLibrariesAsync()
    {
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }));
        _libraryMock.Setup(l => l.ScanAllLibrariesAsync())
            .ReturnsAsync(Result.Success(new ScanResult { TracksAdded = 10, TracksUpdated = 2, TracksRemoved = 1, Duration = TimeSpan.FromSeconds(3.5) }));

        var (player, output) = CreatePlayerWithOutput("s\n0\n");
        await player.HandleInputAsync("D");

        _libraryMock.Verify(l => l.ScanAllLibrariesAsync(), Times.Once);
    }

    [Fact]
    public async Task ManageDirectories_Scan_ShouldDisplayResults()
    {
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }));
        _libraryMock.Setup(l => l.ScanAllLibrariesAsync())
            .ReturnsAsync(Result.Success(new ScanResult { TracksAdded = 10, TracksUpdated = 2, TracksRemoved = 1, Duration = TimeSpan.FromSeconds(3.5) }));

        var (player, output) = CreatePlayerWithOutput("s\n0\n");
        await player.HandleInputAsync("D");

        var text = output.ToString();
        text.Should().Contain("10 added");
        text.Should().Contain("2 updated");
        text.Should().Contain("1 removed");
        text.Should().Contain("3.5s");
    }

    [Fact]
    public async Task ManageDirectories_Scan_Failure_ShouldShowError()
    {
        _libraryMock.SetupSequence(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }))
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string> { @"C:\Music" } }));
        _libraryMock.Setup(l => l.ScanAllLibrariesAsync())
            .ReturnsAsync(Result.Failure<ScanResult>("Scan failed"));

        var (player, output) = CreatePlayerWithOutput("s\n0\n");
        await player.HandleInputAsync("D");

        output.ToString().Should().Contain("Scan failed");
    }

    [Fact]
    public async Task ManageDirectories_Back_ShouldReturn()
    {
        _libraryMock.Setup(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Success(new Configuration { LibraryPaths = new List<string>() }));

        var (player, output) = CreatePlayerWithOutput("0\n");
        await player.HandleInputAsync("D");

        _libraryMock.Verify(l => l.LoadConfigurationAsync(), Times.Once);
    }

    [Fact]
    public async Task ManageDirectories_LoadConfigFails_ShouldShowError()
    {
        _libraryMock.Setup(l => l.LoadConfigurationAsync())
            .ReturnsAsync(Result.Failure<Configuration>("Config corrupt"));

        var (player, output) = CreatePlayerWithOutput("");
        await player.HandleInputAsync("D");

        output.ToString().Should().Contain("Error: Config corrupt");
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
