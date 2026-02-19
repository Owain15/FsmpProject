using FluentAssertions;
using FsmpConsole;

namespace FSMP.Tests.UI;

public class AppStartupTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configPath;
    private readonly string _dbPath;

    public AppStartupTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FSMP_StartupTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _configPath = Path.Combine(_tempDir, "config.json");
        _dbPath = Path.Combine(_tempDir, "fsmp.db");
    }

    public void Dispose()
    {
        // SQLite may keep the file briefly locked; retry cleanup
        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (Directory.Exists(_tempDir))
                    Directory.Delete(_tempDir, recursive: true);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(100);
            }
        }
    }

    // --- Helpers ---

    private (AppStartup app, StringWriter output) CreateApp(string inputLines)
    {
        var input = new StringReader(inputLines);
        var output = new StringWriter();
        var app = new AppStartup(input, output, _configPath, _dbPath);
        return (app, output);
    }

    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullInput_ShouldThrow()
    {
        var act = () => new AppStartup(null!, TextWriter.Null);
        act.Should().Throw<ArgumentNullException>().WithParameterName("input");
    }

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new AppStartup(TextReader.Null, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== GetConfigPath Tests ==========

    [Fact]
    public void GetConfigPath_WithOverride_ShouldReturnOverride()
    {
        var app = new AppStartup(TextReader.Null, TextWriter.Null, @"C:\custom\config.json");
        app.GetConfigPath().Should().Be(@"C:\custom\config.json");
    }

    [Fact]
    public void GetConfigPath_WithoutOverride_ShouldReturnAppDataPath()
    {
        var app = new AppStartup(TextReader.Null, TextWriter.Null);
        var result = app.GetConfigPath();
        result.Should().EndWith(Path.Combine("FSMP", "config.json"));
    }

    // ========== GetDatabasePath Tests ==========

    [Fact]
    public void GetDatabasePath_WithOverride_ShouldReturnOverride()
    {
        var app = new AppStartup(TextReader.Null, TextWriter.Null, dbPathOverride: @"C:\custom\fsmp.db");
        app.GetDatabasePath(null).Should().Be(@"C:\custom\fsmp.db");
    }

    [Fact]
    public void GetDatabasePath_WithConfig_ShouldReturnConfigPath()
    {
        var app = new AppStartup(TextReader.Null, TextWriter.Null);
        var config = new FSMP.Core.Models.Configuration { DatabasePath = @"C:\data\mydb.db" };
        app.GetDatabasePath(config).Should().Be(@"C:\data\mydb.db");
    }

    [Fact]
    public void GetDatabasePath_WithoutOverrideOrConfig_ShouldReturnAppDataPath()
    {
        var app = new AppStartup(TextReader.Null, TextWriter.Null);
        var result = app.GetDatabasePath(null);
        result.Should().EndWith(Path.Combine("FSMP", "fsmp.db"));
    }

    // ========== RunAsync Tests ==========

    [Fact]
    public async Task RunAsync_ShouldCreateConfigFile()
    {
        var (app, output) = CreateApp("8\n");

        await app.RunAsync();

        File.Exists(_configPath).Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_ShouldCreateDatabaseFile()
    {
        var (app, output) = CreateApp("8\n");

        await app.RunAsync();

        File.Exists(_dbPath).Should().BeTrue();
    }

    [Fact]
    public async Task RunAsync_ShouldApplyMigrations()
    {
        var (app, output) = CreateApp("8\n");

        await app.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Database ready at:");
    }

    [Fact]
    public async Task RunAsync_ShouldShowWelcomeMessage()
    {
        var (app, output) = CreateApp("8\n");

        await app.RunAsync();

        var text = output.ToString();
        text.Should().Contain("FSMP - File System Music Player");
        text.Should().Contain("Config loaded from:");
    }

    [Fact]
    public async Task RunAsync_ShouldDisplayMenuAndAcceptExit()
    {
        var (app, output) = CreateApp("8\n");

        await app.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Browse & Play");
        text.Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_WithAutoScanAndNoPaths_ShouldSkipScan()
    {
        var (app, output) = CreateApp("8\n");

        await app.RunAsync();

        var text = output.ToString();
        text.Should().NotContain("Auto-scanning");
    }

    [Fact]
    public async Task RunAsync_ShouldBeIdempotent_RunTwice()
    {
        // First run
        var (app1, output1) = CreateApp("8\n");
        await app1.RunAsync();

        // Second run - should work with existing config and DB
        var (app2, output2) = CreateApp("8\n");
        await app2.RunAsync();

        var text = output2.ToString();
        text.Should().Contain("Config loaded from:");
        text.Should().Contain("Database ready at:");
        text.Should().Contain("Goodbye!");
    }

    [Fact]
    public async Task RunAsync_ViewStatistics_ShouldShowEmptyStats()
    {
        // Option 5 = View Statistics, then 8 = Exit
        var (app, output) = CreateApp("5\n8\n");

        await app.RunAsync();

        var text = output.ToString();
        text.Should().Contain("Library Statistics");
        text.Should().Contain("Total tracks: 0");
    }

    [Fact]
    public async Task RunAsync_ScanLibraries_NoPaths_ShouldShowMessage()
    {
        // Option 4 = Scan Libraries (no paths configured), then 8 = Exit
        var (app, output) = CreateApp("4\n8\n");

        await app.RunAsync();

        var text = output.ToString();
        text.Should().Contain("No library paths configured");
    }

    [Fact]
    public async Task RunAsync_BrowseAndPlay_EmptyLibrary_ShouldShowNoArtists()
    {
        // Option 1 = Browse, then 0 = Back, then 8 = Exit
        var (app, output) = CreateApp("1\n0\n8\n");

        await app.RunAsync();

        var text = output.ToString();
        text.Should().Contain("No artists");
    }
}