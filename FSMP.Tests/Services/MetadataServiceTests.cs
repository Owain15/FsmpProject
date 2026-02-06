using FluentAssertions;
using FsmpLibrary.Services;

namespace FSMP.Tests.Services;

public class MetadataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly MetadataService _service;
    private int _fileCounter;

    public MetadataServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "FSMP_Tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
        _service = new MetadataService();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }
    }

    // --- Helper: create a minimal valid WAV file ---
    private string CreateWavFile(string? fileName = null, int sampleRate = 44100,
        short channels = 1, short bitsPerSample = 16, int numSamples = 44100)
    {
        fileName ??= $"test_{_fileCounter++}.wav";
        var filePath = Path.Combine(_tempDir, fileName);
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        short blockAlign = (short)(channels * bitsPerSample / 8);
        int dataSize = numSamples * blockAlign;

        using var stream = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(stream);

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize); // ChunkSize
        writer.Write("WAVE"u8);

        // fmt sub-chunk
        writer.Write("fmt "u8);
        writer.Write(16);              // Subchunk1Size (PCM)
        writer.Write((short)1);        // AudioFormat (PCM)
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);

        // data sub-chunk (silence)
        writer.Write("data"u8);
        writer.Write(dataSize);
        writer.Write(new byte[dataSize]);

        return filePath;
    }

    private string CreateWavFileWithMetadata(string? title = null, string? artist = null,
        string? album = null, uint year = 0, string? genre = null, uint trackNumber = 0,
        uint discNumber = 0)
    {
        var filePath = CreateWavFile();

        // Use TagLib to write metadata into the WAV
        using var tagFile = TagLib.File.Create(filePath);
        if (title != null) tagFile.Tag.Title = title;
        if (artist != null) tagFile.Tag.Performers = new[] { artist };
        if (album != null) tagFile.Tag.Album = album;
        if (year > 0) tagFile.Tag.Year = year;
        if (genre != null) tagFile.Tag.Genres = new[] { genre };
        if (trackNumber > 0) tagFile.Tag.Track = trackNumber;
        if (discNumber > 0) tagFile.Tag.Disc = discNumber;
        tagFile.Save();

        return filePath;
    }

    private string CreateCorruptFile()
    {
        var filePath = Path.Combine(_tempDir, $"corrupt_{_fileCounter++}.mp3");
        var random = new Random(42);
        var bytes = new byte[256];
        random.NextBytes(bytes);
        File.WriteAllBytes(filePath, bytes);
        return filePath;
    }

    // ========== ReadMetadata Tests ==========

    [Fact]
    public void ReadMetadata_ShouldExtractTitle()
    {
        var wavPath = CreateWavFileWithMetadata(title: "Test Title");

        var metadata = _service.ReadMetadata(wavPath);

        metadata.Title.Should().Be("Test Title");
    }

    [Fact]
    public void ReadMetadata_ShouldExtractArtist()
    {
        var wavPath = CreateWavFileWithMetadata(artist: "AC/DC");

        var metadata = _service.ReadMetadata(wavPath);

        metadata.Artist.Should().Be("AC/DC");
    }

    [Fact]
    public void ReadMetadata_ShouldExtractAlbum()
    {
        var wavPath = CreateWavFileWithMetadata(album: "Highway to Hell");

        var metadata = _service.ReadMetadata(wavPath);

        metadata.Album.Should().Be("Highway to Hell");
    }

    [Fact]
    public void ReadMetadata_ShouldExtractMultipleFields()
    {
        var wavPath = CreateWavFileWithMetadata(
            title: "My Song", artist: "My Artist", album: "My Album",
            year: 2024, genre: "Rock", trackNumber: 3, discNumber: 2);

        var metadata = _service.ReadMetadata(wavPath);

        metadata.Title.Should().Be("My Song");
        metadata.Artist.Should().Be("My Artist");
        metadata.Album.Should().Be("My Album");
        metadata.Year.Should().Be(2024);
        metadata.Genre.Should().Be("Rock");
        metadata.TrackNumber.Should().Be(3);
        metadata.DiscNumber.Should().Be(2);
    }

    [Fact]
    public void ReadMetadata_ShouldReturnNulls_ForFileWithNoTags()
    {
        var wavPath = CreateWavFile();

        var metadata = _service.ReadMetadata(wavPath);

        metadata.Title.Should().BeNull();
        metadata.Artist.Should().BeNull();
        metadata.Album.Should().BeNull();
        metadata.Year.Should().BeNull();
        metadata.Genre.Should().BeNull();
        metadata.TrackNumber.Should().BeNull();
        metadata.DiscNumber.Should().BeNull();
    }

    [Fact]
    public void ReadMetadata_ShouldExtractDuration()
    {
        // 44100 samples at 44100 Hz = 1 second
        var wavPath = CreateWavFile(numSamples: 44100);

        var metadata = _service.ReadMetadata(wavPath);

        metadata.Duration.Should().NotBeNull();
        metadata.Duration!.Value.TotalSeconds.Should().BeApproximately(1.0, 0.1);
    }

    [Fact]
    public void ReadMetadata_ShouldExtractBitRateAndSampleRate()
    {
        var wavPath = CreateWavFile(sampleRate: 44100, bitsPerSample: 16);

        var metadata = _service.ReadMetadata(wavPath);

        metadata.SampleRate.Should().Be(44100);
        metadata.BitRate.Should().NotBeNull();
    }

    // ========== ExtractAlbumArt Tests ==========

    [Fact]
    public void ExtractAlbumArt_ShouldReturnBytes_WhenArtPresent()
    {
        var wavPath = CreateWavFile();

        // Write a fake album art image
        using (var tagFile = TagLib.File.Create(wavPath))
        {
            var picture = new TagLib.Picture(new TagLib.ByteVector(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }))
            {
                Type = TagLib.PictureType.FrontCover,
                MimeType = "image/jpeg"
            };
            tagFile.Tag.Pictures = new TagLib.IPicture[] { picture };
            tagFile.Save();
        }

        var art = _service.ExtractAlbumArt(wavPath);

        art.Should().NotBeNull();
        art.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void ExtractAlbumArt_ShouldReturnNull_WhenNoArt()
    {
        var wavPath = CreateWavFile();

        var art = _service.ExtractAlbumArt(wavPath);

        art.Should().BeNull();
    }

    // ========== GetDuration Tests ==========

    [Fact]
    public void GetDuration_ShouldReturnCorrectTimeSpan()
    {
        // 44100 samples at 44100 Hz = 1 second
        var wavPath = CreateWavFile(numSamples: 44100);

        var duration = _service.GetDuration(wavPath);

        duration.Should().NotBeNull();
        duration!.Value.TotalSeconds.Should().BeApproximately(1.0, 0.1);
    }

    [Fact]
    public void GetDuration_ShouldReturnLongerDuration_ForLargerFile()
    {
        // 88200 samples at 44100 Hz = 2 seconds
        var wavPath = CreateWavFile(numSamples: 88200);

        var duration = _service.GetDuration(wavPath);

        duration.Should().NotBeNull();
        duration!.Value.TotalSeconds.Should().BeApproximately(2.0, 0.1);
    }

    // ========== GetAudioProperties Tests ==========

    [Fact]
    public void GetAudioProperties_ShouldReturnBitRateAndSampleRate()
    {
        var wavPath = CreateWavFile(sampleRate: 48000, channels: 2, bitsPerSample: 16);

        var props = _service.GetAudioProperties(wavPath);

        props.SampleRate.Should().Be(48000);
        props.Channels.Should().Be(2);
        props.BitsPerSample.Should().Be(16);
        props.BitRate.Should().NotBeNull();
    }

    [Fact]
    public void GetAudioProperties_ShouldReturnDifferentValues_ForDifferentFiles()
    {
        var wavPath = CreateWavFile(sampleRate: 22050, channels: 1, bitsPerSample: 8);

        var props = _service.GetAudioProperties(wavPath);

        props.SampleRate.Should().Be(22050);
        props.Channels.Should().Be(1);
        props.BitsPerSample.Should().Be(8);
        props.BitRate.Should().NotBeNull();
    }

    // ========== Error Handling Tests ==========

    [Fact]
    public void ReadMetadata_ShouldReturnDefaults_ForCorruptFile()
    {
        var corruptPath = CreateCorruptFile();

        var metadata = _service.ReadMetadata(corruptPath);

        metadata.Should().NotBeNull();
        metadata.Title.Should().BeNull();
        metadata.Artist.Should().BeNull();
    }

    [Fact]
    public void ReadMetadata_ShouldThrow_ForMissingFile()
    {
        var act = () => _service.ReadMetadata(@"C:\nonexistent\file.mp3");

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void ReadMetadata_ShouldThrow_ForNullPath()
    {
        var act = () => _service.ReadMetadata(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExtractAlbumArt_ShouldReturnNull_ForCorruptFile()
    {
        var corruptPath = CreateCorruptFile();

        var art = _service.ExtractAlbumArt(corruptPath);

        art.Should().BeNull();
    }

    [Fact]
    public void ExtractAlbumArt_ShouldThrow_ForMissingFile()
    {
        var act = () => _service.ExtractAlbumArt(@"C:\nonexistent\file.mp3");

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void ExtractAlbumArt_ShouldThrow_ForNullPath()
    {
        var act = () => _service.ExtractAlbumArt(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetDuration_ShouldReturnNull_ForCorruptFile()
    {
        var corruptPath = CreateCorruptFile();

        var duration = _service.GetDuration(corruptPath);

        duration.Should().BeNull();
    }

    [Fact]
    public void GetDuration_ShouldThrow_ForMissingFile()
    {
        var act = () => _service.GetDuration(@"C:\nonexistent\file.mp3");

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void GetAudioProperties_ShouldReturnDefaults_ForCorruptFile()
    {
        var corruptPath = CreateCorruptFile();

        var props = _service.GetAudioProperties(corruptPath);

        props.Should().NotBeNull();
        props.BitRate.Should().BeNull();
        props.SampleRate.Should().BeNull();
    }

    [Fact]
    public void GetAudioProperties_ShouldThrow_ForMissingFile()
    {
        var act = () => _service.GetAudioProperties(@"C:\nonexistent\file.mp3");

        act.Should().Throw<FileNotFoundException>();
    }

    // ========== Integration Tests with Real Sample Files ==========

    private static readonly string _sampleMp3 = Path.Combine(
        FindRepoRoot(), "res", "sampleMusic", "Music", "Bonobo", "Migration",
        "08 - Kerala_3d71b4be-278e-461c-ab04-0698a78c11c6.mp3");

    private static readonly string _sampleWma = Path.Combine(
        FindRepoRoot(), "res", "sampleMusic", "Music", "AC-DC", "Highway to Hell",
        "01 Highway to Hell.wma");

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
                return dir.FullName;
            dir = dir.Parent;
        }
        return AppContext.BaseDirectory;
    }

    [Fact]
    public void ReadMetadata_ShouldExtractArtist_FromWma()
    {
        if (!File.Exists(_sampleWma)) return; // skip on machines without sample files

        var metadata = _service.ReadMetadata(_sampleWma);

        metadata.Artist.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ReadMetadata_ShouldExtractAlbum_FromMp3()
    {
        if (!File.Exists(_sampleMp3)) return;

        var metadata = _service.ReadMetadata(_sampleMp3);

        metadata.Album.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetDuration_ShouldReturnDuration_FromMp3()
    {
        if (!File.Exists(_sampleMp3)) return;

        var duration = _service.GetDuration(_sampleMp3);

        duration.Should().NotBeNull();
        duration!.Value.TotalSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetAudioProperties_ShouldReturnProperties_FromMp3()
    {
        if (!File.Exists(_sampleMp3)) return;

        var props = _service.GetAudioProperties(_sampleMp3);

        props.BitRate.Should().NotBeNull();
        props.SampleRate.Should().NotBeNull();
        props.Channels.Should().NotBeNull();
    }

    [Fact]
    public void ReadMetadata_ShouldExtractMultipleFields_FromWma()
    {
        if (!File.Exists(_sampleWma)) return;

        var metadata = _service.ReadMetadata(_sampleWma);

        metadata.Title.Should().NotBeNullOrWhiteSpace();
        metadata.Artist.Should().NotBeNullOrWhiteSpace();
        metadata.Duration.Should().NotBeNull();
        metadata.Duration!.Value.TotalSeconds.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ReadMetadata_ShouldExtractMultipleFields_FromMp3()
    {
        if (!File.Exists(_sampleMp3)) return;

        var metadata = _service.ReadMetadata(_sampleMp3);

        metadata.Title.Should().NotBeNullOrWhiteSpace();
        metadata.Artist.Should().NotBeNullOrWhiteSpace();
        metadata.Duration.Should().NotBeNull();
        metadata.Duration!.Value.TotalSeconds.Should().BeGreaterThan(0);
        metadata.BitRate.Should().NotBeNull();
        metadata.SampleRate.Should().NotBeNull();
    }
}
