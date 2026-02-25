using FSMP.Core;
using FSMP.Core.Interfaces;
using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FSMP.Core.Models;
using FSMP.Tests.TestHelpers;
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
        _audioMock.Setup(a => a.Player).Returns(new MockAudioPlayer());
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

    // ========== RunAsync Tests ==========

    [Fact]
    public async Task RunAsync_ShouldLaunchPlayerUI_AndExitWithX()
    {
        var (menu, output) = CreateMenuWithOutput("X\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Queue: (empty)");
        text.Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_ShouldShowPlayerControls()
    {
        var (menu, output) = CreateMenuWithOutput("X\n");

        await menu.RunAsync();

        var text = output.ToString();
        text.Should().Contain("[N] Next");
        text.Should().Contain("[B] Browse");
        text.Should().Contain("[L] Playlists");
        text.Should().Contain("[D] Directories");
        text.Should().Contain("[X] Exit");
    }
}
