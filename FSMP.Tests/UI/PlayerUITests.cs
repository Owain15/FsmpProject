using FSMP.Core;
using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FSMP.Core.Models;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FSMP.Tests.UI;

public class PlayerUITests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<IAudioService> _audioMock;
    private readonly Mock<IMetadataService> _metadataMock;
    private readonly ActivePlaylistService _activePlaylist;
    private readonly PlaylistService _playlistService;
    private readonly ConfigurationService _configService;
    private readonly LibraryScanService _scanService;
    private readonly string _configDir;

    public PlayerUITests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
        _audioMock = new Mock<IAudioService>();
        _metadataMock = new Mock<IMetadataService>();
        _activePlaylist = new ActivePlaylistService();
        _playlistService = new PlaylistService(_unitOfWork);

        _configDir = Path.Combine(Path.GetTempPath(), "FSMP_PlayerUITests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_configDir);
        _configService = new ConfigurationService(Path.Combine(_configDir, "config.json"));
        _scanService = new LibraryScanService(_unitOfWork, _metadataMock.Object);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, recursive: true);
    }

    // --- Helpers ---

    private (PlayerUI player, StringWriter output) CreatePlayerWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var player = new PlayerUI(_activePlaylist, _audioMock.Object, _unitOfWork,
            _playlistService, _configService, _scanService, input, output);
        return (player, output);
    }

    private async Task<Track> CreateTrackAsync(string title, string? artistName = null, string? albumTitle = null, TimeSpan? duration = null)
    {
        Artist? artist = null;
        if (artistName != null)
        {
            artist = new Artist { Name = artistName, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            await _unitOfWork.Artists.AddAsync(artist);
            await _unitOfWork.SaveAsync();
        }

        Album? album = null;
        if (albumTitle != null)
        {
            album = new Album
            {
                Title = albumTitle,
                ArtistId = artist?.ArtistId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await _unitOfWork.Albums.AddAsync(album);
            await _unitOfWork.SaveAsync();
        }

        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{title}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            ArtistId = artist?.ArtistId,
            AlbumId = album?.AlbumId,
            Duration = duration ?? TimeSpan.FromSeconds(200),
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullActivePlaylist_ShouldThrow()
    {
        var act = () => new PlayerUI(null!, _audioMock.Object, _unitOfWork, _playlistService, _configService, _scanService, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("activePlaylist");
    }

    [Fact]
    public void Constructor_WithNullAudioService_ShouldThrow()
    {
        var act = () => new PlayerUI(_activePlaylist, null!, _unitOfWork, _playlistService, _configService, _scanService, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("audioService");
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new PlayerUI(_activePlaylist, _audioMock.Object, null!, _playlistService, _configService, _scanService, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullPlaylistService_ShouldThrow()
    {
        var act = () => new PlayerUI(_activePlaylist, _audioMock.Object, _unitOfWork, null!, _configService, _scanService, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("playlistService");
    }

    [Fact]
    public void Constructor_WithNullConfigService_ShouldThrow()
    {
        var act = () => new PlayerUI(_activePlaylist, _audioMock.Object, _unitOfWork, _playlistService, null!, _scanService, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configService");
    }

    [Fact]
    public void Constructor_WithNullScanService_ShouldThrow()
    {
        var act = () => new PlayerUI(_activePlaylist, _audioMock.Object, _unitOfWork, _playlistService, _configService, null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("scanService");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new PlayerUI(_activePlaylist, _audioMock.Object, _unitOfWork, _playlistService, _configService, _scanService, null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new PlayerUI(_activePlaylist, _audioMock.Object, _unitOfWork, _playlistService, _configService, _scanService, TextReader.Null, null!);
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
        // Empty line then X
        var (player, output) = CreatePlayerWithOutput("\nX\n");

        await player.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Queue: (empty)");
    }

    [Fact]
    public async Task RunAsync_LowercaseX_ShouldExit()
    {
        // Input is uppercased, so "x" becomes "X"
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
        var track = await CreateTrackAsync("Kerala", "Bonobo", "Migration");
        _activePlaylist.SetQueue(new[] { track.TrackId });

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
        var track1 = await CreateTrackAsync("Track1", "Artist1");
        var track2 = await CreateTrackAsync("Track2", "Artist2");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });

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
        _activePlaylist.RepeatMode = RepeatMode.None;
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
    public async Task HandleInputAsync_Next_WithNextTrack_ShouldPlayNext()
    {
        var track1 = await CreateTrackAsync("Track1");
        var track2 = await CreateTrackAsync("Track2");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("N");

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track2.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Next_AtEndNoRepeat_ShouldShowEndOfQueue()
    {
        var track = await CreateTrackAsync("Track1");
        _activePlaylist.SetQueue(new[] { track.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("N");
        _audioMock.Verify(a => a.StopAsync(), Times.Once);

        // Feedback appears on next display cycle
        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("End of queue");
    }

    [Fact]
    public async Task HandleInputAsync_Next_EmptyQueue_ShouldNotThrow()
    {
        var (player, output) = CreatePlayerWithOutput("");

        var act = async () => await player.HandleInputAsync("N");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task HandleInputAsync_Next_RepeatOne_ShouldReplayCurrentTrack()
    {
        var track = await CreateTrackAsync("Track1");
        _activePlaylist.SetQueue(new[] { track.TrackId });
        _activePlaylist.RepeatMode = RepeatMode.One;

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("N");

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Next_RepeatAll_ShouldWrapAround()
    {
        var track1 = await CreateTrackAsync("Track1");
        var track2 = await CreateTrackAsync("Track2");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });
        _activePlaylist.RepeatMode = RepeatMode.All;

        // Move to last track
        _activePlaylist.MoveNext();

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("N");

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track1.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ========== HandleInputAsync — Previous (P) ==========

    [Fact]
    public async Task HandleInputAsync_Previous_WithPreviousTrack_ShouldPlayPrevious()
    {
        var track1 = await CreateTrackAsync("Track1");
        var track2 = await CreateTrackAsync("Track2");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });
        _activePlaylist.MoveNext(); // move to track2

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("P");

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track1.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Previous_AtBeginning_ShouldShowBeginningOfQueue()
    {
        var track = await CreateTrackAsync("Track1");
        _activePlaylist.SetQueue(new[] { track.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("P");

        // Feedback appears on next display cycle
        await player.DisplayPlayerStateAsync();
        output.ToString().Should().Contain("Beginning of queue");
    }

    // ========== HandleInputAsync — Pause/Resume (K) ==========

    [Fact]
    public async Task HandleInputAsync_K_WhenPlaying_ShouldPause()
    {
        var track = await CreateTrackAsync("Track1");
        _activePlaylist.SetQueue(new[] { track.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        // First play a track to set _isPlaying = true
        await player.HandleInputAsync("N");
        _audioMock.Reset();

        // Better approach: use a 2-track queue, play next to set isPlaying
        var track2 = await CreateTrackAsync("Track2");
        _activePlaylist.SetQueue(new[] { track.TrackId, track2.TrackId });

        // Play next track (moves from track1 to track2, sets _isPlaying = true)
        await player.HandleInputAsync("N");
        _audioMock.Reset();

        // Now pause
        await player.HandleInputAsync("K");

        _audioMock.Verify(a => a.PauseAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_K_WhenNotPlaying_WithCurrentTrack_ShouldResume()
    {
        var track = await CreateTrackAsync("Track1");
        _activePlaylist.SetQueue(new[] { track.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        // K when not playing but has current track → resume
        await player.HandleInputAsync("K");

        _audioMock.Verify(a => a.ResumeAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_K_WhenNotPlaying_NoCurrentTrack_ShouldDoNothing()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("K");

        _audioMock.Verify(a => a.ResumeAsync(), Times.Never);
        _audioMock.Verify(a => a.PauseAsync(), Times.Never);
    }

    // ========== HandleInputAsync — Restart (R) ==========

    [Fact]
    public async Task HandleInputAsync_Restart_WithCurrentTrack_ShouldSeekToZero()
    {
        var track = await CreateTrackAsync("Track1");
        _activePlaylist.SetQueue(new[] { track.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("R");

        _audioMock.Verify(a => a.SeekAsync(TimeSpan.Zero), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Restart_NoCurrentTrack_ShouldDoNothing()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("R");

        _audioMock.Verify(a => a.SeekAsync(It.IsAny<TimeSpan>()), Times.Never);
    }

    // ========== HandleInputAsync — Stop (S) ==========

    [Fact]
    public async Task HandleInputAsync_Stop_ShouldCallStopAsync()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("S");

        _audioMock.Verify(a => a.StopAsync(), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_Stop_ShouldSetNotPlaying()
    {
        var track1 = await CreateTrackAsync("Track1");
        var track2 = await CreateTrackAsync("Track2");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        // Play next to set isPlaying = true
        await player.HandleInputAsync("N");

        // Stop
        await player.HandleInputAsync("S");

        // Display state — should show Stopped
        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Stopped");
    }

    // ========== HandleInputAsync — Repeat Mode (M) ==========

    [Fact]
    public async Task HandleInputAsync_RepeatMode_ShouldCycleNoneToOne()
    {
        _activePlaylist.RepeatMode = RepeatMode.None;
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("M");

        _activePlaylist.RepeatMode.Should().Be(RepeatMode.One);
    }

    [Fact]
    public async Task HandleInputAsync_RepeatMode_ShouldCycleOneToAll()
    {
        _activePlaylist.RepeatMode = RepeatMode.One;
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("M");

        _activePlaylist.RepeatMode.Should().Be(RepeatMode.All);
    }

    [Fact]
    public async Task HandleInputAsync_RepeatMode_ShouldCycleAllToNone()
    {
        _activePlaylist.RepeatMode = RepeatMode.All;
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("M");

        _activePlaylist.RepeatMode.Should().Be(RepeatMode.None);
    }

    // ========== HandleInputAsync — Shuffle (H) ==========

    [Fact]
    public async Task HandleInputAsync_Shuffle_ShouldToggleShuffle()
    {
        var track1 = await CreateTrackAsync("Track1");
        var track2 = await CreateTrackAsync("Track2");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });

        _activePlaylist.IsShuffled.Should().BeFalse();

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("H");

        _activePlaylist.IsShuffled.Should().BeTrue();
    }

    [Fact]
    public async Task HandleInputAsync_Shuffle_TwiceShouldUnshuffle()
    {
        var track1 = await CreateTrackAsync("Track1");
        var track2 = await CreateTrackAsync("Track2");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("H");
        await player.HandleInputAsync("H");

        _activePlaylist.IsShuffled.Should().BeFalse();
    }

    // ========== HandleInputAsync — Unknown Command ==========

    [Fact]
    public async Task HandleInputAsync_UnknownCommand_ShouldDoNothing()
    {
        var (player, output) = CreatePlayerWithOutput("");

        var act = async () => await player.HandleInputAsync("Z");

        await act.Should().NotThrowAsync();
        _audioMock.VerifyNoOtherCalls();
    }

    // ========== Queue Display Tests ==========

    [Fact]
    public async Task DisplayPlayerStateAsync_CurrentTrackHighlighted_ShouldShowArrow()
    {
        var track1 = await CreateTrackAsync("First", "ArtistA");
        var track2 = await CreateTrackAsync("Second", "ArtistB");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        var text = output.ToString();
        // Current track (index 0) should have "> " prefix
        text.Should().Contain("> 1) First");
        // Other track should have "  " prefix
        text.Should().Contain("  2) Second");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_TrackWithDuration_ShouldShowDuration()
    {
        var track = await CreateTrackAsync("Kerala", duration: TimeSpan.FromSeconds(195));
        _activePlaylist.SetQueue(new[] { track.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("[3:15]");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_RepeatOne_ShouldShowInStatus()
    {
        _activePlaylist.RepeatMode = RepeatMode.One;
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Repeat: One");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_RepeatAll_ShouldShowInStatus()
    {
        _activePlaylist.RepeatMode = RepeatMode.All;
        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Repeat: All");
    }

    [Fact]
    public async Task DisplayPlayerStateAsync_ShuffleOn_ShouldShowOn()
    {
        var track1 = await CreateTrackAsync("Track1");
        var track2 = await CreateTrackAsync("Track2");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId });
        _activePlaylist.ToggleShuffle();

        var (player, output) = CreatePlayerWithOutput("");

        await player.DisplayPlayerStateAsync();

        output.ToString().Should().Contain("Shuffle: On");
    }

    // ========== HandleInputAsync — Skip to track number ==========

    [Fact]
    public async Task HandleInputAsync_TrackNumber_ShouldJumpAndPlay()
    {
        var track1 = await CreateTrackAsync("Track1");
        var track2 = await CreateTrackAsync("Track2");
        var track3 = await CreateTrackAsync("Track3");
        _activePlaylist.SetQueue(new[] { track1.TrackId, track2.TrackId, track3.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("3");

        _activePlaylist.CurrentIndex.Should().Be(2);
        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track3.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleInputAsync_TrackNumber_Zero_ShouldNotJump()
    {
        var track1 = await CreateTrackAsync("Track1");
        _activePlaylist.SetQueue(new[] { track1.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("0");

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.IsAny<Track>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleInputAsync_TrackNumber_OutOfRange_ShouldNotJump()
    {
        var track1 = await CreateTrackAsync("Track1");
        _activePlaylist.SetQueue(new[] { track1.TrackId });

        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("5");

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.IsAny<Track>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleInputAsync_TrackNumber_EmptyQueue_ShouldNotJump()
    {
        var (player, output) = CreatePlayerWithOutput("");

        await player.HandleInputAsync("1");

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.IsAny<Track>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}