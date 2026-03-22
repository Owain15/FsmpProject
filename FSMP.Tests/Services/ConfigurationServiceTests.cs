using System.Text.Json;
using FluentAssertions;
using FSMP.Core.Models;
using FSMP.Core.Services;

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
        config.DatabasePath.Should().BeEmpty("caller provides actual path via DI");
        config.AutoScanOnStartup.Should().BeTrue();
        config.DefaultVolume.Should().Be(75);
        config.ResumeSession.Should().BeTrue();
        config.LastPlayedTrackPath.Should().BeNull();
        config.AutoPlayOnStartup.Should().BeFalse();
        config.TextSize.Should().Be("Medium");
        config.DoubleClickAction.Should().Be("PlayNow");
        config.DefaultSortOrder.Should().Be("Artist");
        config.DefaultOrganizeMode.Should().Be("Copy");
        config.DefaultDuplicateStrategy.Should().Be("Skip");
        config.UnknownArtistName.Should().Be("Unknown Artist");
        config.UnknownAlbumName.Should().Be("Unknown Album");
    }

    [Fact]
    public async Task SaveAndLoad_ShouldRoundTrip_NewProperties()
    {
        var config = new Configuration
        {
            ResumeSession = false,
            AutoPlayOnStartup = true,
            TextSize = "Large",
            DoubleClickAction = "AddToQueue",
            DefaultSortOrder = "Title",
            DefaultOrganizeMode = "Move",
            DefaultDuplicateStrategy = "Overwrite",
            UnknownArtistName = "No Artist",
            UnknownAlbumName = "No Album"
        };

        await _service.SaveConfigurationAsync(config);
        var loaded = await _service.LoadConfigurationAsync();

        loaded.ResumeSession.Should().BeFalse();
        loaded.AutoPlayOnStartup.Should().BeTrue();
        loaded.TextSize.Should().Be("Large");
        loaded.DoubleClickAction.Should().Be("AddToQueue");
        loaded.DefaultSortOrder.Should().Be("Title");
        loaded.DefaultOrganizeMode.Should().Be("Move");
        loaded.DefaultDuplicateStrategy.Should().Be("Overwrite");
        loaded.UnknownArtistName.Should().Be("No Artist");
        loaded.UnknownAlbumName.Should().Be("No Album");
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
            ResumeSession = false,
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
            ResumeSession = true,
            LastPlayedTrackPath = @"C:\MyMusic\track.wav"
        };
        await _service.SaveConfigurationAsync(original);

        var loaded = await _service.LoadConfigurationAsync();

        loaded.LibraryPaths.Should().ContainSingle().Which.Should().Be(@"C:\MyMusic");
        loaded.DatabasePath.Should().Be(@"C:\db\fsmp.db");
        loaded.AutoScanOnStartup.Should().BeFalse();
        loaded.DefaultVolume.Should().Be(90);
        loaded.ResumeSession.Should().BeTrue();
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
