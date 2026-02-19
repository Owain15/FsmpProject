using FluentAssertions;
using FsmpConsole;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FSMP.Core.Models;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FSMP.Tests.Integration;

/// <summary>
/// End-to-end tests exercising complete workflows with real SQLite databases.
/// These tests verify data flows through the full stack: UI → Service → Repository → DB.
/// </summary>
public class EndToEndTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _configPath;
    private readonly string _dbPath;
    private readonly string _musicDir;

    public EndToEndTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FSMP_E2E", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _configPath = Path.Combine(_tempDir, "config.json");
        _dbPath = Path.Combine(_tempDir, "fsmp.db");
        _musicDir = Path.Combine(_tempDir, "music");
        Directory.CreateDirectory(_musicDir);
    }

    public void Dispose()
    {
        // SQLite may hold file locks briefly after dispose
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

    private FsmpDbContext CreateSqliteContext()
    {
        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseSqlite($"Data Source={_dbPath};Pooling=False")
            .Options;
        return new FsmpDbContext(options);
    }

    private static int _wavSeed = 0;

    private void CreateWavFile(string path, int durationMs = 500)
    {
        // Create a minimal valid WAV file with unique content per call
        int seed = Interlocked.Increment(ref _wavSeed);
        int sampleRate = 44100;
        short channels = 1;
        short bitsPerSample = 16;
        int numSamples = sampleRate * durationMs / 1000;
        int dataSize = numSamples * channels * (bitsPerSample / 8);

        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);

        // fmt chunk
        writer.Write("fmt "u8);
        writer.Write(16); // chunk size
        writer.Write((short)1); // PCM
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8); // byte rate
        writer.Write((short)(channels * bitsPerSample / 8)); // block align
        writer.Write(bitsPerSample);

        // data chunk
        writer.Write("data"u8);
        writer.Write(dataSize);

        // Write unique tone data (seed varies frequency so each file hashes differently)
        for (int i = 0; i < numSamples; i++)
            writer.Write((short)(Math.Sin(2.0 * Math.PI * (seed * 100 + 200) * i / sampleRate) * 10000));
    }

    private void CreateMusicLibrary(string basePath, string artistName, string albumName, params string[] trackNames)
    {
        var albumDir = Path.Combine(basePath, artistName, albumName);
        Directory.CreateDirectory(albumDir);
        foreach (var name in trackNames)
        {
            CreateWavFile(Path.Combine(albumDir, $"{name}.wav"));
        }
    }

    // ========== Test 1: Fresh Install Workflow ==========

    [Fact]
    public async Task FreshInstall_ShouldCreateConfigAndDatabase()
    {
        // Run app with immediate exit
        var input = new StringReader("8\n");
        var output = new StringWriter();
        var app = new AppStartup(input, output, _configPath, _dbPath);

        await app.RunAsync();

        // Config file should exist
        File.Exists(_configPath).Should().BeTrue();

        // Database file should exist
        File.Exists(_dbPath).Should().BeTrue();

        // Config should have default values
        var configService = new ConfigurationService(_configPath);
        var config = await configService.LoadConfigurationAsync();
        config.DefaultVolume.Should().Be(75);
        config.AutoScanOnStartup.Should().BeTrue();
        config.LibraryPaths.Should().BeEmpty();

        // Output should show welcome and goodbye
        var text = output.ToString();
        text.Should().Contain("FSMP - File System Music Player");
        text.Should().Contain("Goodbye!");
    }

    // ========== Test 2: Full Workflow ==========

    [Fact]
    public async Task FullWorkflow_AddLibrary_Scan_Browse_ViewStats()
    {
        // Setup: create music files
        CreateMusicLibrary(_musicDir, "TestArtist", "TestAlbum", "Track01", "Track02");

        // Step 1: Start app, add library path, exit
        var input1 = new StringReader($"6\na\n{_musicDir}\n\n8\n");
        var output1 = new StringWriter();
        var app1 = new AppStartup(input1, output1, _configPath, _dbPath);
        await app1.RunAsync();
        output1.ToString().Should().Contain("Path added");

        // Step 2: Start app again, scan libraries, then browse, then view stats, then exit
        // AutoScanOnStartup is true but we just added the path so it should auto-scan
        var input2 = new StringReader("1\n0\n5\n8\n");
        var output2 = new StringWriter();
        var app2 = new AppStartup(input2, output2, _configPath, _dbPath);
        await app2.RunAsync();

        var text = output2.ToString();
        // Auto-scan should have found tracks
        text.Should().Contain("Auto-scanning");
        text.Should().Contain("added");
        // View statistics should show tracks
        text.Should().Contain("Library Statistics");
    }

    // ========== Test 3: Multi-Library Scenario ==========

    [Fact]
    public async Task MultiLibrary_ScanMultiplePaths_ShouldImportAll()
    {
        // Create two separate library directories
        var lib1 = Path.Combine(_musicDir, "Library1");
        var lib2 = Path.Combine(_musicDir, "Library2");
        CreateMusicLibrary(lib1, "ArtistA", "AlbumA", "SongA1", "SongA2");
        CreateMusicLibrary(lib2, "ArtistB", "AlbumB", "SongB1");

        // Setup DB and services
        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);

        // Scan both libraries
        var result = await scanService.ScanAllLibrariesAsync(new List<string> { lib1, lib2 });

        result.TracksAdded.Should().Be(3);

        // Verify all tracks in DB
        var tracks = (await uow.Tracks.GetAllAsync()).ToList();
        tracks.Should().HaveCount(3);

        // Verify tracks have filenames as titles (WAV files have no metadata tags)
        tracks.Select(t => t.Title).Should().Contain("SongA1");
        tracks.Select(t => t.Title).Should().Contain("SongA2");
        tracks.Select(t => t.Title).Should().Contain("SongB1");

        // All tracks should have unique file hashes
        tracks.Select(t => t.FileHash).Distinct().Should().HaveCount(3);
    }

    // ========== Test 4: Statistics After Plays ==========

    [Fact]
    public async Task Statistics_AfterMultiplePlays_ShouldAccumulate()
    {
        // Setup DB
        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();
        using var uow = new UnitOfWork(context);

        // Create tracks
        var artist = new Artist { Name = "Stats Artist" };
        await uow.Artists.AddAsync(artist);
        await uow.SaveAsync();

        var tracks = new List<Track>();
        for (int i = 1; i <= 3; i++)
        {
            var track = new Track
            {
                Title = $"Song {i}",
                FilePath = Path.Combine(_musicDir, $"song{i}.wav"),
                FileHash = Guid.NewGuid().ToString(),
                ImportedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ArtistId = artist.ArtistId,
            };
            await uow.Tracks.AddAsync(track);
            tracks.Add(track);
        }
        await uow.SaveAsync();

        // Record plays
        var trackingService = new PlaybackTrackingService(uow);
        for (int i = 0; i < 10; i++)
        {
            var track = tracks[i % 3]; // Rotate through tracks
            await trackingService.RecordPlaybackAsync(track, TimeSpan.FromMinutes(3), completed: true, skipped: false);
        }

        // Verify statistics
        var statsService = new StatisticsService(uow);

        var totalPlays = await statsService.GetTotalPlayCountAsync();
        totalPlays.Should().Be(10);

        var totalTime = await statsService.GetTotalListeningTimeAsync();
        totalTime.Should().Be(TimeSpan.FromMinutes(30));

        var totalTracks = await statsService.GetTotalTrackCountAsync();
        totalTracks.Should().Be(3);

        // Most played should reflect distribution (4, 3, 3 plays)
        var mostPlayed = (await statsService.GetMostPlayedTracksAsync(3)).ToList();
        mostPlayed.Should().HaveCount(3);
        mostPlayed[0].PlayCount.Should().Be(4); // Song 1 gets plays 0,3,6,9
    }

    // ========== Test 5: Persistence Across Restart ==========

    [Fact]
    public async Task Persistence_DataSurvivesRestart()
    {
        // First session: create data
        {
            using var context = CreateSqliteContext();
            await context.Database.MigrateAsync();
            using var uow = new UnitOfWork(context);

            var artist = new Artist { Name = "Persistent Artist" };
            await uow.Artists.AddAsync(artist);
            await uow.SaveAsync();

            var track = new Track
            {
                Title = "Persistent Song",
                FilePath = @"C:\Music\persistent.wav",
                FileHash = "abc123",
                ImportedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ArtistId = artist.ArtistId,
                PlayCount = 5,
                IsFavorite = true,
                Rating = 4,
                CustomTitle = "My Custom Title",
            };
            await uow.Tracks.AddAsync(track);
            await uow.SaveAsync();
        }

        // Second session: read data back
        {
            using var context = CreateSqliteContext();
            // No need to migrate again — DB already exists
            using var uow = new UnitOfWork(context);

            var tracks = (await uow.Tracks.GetAllAsync()).ToList();
            tracks.Should().HaveCount(1);

            var track = tracks[0];
            track.Title.Should().Be("Persistent Song");
            track.CustomTitle.Should().Be("My Custom Title");
            track.DisplayTitle.Should().Be("My Custom Title");
            track.PlayCount.Should().Be(5);
            track.IsFavorite.Should().BeTrue();
            track.Rating.Should().Be(4);

            var artists = (await uow.Artists.GetAllAsync()).ToList();
            artists.Should().HaveCount(1);
            artists[0].Name.Should().Be("Persistent Artist");
        }
    }

    // ========== Test 6: Custom Metadata Overrides ==========

    [Fact]
    public async Task CustomMetadata_OverridesFileMetadata()
    {
        // Setup
        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();
        using var uow = new UnitOfWork(context);

        var artist = new Artist { Name = "File Artist" };
        await uow.Artists.AddAsync(artist);
        await uow.SaveAsync();

        var album = new Album { Title = "File Album", ArtistId = artist.ArtistId };
        await uow.Albums.AddAsync(album);
        await uow.SaveAsync();

        var track = new Track
        {
            Title = "File Title",
            FilePath = @"C:\Music\test.mp3",
            FileHash = Guid.NewGuid().ToString(),
            ImportedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ArtistId = artist.ArtistId,
            AlbumId = album.AlbumId,
        };
        await uow.Tracks.AddAsync(track);
        await uow.SaveAsync();

        // Display properties should show file metadata initially
        track.DisplayTitle.Should().Be("File Title");
        track.DisplayArtist.Should().Be("File Artist");
        track.DisplayAlbum.Should().Be("File Album");

        // Edit metadata via MetadataEditor
        var editInput = new StringReader(
            "File Title\n" +   // search term
            "1\n" +            // select first result
            "Custom Title\n" + // new title
            "Custom Artist\n" + // new artist
            "Custom Album\n" + // new album
            "5\n" +            // rating
            "y\n" +            // favorite
            "Great song\n"     // comment
        );
        var editOutput = new StringWriter();
        var editor = new MetadataEditor(uow, editInput, editOutput);
        await editor.RunAsync();

        // Verify custom overrides in DB
        var updated = await uow.Tracks.GetByIdAsync(track.TrackId);
        updated!.CustomTitle.Should().Be("Custom Title");
        updated.CustomArtist.Should().Be("Custom Artist");
        updated.CustomAlbum.Should().Be("Custom Album");
        updated.Rating.Should().Be(5);
        updated.IsFavorite.Should().BeTrue();
        updated.Comment.Should().Be("Great song");

        // Display properties should now show custom overrides
        updated.DisplayTitle.Should().Be("Custom Title");
        updated.DisplayArtist.Should().Be("Custom Artist");
        updated.DisplayAlbum.Should().Be("Custom Album");

        // Original file metadata should be unchanged
        updated.Title.Should().Be("File Title");
    }

    // ========== Test 7: Duplicate File Detection ==========

    [Fact]
    public async Task DuplicateDetection_SameFile_ShouldNotDuplicate()
    {
        // Create one WAV file
        var artistDir = Path.Combine(_musicDir, "DupArtist", "DupAlbum");
        Directory.CreateDirectory(artistDir);
        CreateWavFile(Path.Combine(artistDir, "Song.wav"));

        // Setup DB and scan
        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);

        // First scan
        var result1 = await scanService.ScanLibraryAsync(_musicDir);
        result1.TracksAdded.Should().Be(1);

        // Second scan of the same directory — no new tracks
        var scanService2 = new LibraryScanService(uow, metadataService);
        var result2 = await scanService2.ScanLibraryAsync(_musicDir);
        result2.TracksAdded.Should().Be(0);

        // Only 1 track in DB
        var count = await uow.Tracks.CountAsync();
        count.Should().Be(1);
    }

    // ========== Test 8: Scan Imports Tracks Into Database ==========

    [Fact]
    public async Task ScanImportsTracks_ShouldBeQueryableAfterScan()
    {
        // Create music files
        CreateMusicLibrary(_musicDir, "ScanArtist", "ScanAlbum", "ScanTrack1", "ScanTrack2");

        // Setup DB, scan
        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);
        var result = await scanService.ScanLibraryAsync(_musicDir);

        result.TracksAdded.Should().Be(2);

        // Tracks should exist in DB and be queryable
        var tracks = (await uow.Tracks.GetAllAsync()).ToList();
        tracks.Should().HaveCount(2);

        // Track titles should fall back to filenames (WAV files have no metadata tags)
        tracks.Select(t => t.Title).Should().Contain("ScanTrack1");
        tracks.Select(t => t.Title).Should().Contain("ScanTrack2");
    }

    // ========== Test 9: Statistics Viewer After Scan ==========

    [Fact]
    public async Task StatisticsViewer_AfterScan_ShouldShowTrackCount()
    {
        // Create music files and scan
        CreateMusicLibrary(_musicDir, "StatsArtist", "StatsAlbum", "StatsTrack1", "StatsTrack2");

        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();
        using var uow = new UnitOfWork(context);
        var metadataService = new MetadataService();
        var scanService = new LibraryScanService(uow, metadataService);
        await scanService.ScanLibraryAsync(_musicDir);

        // View stats via StatisticsViewer
        var statsService = new StatisticsService(uow);
        var statsInput = new StringReader("1\n0\n"); // Overview then back
        var statsOutput = new StringWriter();
        var viewer = new StatisticsViewer(statsService, statsInput, statsOutput);
        await viewer.RunAsync();

        var text = statsOutput.ToString();
        text.Should().Contain("Total tracks:   2");
    }

    // ========== Test 10: Config Persistence ==========

    [Fact]
    public async Task ConfigPersistence_ChangesPreservedAcrossRestart()
    {
        var configService = new ConfigurationService(_configPath);

        // First session: modify config
        await configService.AddLibraryPathAsync(@"C:\Music\Library1");
        await configService.AddLibraryPathAsync(@"D:\Audio");

        // Second session: load config
        var configService2 = new ConfigurationService(_configPath);
        var config = await configService2.LoadConfigurationAsync();

        config.LibraryPaths.Should().HaveCount(2);
        config.LibraryPaths.Should().Contain(@"C:\Music\Library1");
        config.LibraryPaths.Should().Contain(@"D:\Audio");
    }

    // ========== Test 11: Favorites Workflow ==========

    [Fact]
    public async Task FavoritesWorkflow_MarkAndRetrieve()
    {
        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();
        using var uow = new UnitOfWork(context);

        // Create tracks
        for (int i = 1; i <= 5; i++)
        {
            await uow.Tracks.AddAsync(new Track
            {
                Title = $"Track {i}",
                FilePath = Path.Combine(_musicDir, $"track{i}.wav"),
                FileHash = Guid.NewGuid().ToString(),
                ImportedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsFavorite = i <= 2, // First 2 are favorites
            });
        }
        await uow.SaveAsync();

        var statsService = new StatisticsService(uow);
        var favorites = (await statsService.GetFavoritesAsync()).ToList();
        favorites.Should().HaveCount(2);
        favorites.Select(t => t.Title).Should().Contain("Track 1");
        favorites.Select(t => t.Title).Should().Contain("Track 2");
    }

    // ========== Test 12: Seed Data Present After Migration ==========

    [Fact]
    public async Task Migration_SeedDataPresent()
    {
        using var context = CreateSqliteContext();
        await context.Database.MigrateAsync();

        // Genre seeds
        var genres = await context.Genres.ToListAsync();
        genres.Should().HaveCount(5);
        genres.Select(g => g.Name).Should().Contain("Rock");
        genres.Select(g => g.Name).Should().Contain("Jazz");

        // FileExtension seeds
        var extensions = await context.FileExtensions.ToListAsync();
        extensions.Should().HaveCount(3);
        extensions.Select(e => e.Extension).Should().Contain("wav");
        extensions.Select(e => e.Extension).Should().Contain("mp3");
    }
}
