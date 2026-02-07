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

        return new MenuSystem(
            _audioMock.Object,
            _configService,
            statsService,
            scanService,
            _unitOfWork,
            input,
            output);
    }

    private (MenuSystem menu, StringWriter output) CreateMenuWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var statsService = new StatisticsService(_unitOfWork);
        var scanService = new LibraryScanService(_unitOfWork, _metadataMock.Object);

        var menu = new MenuSystem(
            _audioMock.Object,
            _configService,
            statsService,
            scanService,
            _unitOfWork,
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

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullAudioService_ShouldThrow()
    {
        var act = () => new MenuSystem(null!, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("audioService");
    }

    [Fact]
    public void Constructor_WithNullConfigService_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, null!,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("configService");
    }

    [Fact]
    public void Constructor_WithNullStatsService_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            null!,
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("statsService");
    }

    [Fact]
    public void Constructor_WithNullScanService_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            null!,
            _unitOfWork, TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("scanService");
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            null!, TextReader.Null, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, null!, TextWriter.Null);

        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new MenuSystem(_audioMock.Object, _configService,
            new StatisticsService(_unitOfWork),
            new LibraryScanService(_unitOfWork, _metadataMock.Object),
            _unitOfWork, TextReader.Null, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== DisplayMainMenu Tests ==========

    [Fact]
    public void DisplayMainMenu_ShouldShowAllOptions()
    {
        var (menu, output) = CreateMenuWithOutput("6\n");

        menu.DisplayMainMenu();

        var text = output.ToString();
        text.Should().Contain("FSMP");
        text.Should().Contain("Browse & Play");
        text.Should().Contain("Scan Libraries");
        text.Should().Contain("View Statistics");
        text.Should().Contain("Manage Libraries");
        text.Should().Contain("Settings");
        text.Should().Contain("Exit");
    }

    // ========== RunAsync Tests ==========

    [Fact]
    public async Task RunAsync_Option6_ShouldExitAndDisplayGoodbye()
    {
        var (menu, output) = CreateMenuWithOutput("6\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_InvalidOption_ShouldShowErrorAndContinue()
    {
        var (menu, output) = CreateMenuWithOutput("99\n6\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Invalid option");
        text.Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_EmptyInput_ShouldContinueLoop()
    {
        var (menu, output) = CreateMenuWithOutput("\n6\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Goodbye!");
    }

    // ========== Browse & Play Tests ==========

    [Fact]
    public async Task RunAsync_BrowseAndPlay_NoTracks_ShouldShowMessage()
    {
        var (menu, output) = CreateMenuWithOutput("1\n6\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("No tracks in library");
    }

    [Fact]
    public async Task RunAsync_BrowseAndPlay_ShouldListTracks()
    {
        await CreateTrackAsync("Kerala");

        var (menu, output) = CreateMenuWithOutput("1\n0\n6\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Kerala");
    }

    [Fact]
    public async Task RunAsync_BrowseAndPlay_SelectTrack_ShouldCallPlayTrackAsync()
    {
        var track = await CreateTrackAsync("Kerala");

        var (menu, output) = CreateMenuWithOutput("1\n1\n6\n");

        await menu.RunAsync();

        _audioMock.Verify(a => a.PlayTrackAsync(
            It.Is<Track>(t => t.TrackId == track.TrackId),
            It.IsAny<CancellationToken>()), Times.Once);
        output.ToString().Should().Contain("Playing:");
    }

    [Fact]
    public async Task RunAsync_BrowseAndPlay_InvalidSelection_ShouldShowError()
    {
        await CreateTrackAsync("Kerala");

        var (menu, output) = CreateMenuWithOutput("1\n999\n6\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Invalid selection");
    }

    // ========== View Statistics Tests ==========

    [Fact]
    public async Task RunAsync_ViewStatistics_ShouldShowStats()
    {
        await CreateTrackAsync("Track 1");

        var (menu, output) = CreateMenuWithOutput("3\n6\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Library Statistics");
        text.Should().Contain("Total tracks: 1");
    }

    // ========== Manage Libraries Tests ==========

    [Fact]
    public async Task RunAsync_ManageLibraries_ShouldShowNoPathsMessage()
    {
        var (menu, output) = CreateMenuWithOutput("4\n0\n6\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("(none configured)");
    }

    [Fact]
    public async Task RunAsync_ManageLibraries_AddPath_ShouldAddPath()
    {
        var (menu, output) = CreateMenuWithOutput("4\na\nC:\\Music\n6\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("Path added");
        var config = await _configService.LoadConfigurationAsync();
        config.LibraryPaths.Should().Contain(@"C:\Music");
    }

    // ========== Settings Tests ==========

    [Fact]
    public async Task RunAsync_Settings_ShouldShowCurrentSettings()
    {
        var (menu, output) = CreateMenuWithOutput("5\n\n6\n");

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
        var (menu, output) = CreateMenuWithOutput("2\n6\n");

        await menu.RunAsync();

        output.ToString().Should().Contain("No library paths configured");
    }
}
