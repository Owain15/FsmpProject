using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class DirectoryManagerTests : IDisposable
{
    private readonly string _sourceDir;
    private readonly string _destDir;
    private int _fileCounter;

    public DirectoryManagerTests()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"fsmo_dirmgr_test_{Guid.NewGuid():N}");
        _sourceDir = Path.Combine(baseDir, "source");
        _destDir = Path.Combine(baseDir, "dest");
        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_destDir);
    }

    public void Dispose()
    {
        var baseDir = Path.GetDirectoryName(_sourceDir)!;
        if (Directory.Exists(baseDir))
            Directory.Delete(baseDir, true);
    }

    #region Helpers

    private string CreateWavWithMetadata(string? artist = null, string? album = null, string? subDir = null)
    {
        var dir = subDir != null ? Path.Combine(_sourceDir, subDir) : _sourceDir;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var fileName = $"track_{_fileCounter++}.wav";
        var filePath = Path.Combine(dir, fileName);

        int sampleRate = 44100;
        short channels = 1;
        short bitsPerSample = 16;
        int numSamples = 4410;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        short blockAlign = (short)(channels * bitsPerSample / 8);
        int dataSize = numSamples * blockAlign;

        using (var stream = new FileStream(filePath, FileMode.Create))
        using (var writer = new BinaryWriter(stream))
        {
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
        }

        if (artist != null || album != null)
        {
            using var tagFile = TagLib.File.Create(filePath);
            if (artist != null) tagFile.Tag.Performers = new[] { artist };
            if (album != null) tagFile.Tag.Album = album;
            tagFile.Save();
        }

        return filePath;
    }

    private void CreateNonAudioFile(string fileName, string? subDir = null)
    {
        var dir = subDir != null ? Path.Combine(_sourceDir, subDir) : _sourceDir;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, fileName), "not audio");
    }

    #endregion

    #region ReorganiseDirectory

    [Fact]
    public void ReorganiseDirectory_OrganizesFilesIntoCorrectStructure()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration");

        var result = DirectoryManager.ReorganiseDirectory(new DirectoryInfo(_sourceDir), _destDir);

        result.FilesCopied.Should().Be(1);
        Directory.Exists(Path.Combine(_destDir, "Bonobo", "Migration")).Should().BeTrue();
    }

    [Fact]
    public void ReorganiseDirectory_WithMixedFormats()
    {
        // WAV with metadata
        CreateWavWithMetadata(artist: "Artist1", album: "Album1");
        CreateWavWithMetadata(artist: "Artist2", album: "Album2");

        var result = DirectoryManager.ReorganiseDirectory(new DirectoryInfo(_sourceDir), _destDir);

        result.FilesCopied.Should().Be(2);
        Directory.Exists(Path.Combine(_destDir, "Artist1", "Album1")).Should().BeTrue();
        Directory.Exists(Path.Combine(_destDir, "Artist2", "Album2")).Should().BeTrue();
    }

    [Fact]
    public void ReorganiseDirectory_ThrowsOnNullSourceDir()
    {
        var act = () => DirectoryManager.ReorganiseDirectory(null!, _destDir);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ReorganiseDirectory_ThrowsOnNullDestinationPath()
    {
        var act = () => DirectoryManager.ReorganiseDirectory(new DirectoryInfo(_sourceDir), null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetAllDistinctAudioFiles

    [Fact]
    public void GetAllDistinctAudioFiles_ReturnsUniqueFiles()
    {
        // Create files with same name in different subdirectories
        CreateWavWithMetadata(artist: "A", subDir: "sub1");
        // Create another file with a different name
        CreateWavWithMetadata(artist: "B");

        var files = DirectoryManager.GetAllDistinctAudioFiles(_sourceDir);

        files.Should().HaveCount(2);
    }

    [Fact]
    public void GetAllDistinctAudioFiles_ExcludesUnsupportedFormats()
    {
        CreateWavWithMetadata(artist: "Bonobo");
        CreateNonAudioFile("readme.txt");
        CreateNonAudioFile("cover.jpg");

        var files = DirectoryManager.GetAllDistinctAudioFiles(_sourceDir);

        files.Should().HaveCount(1);
        files.Should().OnlyContain(f => f.Extension.Equals(".wav", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetAllDistinctAudioFiles_DeduplicatesByFileName()
    {
        // Create two files with the same name in different subdirectories
        var sub1 = Path.Combine(_sourceDir, "sub1");
        var sub2 = Path.Combine(_sourceDir, "sub2");
        Directory.CreateDirectory(sub1);
        Directory.CreateDirectory(sub2);

        // Create identical file names
        var filePath1 = Path.Combine(sub1, "same.wav");
        var filePath2 = Path.Combine(sub2, "same.wav");

        // Write minimal WAV to both
        foreach (var fp in new[] { filePath1, filePath2 })
        {
            using var stream = new FileStream(fp, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            writer.Write("RIFF"u8);
            writer.Write(44);
            writer.Write("WAVE"u8);
            writer.Write("fmt "u8);
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(44100);
            writer.Write(88200);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write("data"u8);
            writer.Write(8);
            writer.Write(new byte[8]);
        }

        var files = DirectoryManager.GetAllDistinctAudioFiles(_sourceDir);

        files.Should().HaveCount(1);
    }

    [Fact]
    public void GetAllDistinctAudioFiles_ThrowsOnNullPath()
    {
        var act = () => DirectoryManager.GetAllDistinctAudioFiles(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}