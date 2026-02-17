using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class MetadataReaderTests : IDisposable
{
    private readonly string _tempDir;
    private int _fileCounter;

    public MetadataReaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"fsmo_metadata_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    #region Helpers

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

    private string CreateWavFile()
    {
        var filePath = Path.Combine(_tempDir, $"test_{_fileCounter++}.wav");
        int sampleRate = 44100;
        short channels = 1;
        short bitsPerSample = 16;
        int numSamples = 44100;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        short blockAlign = (short)(channels * bitsPerSample / 8);
        int dataSize = numSamples * blockAlign;

        using var stream = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(stream);

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
        writer.Write(new byte[dataSize]);

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

    #endregion

    #region ReadMetadata — Real Sample Files

    [Fact]
    public void ReadMetadata_ExtractsArtist_FromMp3()
    {
        if (!File.Exists(_sampleMp3)) return;

        var metadata = MetadataReader.ReadMetadata(_sampleMp3);

        metadata.Artist.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ReadMetadata_ExtractsAlbum_FromMp3()
    {
        if (!File.Exists(_sampleMp3)) return;

        var metadata = MetadataReader.ReadMetadata(_sampleMp3);

        metadata.Album.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ReadMetadata_ExtractsTitle_FromMp3()
    {
        if (!File.Exists(_sampleMp3)) return;

        var metadata = MetadataReader.ReadMetadata(_sampleMp3);

        metadata.Title.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ReadMetadata_ExtractsArtist_FromWma()
    {
        if (!File.Exists(_sampleWma)) return;

        var metadata = MetadataReader.ReadMetadata(_sampleWma);

        metadata.Artist.Should().NotBeNullOrWhiteSpace();
    }

    #endregion

    #region ReadMetadata — Programmatic Files

    [Fact]
    public void ReadMetadata_ReturnsNulls_ForFileWithNoTags()
    {
        var wavPath = CreateWavFile();

        var metadata = MetadataReader.ReadMetadata(wavPath);

        metadata.Title.Should().BeNull();
        metadata.Artist.Should().BeNull();
        metadata.Album.Should().BeNull();
        metadata.TrackNumber.Should().BeNull();
        metadata.Year.Should().BeNull();
    }

    [Fact]
    public void ReadMetadata_HandlesCorruptFile_ReturnsEmptyMetadata()
    {
        var corruptPath = CreateCorruptFile();

        var metadata = MetadataReader.ReadMetadata(corruptPath);

        metadata.Should().NotBeNull();
        metadata.Title.Should().BeNull();
        metadata.Artist.Should().BeNull();
        metadata.Album.Should().BeNull();
    }

    #endregion

    #region ReadMetadata — Input Validation

    [Fact]
    public void ReadMetadata_ThrowsArgumentNullException_ForNullPath()
    {
        var act = () => MetadataReader.ReadMetadata(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ReadMetadata_ThrowsFileNotFoundException_ForMissingFile()
    {
        var act = () => MetadataReader.ReadMetadata(@"C:\nonexistent\file.mp3");

        act.Should().Throw<FileNotFoundException>();
    }

    #endregion
}