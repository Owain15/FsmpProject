using System.Text.Json;
using FluentAssertions;
using FSMP.Core.Models;
using FsmpLibrary.Services;

namespace FSMP.Tests.Services;

public class ConfigurationServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configPath;
    private readonly ConfigurationService _service;

    public ConfigurationServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FSMP_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _configPath = Path.Combine(_tempDir, "config.json");
        _service = new ConfigurationService(_configPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    [Fact]
    public void GetDefaultConfiguration_ShouldReturnValidDefaults()
    {
        var config = _service.GetDefaultConfiguration();

        config.Should().NotBeNull();
        config.LibraryPaths.Should().BeEmpty();
        config.DatabasePath.Should().Contain("FSMP").And.EndWith("fsmp.db");
        config.AutoScanOnStartup.Should().BeTrue();
        config.DefaultVolume.Should().Be(75);
        config.RememberLastPlayed.Should().BeTrue();
        config.LastPlayedTrackPath.Should().BeNull();
    }

    [Fact]
    public async Task LoadConfigurationAsync_ShouldCreateDefault_WhenFileMissing()
    {
        File.Exists(_configPath).Should().BeFalse();

        var config = await _service.LoadConfigurationAsync();

        config.Should().NotBeNull();
        config.DefaultVolume.Should().Be(75);
        File.Exists(_configPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveConfigurationAsync_ShouldWriteValidJson()
    {
        var config = new Configuration
        {
            LibraryPaths = new List<string> { @"C:\Music", @"D:\Music" },
            DatabasePath = @"C:\data\fsmp.db",
            AutoScanOnStartup = false,
            DefaultVolume = 50,
            RememberLastPlayed = false,
            LastPlayedTrackPath = @"C:\Music\song.mp3"
        };

        await _service.SaveConfigurationAsync(config);

        File.Exists(_configPath).Should().BeTrue();
        var json = await File.ReadAllTextAsync(_configPath);
        var deserialized = JsonSerializer.Deserialize<Configuration>(json);
        deserialized.Should().NotBeNull();
        deserialized!.LibraryPaths.Should().HaveCount(2);
        deserialized.DefaultVolume.Should().Be(50);
    }

    [Fact]
    public async Task LoadConfigurationAsync_ShouldReadSavedJson()
    {
        var original = new Configuration
        {
            LibraryPaths = new List<string> { @"C:\MyMusic" },
            DatabasePath = @"C:\db\fsmp.db",
            AutoScanOnStartup = false,
            DefaultVolume = 90,
            RememberLastPlayed = true,
            LastPlayedTrackPath = @"C:\MyMusic\track.wav"
        };
        await _service.SaveConfigurationAsync(original);

        var loaded = await _service.LoadConfigurationAsync();

        loaded.LibraryPaths.Should().ContainSingle().Which.Should().Be(@"C:\MyMusic");
        loaded.DatabasePath.Should().Be(@"C:\db\fsmp.db");
        loaded.AutoScanOnStartup.Should().BeFalse();
        loaded.DefaultVolume.Should().Be(90);
        loaded.RememberLastPlayed.Should().BeTrue();
        loaded.LastPlayedTrackPath.Should().Be(@"C:\MyMusic\track.wav");
    }

    [Fact]
    public async Task AddLibraryPathAsync_ShouldAddPath()
    {
        await _service.AddLibraryPathAsync(@"C:\Music");

        var config = await _service.LoadConfigurationAsync();
        config.LibraryPaths.Should().Contain(@"C:\Music");
    }

    [Fact]
    public async Task AddLibraryPathAsync_ShouldNotDuplicate()
    {
        await _service.AddLibraryPathAsync(@"C:\Music");
        await _service.AddLibraryPathAsync(@"C:\Music");

        var config = await _service.LoadConfigurationAsync();
        config.LibraryPaths.Should().HaveCount(1);
    }

    [Fact]
    public async Task RemoveLibraryPathAsync_ShouldRemovePath()
    {
        await _service.AddLibraryPathAsync(@"C:\Music");
        await _service.AddLibraryPathAsync(@"D:\Music");

        await _service.RemoveLibraryPathAsync(@"C:\Music");

        var config = await _service.LoadConfigurationAsync();
        config.LibraryPaths.Should().ContainSingle().Which.Should().Be(@"D:\Music");
    }

    [Fact]
    public async Task RemoveLibraryPathAsync_ShouldDoNothing_WhenPathNotFound()
    {
        await _service.AddLibraryPathAsync(@"C:\Music");

        await _service.RemoveLibraryPathAsync(@"X:\Nonexistent");

        var config = await _service.LoadConfigurationAsync();
        config.LibraryPaths.Should().HaveCount(1);
    }

    [Fact]
    public async Task SaveConfigurationAsync_ShouldCreateDirectory_WhenMissing()
    {
        var nestedPath = Path.Combine(_tempDir, "sub", "dir", "config.json");
        var nestedService = new ConfigurationService(nestedPath);

        await nestedService.SaveConfigurationAsync(nestedService.GetDefaultConfiguration());

        File.Exists(nestedPath).Should().BeTrue();
    }

    [Fact]
    public async Task ConfigurationFile_ShouldBeAtExpectedPath()
    {
        await _service.LoadConfigurationAsync();

        File.Exists(_configPath).Should().BeTrue();
        _configPath.Should().EndWith("config.json");
    }
}
