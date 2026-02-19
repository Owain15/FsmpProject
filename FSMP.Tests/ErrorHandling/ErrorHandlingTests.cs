using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FSMP.Core.Models;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.ErrorHandling;

/// <summary>
/// Tests for error handling and edge cases across the application.
/// </summary>
public class ErrorHandlingTests : IDisposable
{
    private readonly string _tempDir;

    public ErrorHandlingTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FSMP_ErrTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        for (int i = 0; i < 3; i++)
        {
            try
            {
                if (Directory.Exists(_tempDir))
                    Directory.Delete(_tempDir, recursive: true);
                return;
            }
            catch (IOException) { Thread.Sleep(100); }
        }
    }

    // --- Helpers ---

    private FsmpDbContext CreateSqliteContext(string dbName = "test.db")
    {
        var dbPath = Path.Combine(_tempDir, dbName);
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseSqlite($"Data Source={dbPath};Pooling=False")
            .Options;
        return new FsmpDbContext(options);
    }

    // ========== Corrupt config.json ==========

    [Fact]
    public async Task ConfigService_CorruptJson_ShouldReturnDefaults()
    {
        var configPath = Path.Combine(_tempDir, "config.json");
        await File.WriteAllTextAsync(configPath, "THIS IS NOT VALID JSON {{{");

        var service = new ConfigurationService(configPath);
        var config = await service.LoadConfigurationAsync();

        // Should get defaults, not throw
        config.Should().NotBeNull();
        config.DefaultVolume.Should().Be(75);
        config.AutoScanOnStartup.Should().BeTrue();
        config.LibraryPaths.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfigService_CorruptJson_ShouldOverwriteWithDefaults()
    {
        var configPath = Path.Combine(_tempDir, "config.json");
        await File.WriteAllTextAsync(configPath, "NOT JSON");

        var service = new ConfigurationService(configPath);
        await service.LoadConfigurationAsync();

        // File should now contain valid JSON
        var json = await File.ReadAllTextAsync(configPath);
        json.Should().Contain("DefaultVolume");
        json.Should().Contain("75");
    }

    [Fact]
    public async Task ConfigService_EmptyJson_ShouldReturnDefaults()
    {
        var configPath = Path.Combine(_tempDir, "config.json");
        await File.WriteAllTextAsync(configPath, "");

        var service = new ConfigurationService(configPath);
        var config = await service.LoadConfigurationAsync();

        config.Should().NotBeNull();
        config.DefaultVolume.Should().Be(75);
    }

    [Fact]
    public async Task ConfigService_NullJsonObject_ShouldReturnDefaults()
    {
        var configPath = Path.Combine(_tempDir, "config.json");
        await File.WriteAllTextAsync(configPath, "null");

        var service = new ConfigurationService(configPath);
        var config = await service.LoadConfigurationAsync();

        config.Should().NotBeNull();
        config.DefaultVolume.Should().Be(75);
    }

    // ========== Missing library paths ==========

    [Fact]
    public async Task LibraryScanService_MissingDirectory_ShouldThrow()
    {
        using var context = CreateSqliteContext();
        context.Database.EnsureCreated();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);

        var act = () => scanService.ScanLibraryAsync(@"C:\NonExistent\Path\That\Does\Not\Exist");
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task LibraryScanService_ScanAllWithMixedPaths_ShouldContinueAfterError()
    {
        // Create one valid library, one invalid
        var validLib = Path.Combine(_tempDir, "valid");
        Directory.CreateDirectory(validLib);

        using var context = CreateSqliteContext();
        context.Database.EnsureCreated();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);

        // ScanAllLibrariesAsync catches exceptions per-path
        var paths = new List<string> { validLib, @"C:\NonExistent\Path" };
        var result = await scanService.ScanAllLibrariesAsync(paths);

        // Should have errors from the missing path but not crash
        result.Errors.Should().NotBeEmpty();
    }

    // ========== Corrupt audio files ==========

    [Fact]
    public void MetadataService_CorruptFile_ShouldReturnEmptyMetadata()
    {
        // Create a file with random bytes (not a valid audio file)
        var corruptPath = Path.Combine(_tempDir, "corrupt.mp3");
        File.WriteAllBytes(corruptPath, new byte[] { 0xFF, 0xFE, 0x00, 0x01, 0x02, 0x03 });

        var service = new MetadataService();
        var metadata = service.ReadMetadata(corruptPath);

        metadata.Should().NotBeNull();
        metadata.Title.Should().BeNull();
        metadata.Artist.Should().BeNull();
    }

    [Fact]
    public void MetadataService_CorruptFile_ExtractAlbumArt_ShouldReturnNull()
    {
        var corruptPath = Path.Combine(_tempDir, "corrupt.mp3");
        File.WriteAllBytes(corruptPath, new byte[] { 0xFF, 0xFE, 0x00, 0x01 });

        var service = new MetadataService();
        var art = service.ExtractAlbumArt(corruptPath);

        art.Should().BeNull();
    }

    [Fact]
    public void MetadataService_CorruptFile_GetDuration_ShouldReturnNull()
    {
        var corruptPath = Path.Combine(_tempDir, "corrupt.mp3");
        File.WriteAllBytes(corruptPath, new byte[] { 0xFF, 0xFE, 0x00 });

        var service = new MetadataService();
        var duration = service.GetDuration(corruptPath);

        duration.Should().BeNull();
    }

    [Fact]
    public void MetadataService_CorruptFile_GetAudioProperties_ShouldReturnEmpty()
    {
        var corruptPath = Path.Combine(_tempDir, "corrupt.mp3");
        File.WriteAllBytes(corruptPath, new byte[] { 0xFF, 0xFE, 0x00 });

        var service = new MetadataService();
        var props = service.GetAudioProperties(corruptPath);

        props.Should().NotBeNull();
        props.BitRate.Should().BeNull();
        props.SampleRate.Should().BeNull();
    }

    [Fact]
    public void MetadataService_MissingFile_ShouldThrowFileNotFound()
    {
        var service = new MetadataService();

        var act = () => service.ReadMetadata(@"C:\does\not\exist.mp3");
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void MetadataService_NullPath_ShouldThrowArgumentNull()
    {
        var service = new MetadataService();

        var act = () => service.ReadMetadata(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ========== Corrupt audio files during scan ==========

    [Fact]
    public async Task LibraryScanService_CorruptFileInLibrary_ShouldLogErrorAndContinue()
    {
        // Create a library with one corrupt file
        var libDir = Path.Combine(_tempDir, "lib");
        Directory.CreateDirectory(libDir);
        File.WriteAllBytes(Path.Combine(libDir, "corrupt.mp3"), new byte[] { 0x00, 0x01, 0x02 });

        using var context = CreateSqliteContext();
        context.Database.EnsureCreated();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);

        // Scan should complete without throwing
        var result = await scanService.ScanLibraryAsync(libDir);

        // The corrupt file should either be imported (with empty metadata) or logged as error
        // Since MetadataService returns empty metadata for corrupt files, it will be imported
        (result.TracksAdded + result.Errors.Count).Should().BeGreaterThan(0);
    }

    // ========== File moved after scan ==========

    [Fact]
    public async Task TrackRepository_FileMovedAfterScan_TrackStillExistsInDb()
    {
        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();
        using var uow = new UnitOfWork(context);

        // Simulate a track that was scanned but file was later moved/deleted
        var filePath = Path.Combine(_tempDir, "moved.mp3");
        var track = new Track
        {
            Title = "Moved Song",
            FilePath = filePath,
            FileHash = "abc123",
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await uow.Tracks.AddAsync(track);
        await uow.SaveAsync();

        // File doesn't exist on disk
        File.Exists(filePath).Should().BeFalse();

        // But track still exists in DB
        var found = await uow.Tracks.GetByFilePathAsync(filePath);
        found.Should().NotBeNull();
        found!.Title.Should().Be("Moved Song");
    }

    // ========== AppStartup with corrupt config ==========

    [Fact]
    public async Task AppStartup_CorruptConfig_ShouldRecoverAndRun()
    {
        var configPath = Path.Combine(_tempDir, "config.json");
        var dbPath = Path.Combine(_tempDir, "app.db");
        await File.WriteAllTextAsync(configPath, "CORRUPT DATA!!!");

        var input = new StringReader("8\n");
        var output = new StringWriter();
        var app = new AppStartup(input, output, configPath, dbPath);

        await app.RunAsync();

        var text = output.ToString();
        text.Should().Contain("FSMP - File System Music Player");
        text.Should().Contain("Goodbye!");

        // Config should be replaced with valid defaults
        var json = await File.ReadAllTextAsync(configPath);
        json.Should().Contain("DefaultVolume");
    }

    // ========== AppStartup runs twice (idempotent migrations) ==========

    [Fact]
    public async Task AppStartup_MigrationAlreadyApplied_ShouldNotFail()
    {
        var configPath = Path.Combine(_tempDir, "config.json");
        var dbPath = Path.Combine(_tempDir, "double.db");

        // First run
        var app1 = new AppStartup(new StringReader("8\n"), new StringWriter(), configPath, dbPath);
        await app1.RunAsync();

        // Second run — migration already applied, should succeed
        var output2 = new StringWriter();
        var app2 = new AppStartup(new StringReader("8\n"), output2, configPath, dbPath);
        await app2.RunAsync();

        output2.ToString().Should().Contain("Database ready at:");
    }

    // ========== Edge: empty library paths list ==========

    [Fact]
    public async Task LibraryScanService_EmptyPathsList_ShouldReturnZeroCounts()
    {
        using var context = CreateSqliteContext();
        context.Database.EnsureCreated();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);

        var result = await scanService.ScanAllLibrariesAsync(new List<string>());

        result.TracksAdded.Should().Be(0);
        result.TracksUpdated.Should().Be(0);
        result.TracksRemoved.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    // ========== Edge: unsupported file extensions ignored ==========

    [Fact]
    public async Task LibraryScanService_UnsupportedFiles_ShouldBeIgnored()
    {
        var libDir = Path.Combine(_tempDir, "mixed");
        Directory.CreateDirectory(libDir);
        File.WriteAllText(Path.Combine(libDir, "readme.txt"), "not audio");
        File.WriteAllText(Path.Combine(libDir, "image.jpg"), "not audio");
        File.WriteAllText(Path.Combine(libDir, "doc.pdf"), "not audio");

        using var context = CreateSqliteContext();
        context.Database.EnsureCreated();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);

        var result = await scanService.ScanLibraryAsync(libDir);

        result.TracksAdded.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }
}
