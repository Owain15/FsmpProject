using FluentAssertions;
using FsmpDataAcsses;
using FsmpDataAcsses.Services;
using FsmpLibrary.Models;
using FsmpLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FSMP.Tests.Services;

public class LibraryScanServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FsmpDbContext _context;
    private readonly UnitOfWork _unitOfWork;
    private readonly Mock<IMetadataService> _metadataMock;
    private readonly LibraryScanService _service;
    private int _fileCounter;

    public LibraryScanServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FSMP_ScanTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);

        var options = new DbContextOptionsBuilder<FsmpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new FsmpDbContext(options);
        _context.Database.EnsureCreated();

        _unitOfWork = new UnitOfWork(_context);
        _metadataMock = new Mock<IMetadataService>();

        // Default: return empty metadata
        _metadataMock.Setup(m => m.ReadMetadata(It.IsAny<string>()))
            .Returns((string path) => new TrackMetadata
            {
                Title = Path.GetFileNameWithoutExtension(path),
            });

        _service = new LibraryScanService(_unitOfWork, _metadataMock.Object);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // --- Helpers ---

    private string CreateWavFile(string? subDir = null, string? fileName = null)
    {
        var dir = subDir != null ? Path.Combine(_tempDir, subDir) : _tempDir;
        Directory.CreateDirectory(dir);
        var counter = _fileCounter++;
        fileName ??= $"track_{counter}.wav";
        var filePath = Path.Combine(dir, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(stream);

        int sampleRate = 44100, numSamples = 4410;
        short channels = 1, bitsPerSample = 16;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        short blockAlign = (short)(channels * bitsPerSample / 8);
        int dataSize = numSamples * blockAlign;

        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8);
        writer.Write("fmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);
        writer.Write("data"u8);
        writer.Write(dataSize);
        // Write unique data per file so each has a distinct hash
        var data = new byte[dataSize];
        BitConverter.TryWriteBytes(data.AsSpan(), counter);
        writer.Write(data);

        return filePath;
    }

    private string CreateFakeFile(string extension, string? subDir = null, string? fileName = null)
    {
        var dir = subDir != null ? Path.Combine(_tempDir, subDir) : _tempDir;
        Directory.CreateDirectory(dir);
        var counter = _fileCounter++;
        fileName ??= $"track_{counter}{extension}";
        var filePath = Path.Combine(dir, fileName);
        // Write unique bytes per file so each has a distinct hash
        var bytes = BitConverter.GetBytes(counter);
        File.WriteAllBytes(filePath, bytes);
        return filePath;
    }

    private void SetupMetadataFor(string filePath, string? title = null, string? artist = null,
        string? album = null, int? year = null)
    {
        _metadataMock.Setup(m => m.ReadMetadata(filePath))
            .Returns(new TrackMetadata
            {
                Title = title,
                Artist = artist,
                Album = album,
                Year = year,
                Duration = TimeSpan.FromSeconds(180),
                BitRate = 320,
                SampleRate = 44100,
            });
    }

    // ========== IsSupportedFormat Tests ==========

    [Theory]
    [InlineData(".wav", true)]
    [InlineData(".wma", true)]
    [InlineData(".mp3", true)]
    [InlineData(".WAV", true)]
    [InlineData(".Mp3", true)]
    [InlineData(".flac", false)]
    [InlineData(".ogg", false)]
    [InlineData(".txt", false)]
    [InlineData("", false)]
    public void IsSupportedFormat_ShouldFilterCorrectly(string extension, bool expected)
    {
        LibraryScanService.IsSupportedFormat(extension).Should().Be(expected);
    }

    // ========== CalculateFileHash Tests ==========

    [Fact]
    public void CalculateFileHash_ShouldReturnConsistentHash()
    {
        var filePath = CreateWavFile();

        var hash1 = LibraryScanService.CalculateFileHash(filePath);
        var hash2 = LibraryScanService.CalculateFileHash(filePath);

        hash1.Should().NotBeNullOrWhiteSpace();
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void CalculateFileHash_ShouldReturnDifferentHash_ForDifferentFiles()
    {
        var file1 = CreateWavFile(fileName: "a.wav");
        var file2 = CreateFakeFile(".mp3", fileName: "b.mp3");

        var hash1 = LibraryScanService.CalculateFileHash(file1);
        var hash2 = LibraryScanService.CalculateFileHash(file2);

        hash1.Should().NotBe(hash2);
    }

    // ========== ScanLibraryAsync Tests ==========

    [Fact]
    public async Task ScanLibraryAsync_ShouldImportWavFiles()
    {
        var wavPath = CreateWavFile();
        SetupMetadataFor(wavPath, title: "WAV Track");

        var result = await _service.ScanLibraryAsync(_tempDir);

        result.TracksAdded.Should().Be(1);
        var tracks = await _unitOfWork.Tracks.GetAllAsync();
        tracks.Should().ContainSingle().Which.Title.Should().Be("WAV Track");
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldImportWmaFiles()
    {
        var wmaPath = CreateFakeFile(".wma");
        SetupMetadataFor(wmaPath, title: "WMA Track");

        var result = await _service.ScanLibraryAsync(_tempDir);

        result.TracksAdded.Should().Be(1);
        var tracks = await _unitOfWork.Tracks.GetAllAsync();
        tracks.Should().ContainSingle().Which.Title.Should().Be("WMA Track");
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldImportMp3Files()
    {
        var mp3Path = CreateFakeFile(".mp3");
        SetupMetadataFor(mp3Path, title: "MP3 Track");

        var result = await _service.ScanLibraryAsync(_tempDir);

        result.TracksAdded.Should().Be(1);
        var tracks = await _unitOfWork.Tracks.GetAllAsync();
        tracks.Should().ContainSingle().Which.Title.Should().Be("MP3 Track");
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldIgnoreUnsupportedFormats()
    {
        CreateFakeFile(".flac");
        CreateFakeFile(".ogg");
        var wavPath = CreateWavFile();
        SetupMetadataFor(wavPath, title: "Only WAV");

        var result = await _service.ScanLibraryAsync(_tempDir);

        result.TracksAdded.Should().Be(1);
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldScanSubdirectories()
    {
        var wav1 = CreateWavFile(subDir: "Artist/Album1");
        var wav2 = CreateWavFile(subDir: "Artist/Album2");
        SetupMetadataFor(wav1, title: "Track 1");
        SetupMetadataFor(wav2, title: "Track 2");

        var result = await _service.ScanLibraryAsync(_tempDir);

        result.TracksAdded.Should().Be(2);
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldSkipDuplicates_BySameHash()
    {
        // Create two files with identical content (same hash)
        var dir1 = Path.Combine(_tempDir, "copy1");
        var dir2 = Path.Combine(_tempDir, "copy2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);

        var content = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // "RIFF" bytes
        var path1 = Path.Combine(dir1, "track.wav");
        var path2 = Path.Combine(dir2, "track.wav");
        File.WriteAllBytes(path1, content);
        File.WriteAllBytes(path2, content);

        SetupMetadataFor(path1, title: "Track Original");
        SetupMetadataFor(path2, title: "Track Duplicate");

        var result = await _service.ScanLibraryAsync(_tempDir);

        result.TracksAdded.Should().Be(1);
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldHandleCorruptFiles_AndContinue()
    {
        var goodPath = CreateWavFile();
        var corruptPath = CreateFakeFile(".mp3", fileName: "corrupt.mp3");
        SetupMetadataFor(goodPath, title: "Good Track");
        _metadataMock.Setup(m => m.ReadMetadata(corruptPath))
            .Throws(new TagLib.CorruptFileException("Corrupt"));

        var result = await _service.ScanLibraryAsync(_tempDir);

        result.TracksAdded.Should().Be(1);
        result.Errors.Should().HaveCount(1);
        result.Errors[0].Should().Contain("Corrupt");
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldThrow_ForMissingDirectory()
    {
        var act = () => _service.ScanLibraryAsync(@"C:\nonexistent\path");

        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldPopulateScanResult_Correctly()
    {
        var wav1 = CreateWavFile();
        var wav2 = CreateWavFile();
        SetupMetadataFor(wav1, title: "Track 1");
        SetupMetadataFor(wav2, title: "Track 2");

        var result = await _service.ScanLibraryAsync(_tempDir);

        result.TracksAdded.Should().Be(2);
        result.TracksUpdated.Should().Be(0);
        result.TracksRemoved.Should().Be(0);
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
        result.Errors.Should().BeEmpty();
    }

    // ========== Artist/Album Auto-Creation Tests ==========

    [Fact]
    public async Task ScanLibraryAsync_ShouldCreateArtist_FromMetadata()
    {
        var wavPath = CreateWavFile();
        SetupMetadataFor(wavPath, title: "Track", artist: "Bonobo");

        await _service.ScanLibraryAsync(_tempDir);

        var artists = await _unitOfWork.Artists.GetAllAsync();
        artists.Should().ContainSingle().Which.Name.Should().Be("Bonobo");

        var tracks = await _unitOfWork.Tracks.GetAllAsync();
        tracks.First().ArtistId.Should().NotBeNull();
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldReuseExistingArtist()
    {
        var wav1 = CreateWavFile();
        var wav2 = CreateWavFile();
        SetupMetadataFor(wav1, title: "Track 1", artist: "Bonobo");
        SetupMetadataFor(wav2, title: "Track 2", artist: "Bonobo");

        await _service.ScanLibraryAsync(_tempDir);

        var artists = await _unitOfWork.Artists.GetAllAsync();
        artists.Should().ContainSingle();

        var tracks = (await _unitOfWork.Tracks.GetAllAsync()).ToList();
        tracks.Should().HaveCount(2);
        tracks[0].ArtistId.Should().Be(tracks[1].ArtistId);
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldCreateAlbum_FromMetadata()
    {
        var wavPath = CreateWavFile();
        SetupMetadataFor(wavPath, title: "Track", artist: "Bonobo", album: "Migration", year: 2017);

        await _service.ScanLibraryAsync(_tempDir);

        var albums = await _unitOfWork.Albums.GetAllAsync();
        var album = albums.Should().ContainSingle().Subject;
        album.Title.Should().Be("Migration");
        album.Year.Should().Be(2017);
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldReuseExistingAlbum()
    {
        var wav1 = CreateWavFile();
        var wav2 = CreateWavFile();
        SetupMetadataFor(wav1, title: "Track 1", artist: "Bonobo", album: "Migration");
        SetupMetadataFor(wav2, title: "Track 2", artist: "Bonobo", album: "Migration");

        await _service.ScanLibraryAsync(_tempDir);

        var albums = await _unitOfWork.Albums.GetAllAsync();
        albums.Should().ContainSingle();

        var tracks = (await _unitOfWork.Tracks.GetAllAsync()).ToList();
        tracks.Should().HaveCount(2);
        tracks[0].AlbumId.Should().Be(tracks[1].AlbumId);
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldAssignFileExtension()
    {
        var wavPath = CreateWavFile();
        SetupMetadataFor(wavPath, title: "WAV Track");

        await _service.ScanLibraryAsync(_tempDir);

        var tracks = await _unitOfWork.Tracks.GetAllAsync();
        var track = tracks.First();
        track.FileExtensionId.Should().NotBeNull();

        // Verify it matches the seeded "wav" extension
        var ext = await _unitOfWork.FileExtensions.GetByIdAsync(track.FileExtensionId!.Value);
        ext.Should().NotBeNull();
        ext!.Extension.Should().Be("wav");
    }

    [Fact]
    public async Task ScanLibraryAsync_ShouldUseFallbackTitle_WhenNoMetadataTitle()
    {
        var wavPath = CreateWavFile(fileName: "My Song.wav");
        _metadataMock.Setup(m => m.ReadMetadata(wavPath))
            .Returns(new TrackMetadata()); // null title

        await _service.ScanLibraryAsync(_tempDir);

        var tracks = await _unitOfWork.Tracks.GetAllAsync();
        tracks.Should().ContainSingle().Which.Title.Should().Be("My Song");
    }

    // ========== ScanAllLibrariesAsync Tests ==========

    [Fact]
    public async Task ScanAllLibrariesAsync_ShouldAggregateResults()
    {
        var dir1 = Path.Combine(_tempDir, "lib1");
        var dir2 = Path.Combine(_tempDir, "lib2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);

        var wav1 = CreateFakeFile(".wav", subDir: "lib1", fileName: "a.wav");
        var wav2 = CreateFakeFile(".wav", subDir: "lib2", fileName: "b.wav");
        SetupMetadataFor(wav1, title: "Track A");
        SetupMetadataFor(wav2, title: "Track B");

        var result = await _service.ScanAllLibrariesAsync(new List<string> { dir1, dir2 });

        result.TracksAdded.Should().Be(2);
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task ScanAllLibrariesAsync_ShouldHandleEmptyList()
    {
        var result = await _service.ScanAllLibrariesAsync(new List<string>());

        result.TracksAdded.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }
}
