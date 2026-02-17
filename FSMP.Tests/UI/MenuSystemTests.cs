using FSMP.Core;
using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Models;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FSMP.Tests.UI;

public class MenuSystemTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<IAudioService> _audioMock;
    private readonly Mock<IMetadataService> _metadataMock;
    private readonly string _configDir;
    private readonly ConfigurationService _configService;

    public MenuSystemTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
        _audioMock = new Mock<IAudioService>();
        _metadataMock = new Mock<IMetadataService>();

        _configDir = Path.Combine(Path.GetTempPath(), "FSMP_MenuTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_configDir);
        _configService = new ConfigurationService(Path.Combine(_configDir, "config.json"));
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        if (Directory.Exists(_configDir))
            Directory.Delete(_configDir, recursive: true);
    }

    // --- Helpers ---

    private MenuSystem CreateMenu(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var statsService = new StatisticsService(_unitOfWork);
        var scanService = new LibraryScanService(_unitOfWork, _metadataMock.Object);
        var playlistService = new PlaylistService(_unitOfWork);
        var activePlaylist = new ActivePlaylistService();

        return new MenuSystem(
            _audioMock.Object,
            _configService,
            statsService,
            scanService,
            _unitOfWork,
            playlistService,
            activePlaylist,
            input,
            output);
    }

    private (MenuSystem menu, StringWriter output) CreateMenuWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var statsService = new StatisticsService(_unitOfWork);
        var scanService = new LibraryScanService(_unitOfWork, _metadataMock.Object);
        var playlistService = new PlaylistService(_unitOfWork);
        var activePlaylist = new ActivePlaylistService();

        var menu = new MenuSystem(
            _audioMock.Object,
            _configService,
            statsService,
            scanService,
            _unitOfWork,
            playlistService,
            activePlaylist,
            input,
            output);

        return (menu, output);
    }

    private async Task<Track> CreateTrackAsync(string title, string artist = "Test Artist")
    {
        var artistEntity = new Artist
        {
            Name = artist,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Artists.AddAsync(artistEntity);
        await _unitOfWork.SaveAsync();

        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{title}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            ArtistId = artistEntity.ArtistId,
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    private async Task<Track> CreateTrackWithAlbumAsync(string title, string artist, string albumTitle)
    {
        var artistEntity = new Artist
        {
            Name = artist,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Artists.AddAsync(artistEntity);
        await _unitOfWork.SaveAsync();

        var album = new Album
        {
            Title = albumTitle,
            ArtistId = artistEntity.ArtistId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Albums.AddAsync(album);
        await _unitOfWork.SaveAsync();

        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{title}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            ArtistId = artistEntity.ArtistId,
            AlbumId = album.AlbumId,
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullAudioService_ShouldThrow()
    {
        var act = () => new MenuSystem(null!, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, new PlaylistService(_unitOfWork), new ActivePlaylistService(),
            TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("audioService");
    }

    [Fact]
    public void Constructor_WithNullConfigService_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, null!,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, new PlaylistService(_unitOfWork), new ActivePlaylistService(),
            TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("configService");
    }

    [Fact]
    public void Constructor_WithNullStatsService_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            null!,
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, new PlaylistService(_unitOfWork), new ActivePlaylistService(),
            TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("statsService");
    }

    [Fact]
    public void Constructor_WithNullScanService_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            null!,
            _unitOfWork, new PlaylistService(_unitOfWork), new ActivePlaylistService(),
            TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("scanService");
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            null!, new PlaylistService(_unitOfWork), new ActivePlaylistService(),
            TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullPlaylistService_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, null!, new ActivePlaylistService(),
            TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("playlistService");
    }

    [Fact]
    public void Constructor_WithNullActivePlaylist_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, new PlaylistService(_unitOfWork), null!,
            TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("activePlaylist");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, new PlaylistService(_unitOfWork), new ActivePlaylistService(),
            null!, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, new PlaylistService(_unitOfWork), new ActivePlaylistService(),
            TextReader.Null, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== DisplayMainMenu Tests ==========

    [Fact]
    public void DisplayMainMenu_ShouldShowAllOptions()
    {
        var (menu, output) = CreateMenuWithOutput("8\n");

        menu.DisplayMainMenu();

        var text = output.ToString();
        text.Should().Contain("FSMP");
        text.Should().Contain("Browse & Play");
        text.Should().Contain("Player");
        text.Should().Contain("Playlists");
        text.Should().Contain("Scan Libraries");
        text.Should().Contain("View Statistics");
        text.Should().Contain("Manage Libraries");
        text.Should().Contain("Settings");
        text.Should().Contain("Exit");
    }

    // ========== RunAsync Tests ==========

    [Fact]
    public async Task RunAsync_Option8_ShouldExitAndDisplayGoodbye()
    {
        var (menu, output) = CreateMenuWithOutput("8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_InvalidOption_ShouldShowErrorAndContinue()
    {
        var (menu, output) = CreateMenuWithOutput("99\n8\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Invalid option");
        text.Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_EmptyInput_ShouldContinueLoop()
    {
        var (menu, output) = CreateMenuWithOutput("\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Goodbye!");
    }

    // ========== Browse & Play Tests ==========

    [Fact]
    public async Task RunAsync_BrowseAndPlay_NoTracks_ShouldShowMessage()
    {
        var (menu, output) = CreateMenuWithOutput("1\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("No artists in library");
    }

    [Fact]
    public async Task RunAsync_BrowseAndPlay_ShouldListArtists()
    {
        await CreateTrackAsync("Kerala");

        // Browse → see artists → back → exit
        var (menu, output) = CreateMenuWithOutput("1\n0\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Test Artist");
    }

    [Fact]
    public async Task RunAsync_BrowseAndPlay_SelectTrack_ShouldCallPlayTrackAsync()
    {
        var track = await CreateTrackWithAlbumAsync("Kerala", "Test Artist", "Migration");

        // Browse → select artist → select album → select track → exit
        var (menu, output) = CreateMenuWithOutput("1\n1\n1\n1\n8\n");

        await menu.RunAsync();

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
        output.ToString().Should().Contain("Now Playing");
    }

    [Fact]
    public async Task RunAsync_BrowseAndPlay_InvalidSelection_ShouldShowError()
    {
        await CreateTrackAsync("Kerala");

        var (menu, output) = CreateMenuWithOutput("1\n999\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Invalid selection");
    }

    // ========== View Statistics Tests ==========

    [Fact]
    public async Task RunAsync_ViewStatistics_ShouldShowStats()
    {
        await CreateTrackAsync("Track 1");

        var (menu, output) = CreateMenuWithOutput("5\n8\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Library Statistics");
        text.Should().Contain("Total tracks: 1");
    }

    // ========== Manage Libraries Tests ==========

    [Fact]
    public async Task RunAsync_ManageLibraries_ShouldShowNoPathsMessage()
    {
        var (menu, output) = CreateMenuWithOutput("6\n0\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("(none configured)");
    }

    [Fact]
    public async Task RunAsync_ManageLibraries_AddPath_ShouldAddPath()
    {
        var (menu, output) = CreateMenuWithOutput("6\na\nC:\\Music\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Path added");
        var config = await _configService.LoadConfigurationAsync();
        config.LibraryPaths.Should().Contain(@"C:\Music");
    }

    // ========== Settings Tests ==========

    [Fact]
    public async Task RunAsync_Settings_ShouldShowCurrentSettings()
    {
        var (menu, output) = CreateMenuWithOutput("7\n\n8\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Settings");
        text.Should().Contain("Volume:");
        text.Should().Contain("Auto-scan:");
    }

    // ========== Scan Libraries Tests ==========

    [Fact]
    public async Task RunAsync_ScanLibraries_NoPathsConfigured_ShouldShowMessage()
    {
        var (menu, output) = CreateMenuWithOutput("4\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("No library paths configured");
    }

    // ========== Player Tests ==========

    [Fact]
    public async Task RunAsync_OpenPlayer_ShouldShowPlayerUI()
    {
        // Option 2 = Player, then Q = back from player, then 8 = exit
        var (menu, output) = CreateMenuWithOutput("2\nQ\n8\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Queue: (empty)");
        text.Should().Contain("Goodbye!");
    }

    // ========== Playlist Tests ==========

    [Fact]
    public async Task RunAsync_Playlists_EmptyList_ShouldShowBack()
    {
        // Option 3 = Playlists, then 0 = back, then 8 = exit
        var (menu, output) = CreateMenuWithOutput("3\n0\n8\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Playlists");
        text.Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_Playlists_CreateNew_ShouldCreatePlaylist()
    {
        // Option 3 = Playlists, C = create, name "My Mix", desc "Best tracks", then 0 = back, 8 = exit
        var (menu, output) = CreateMenuWithOutput("3\nc\nMy Mix\nBest tracks\n0\n8\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Created playlist: My Mix");
    }

    [Fact]
    public async Task RunAsync_Playlists_CreateNew_EmptyName_ShouldNotCreate()
    {
        // Option 3 = Playlists, C = create, empty name, then 0 = back, 8 = exit
        var (menu, output) = CreateMenuWithOutput("3\nc\n\n0\n8\n");

        await menu.RunAsync();

        output.ToString().Should().NotContain("Created playlist");
    }

    [Fact]
    public async Task RunAsync_Playlists_SelectPlaylist_ShouldShowPlaylist()
    {
        // Create a playlist first
        var playlistService = new PlaylistService(_unitOfWork);
        var playlist = await playlistService.CreatePlaylistAsync("My Mix", "Test desc");

        // Option 3 = Playlists, 1 = select first, 0 = back from view, 0 = back from list, 8 = exit
        var (menu, output) = CreateMenuWithOutput("3\n1\n0\n0\n8\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("My Mix");
        text.Should().Contain("Test desc");
        text.Should().Contain("(no tracks)");
    }

    [Fact]
    public async Task RunAsync_Playlists_ViewPlaylistWithTracks_ShouldListTracks()
    {
        var track = await CreateTrackAsync("Kerala");
        var playlistService = new PlaylistService(_unitOfWork);
        var playlist = await playlistService.CreatePlaylistAsync("My Mix");
        await playlistService.AddTrackAsync(playlist.PlaylistId, track.TrackId);

        // Option 3 = Playlists, 1 = select, 0 = back from view, 0 = back from list, 8 = exit
        var (menu, output) = CreateMenuWithOutput("3\n1\n0\n0\n8\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Kerala");
    }

    [Fact]
    public async Task RunAsync_Playlists_LoadIntoQueue_ShouldLoadTracks()
    {
        var track = await CreateTrackAsync("Kerala");
        var playlistService = new PlaylistService(_unitOfWork);
        var playlist = await playlistService.CreatePlaylistAsync("My Mix");
        await playlistService.AddTrackAsync(playlist.PlaylistId, track.TrackId);

        // Option 3 = Playlists, 1 = select, L = load into queue, 0 = back from list, 8 = exit
        var (menu, output) = CreateMenuWithOutput("3\n1\nl\n0\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Loaded 1 tracks into player queue");
    }

    [Fact]
    public async Task RunAsync_Playlists_LoadEmptyPlaylist_ShouldShowMessage()
    {
        var playlistService = new PlaylistService(_unitOfWork);
        await playlistService.CreatePlaylistAsync("Empty Mix");

        // Option 3 = Playlists, 1 = select, L = load, 0 = back from list, 8 = exit
        var (menu, output) = CreateMenuWithOutput("3\n1\nl\n0\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("No tracks to load");
    }

    [Fact]
    public async Task RunAsync_Playlists_DeletePlaylist_ShouldDelete()
    {
        var playlistService = new PlaylistService(_unitOfWork);
        await playlistService.CreatePlaylistAsync("My Mix");

        // Option 3 = Playlists, 1 = select, D = delete, 0 = back from list, 8 = exit
        var (menu, output) = CreateMenuWithOutput("3\n1\nd\n0\n8\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Playlist deleted");
    }
}
