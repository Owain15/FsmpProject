using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Models;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FSMP.Tests.UI;

public class LibraryManagerTests : IDisposable
{
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<IMetadataService> _metadataMock;
    private readonly string _configDir;
    private readonly ConfigurationService _configService;
    private readonly LibraryScanService _scanService;

    public LibraryManagerTests()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();
        _unitOfWork = new UnitOfWork(_context);

        _metadataMock = new Mock<IMetadataService>();
        _scanService = new LibraryScanService(_unitOfWork, _metadataMock.Object);

        _configDir = Path.Combine(Path.GetTempPath(), "FSMP_LibMgrTests", Guid.NewGuid().ToString());
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

    private (LibraryManager manager, StringWriter output) CreateManagerWithOutput(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var manager = new LibraryManager(_configService, _scanService, _unitOfWork, input, output);
        return (manager, output);
    }

    private async Task<Track> CreateTrackAsync(string title)
    {
        var track = new Track
        {
            Title = title,
            FilePath = $@"C:\Music\{title}.mp3",
            FileHash = Guid.NewGuid().ToString(),
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _unitOfWork.Tracks.AddAsync(track);
        await _unitOfWork.SaveAsync();
        return track;
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullConfigService_ShouldThrow()
    {
        var act = () => new LibraryManager(null!, _scanService, _unitOfWork, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("configService");
    }

    [Fact]
    public void Constructor_WithNullScanService_ShouldThrow()
    {
        var act = () => new LibraryManager(_configService, null!, _unitOfWork, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("scanService");
    }

    [Fact]
    public void Constructor_WithNullUnitOfWork_ShouldThrow()
    {
        var act = () => new LibraryManager(_configService, _scanService, null!, TextReader.Null, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("unitOfWork");
    }

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new LibraryManager(_configService, _scanService, _unitOfWork, null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new LibraryManager(_configService, _scanService, _unitOfWork, TextReader.Null, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== DisplayLibraryPathsAsync Tests ==========

    [Fact]
    public async Task DisplayLibraryPathsAsync_NoPaths_ShouldShowNoneConfigured()
    {
        var (manager, output) = CreateManagerWithOutput("");

        await manager.DisplayLibraryPathsAsync();

        output.ToString().Should().Contain("(none configured)");
    }

    [Fact]
    public async Task DisplayLibraryPathsAsync_WithPaths_ShouldListPaths()
    {
        await _configService.AddLibraryPathAsync(@"C:\Music");
        await _configService.AddLibraryPathAsync(@"D:\Audio");

        var (manager, output) = CreateManagerWithOutput("");

        await manager.DisplayLibraryPathsAsync();

        var text = output.ToString();
        text.Should().Contain("Library Paths");
        text.Should().Contain(@"C:\Music");
        text.Should().Contain(@"D:\Audio");
    }

    [Fact]
    public async Task DisplayLibraryPathsAsync_ShouldShowTrackCount()
    {
        await _configService.AddLibraryPathAsync(@"C:\Music");
        await CreateTrackAsync("Track1");
        await CreateTrackAsync("Track2");

        var (manager, output) = CreateManagerWithOutput("");

        await manager.DisplayLibraryPathsAsync();

        output.ToString().Should().Contain("Total tracks in database: 2");
    }

    // ========== AddLibraryPathAsync Tests ==========

    [Fact]
    public async Task AddLibraryPathAsync_ValidPath_ShouldAddToConfig()
    {
        var (manager, output) = CreateManagerWithOutput(@"C:\NewMusic" + "\n");

        await manager.AddLibraryPathAsync();

        output.ToString().Should().Contain("Path added");
        var config = await _configService.LoadConfigurationAsync();
        config.LibraryPaths.Should().Contain(@"C:\NewMusic");
    }

    [Fact]
    public async Task AddLibraryPathAsync_EmptyPath_ShouldShowError()
    {
        var (manager, output) = CreateManagerWithOutput("\n");

        await manager.AddLibraryPathAsync();

        output.ToString().Should().Contain("No path entered");
    }

    // ========== RemoveLibraryPathAsync Tests ==========

    [Fact]
    public async Task RemoveLibraryPathAsync_ValidIndex_ShouldRemoveFromConfig()
    {
        await _configService.AddLibraryPathAsync(@"C:\Music");
        await _configService.AddLibraryPathAsync(@"D:\Audio");

        var (manager, output) = CreateManagerWithOutput("1\n");

        await manager.RemoveLibraryPathAsync();

        output.ToString().Should().Contain(@"Removed: C:\Music");
        var config = await _configService.LoadConfigurationAsync();
        config.LibraryPaths.Should().NotContain(@"C:\Music");
        config.LibraryPaths.Should().Contain(@"D:\Audio");
    }

    [Fact]
    public async Task RemoveLibraryPathAsync_InvalidIndex_ShouldShowError()
    {
        await _configService.AddLibraryPathAsync(@"C:\Music");

        var (manager, output) = CreateManagerWithOutput("999\n");

        await manager.RemoveLibraryPathAsync();

        output.ToString().Should().Contain("Invalid selection");
    }

    [Fact]
    public async Task RemoveLibraryPathAsync_NoPaths_ShouldShowMessage()
    {
        var (manager, output) = CreateManagerWithOutput("");

        await manager.RemoveLibraryPathAsync();

        output.ToString().Should().Contain("No paths to remove");
    }

    // ========== ScanAllLibrariesAsync Tests ==========

    [Fact]
    public async Task ScanAllLibrariesAsync_NoPaths_ShouldShowMessage()
    {
        var (manager, output) = CreateManagerWithOutput("");

        await manager.ScanAllLibrariesAsync();

        output.ToString().Should().Contain("No library paths configured");
    }

    [Fact]
    public async Task ScanAllLibrariesAsync_WithPaths_ShouldShowResults()
    {
        var tempLib = Path.Combine(Path.GetTempPath(), "FSMP_ScanTest_" + Guid.NewGuid());
        Directory.CreateDirectory(tempLib);
        try
        {
            await _configService.AddLibraryPathAsync(tempLib);

            var (manager, output) = CreateManagerWithOutput("");

            await manager.ScanAllLibrariesAsync();

            var text = output.ToString();
            text.Should().Contain("Scanning all libraries");
            text.Should().Contain("Scan complete:");
            text.Should().Contain("Duration:");
        }
        finally
        {
            Directory.Delete(tempLib, true);
        }
    }

    // ========== ScanLibraryAsync Tests ==========

    [Fact]
    public async Task ScanLibraryAsync_ShouldShowResults()
    {
        var tempLib = Path.Combine(Path.GetTempPath(), "FSMP_ScanTest_" + Guid.NewGuid());
        Directory.CreateDirectory(tempLib);
        try
        {
            var (manager, output) = CreateManagerWithOutput("");

            await manager.ScanLibraryAsync(tempLib);

            var text = output.ToString();
            text.Should().Contain($"Scanning: {tempLib}");
            text.Should().Contain("Added:");
        }
        finally
        {
            Directory.Delete(tempLib, true);
        }
    }

    // ========== RunAsync Integration Tests ==========

    [Fact]
    public async Task RunAsync_BackOption_ShouldExit()
    {
        var (manager, output) = CreateManagerWithOutput("0\n");

        await manager.RunAsync();

        output.ToString().Should().Contain("Library Paths");
    }

    [Fact]
    public async Task RunAsync_AddPath_ShouldAddAndRedisplay()
    {
        // Add path, then exit
        var (manager, output) = CreateManagerWithOutput("a\nC:\\Music\n0\n");

        await manager.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Path added");
    }

    [Fact]
    public async Task RunAsync_InvalidOption_ShouldShowError()
    {
        var (manager, output) = CreateManagerWithOutput("x\n0\n");

        await manager.RunAsync();

        output.ToString().Should().Contain("Invalid option");
    }
}
