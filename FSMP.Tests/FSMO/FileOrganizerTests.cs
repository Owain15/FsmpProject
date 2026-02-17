using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class FileOrganizerTests : IDisposable
{
    private readonly string _sourceDir;
    private readonly string _destDir;
    private int _fileCounter;

    public FileOrganizerTests()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"fsmo_organizer_test_{Guid.NewGuid():N}");
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

    private string CreateWavWithMetadata(string? artist = null, string? album = null, string? title = null, string? subDir = null)
    {
        var dir = subDir != null ? Path.Combine(_sourceDir, subDir) : _sourceDir;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var fileName = $"track_{_fileCounter++}.wav";
        var filePath = Path.Combine(dir, fileName);

        // Write minimal WAV
        int sampleRate = 44100;
        short channels = 1;
        short bitsPerSample = 16;
        int numSamples = 4410; // 0.1 seconds
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

        // Write metadata tags
        if (artist != null || album != null || title != null)
        {
            using var tagFile = TagLib.File.Create(filePath);
            if (artist != null) tagFile.Tag.Performers = new[] { artist };
            if (album != null) tagFile.Tag.Album = album;
            if (title != null) tagFile.Tag.Title = title;
            tagFile.Save();
        }

        return filePath;
    }

    #endregion

    #region Organize Copy Mode

    [Fact]
    public void Organize_CopyMode_CopiesFileToCorrectLocation()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration", title: "Kerala");

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        var expectedDir = Path.Combine(_destDir, "Bonobo", "Migration");
        Directory.Exists(expectedDir).Should().BeTrue();
        Directory.GetFiles(expectedDir).Should().HaveCount(1);
    }

    [Fact]
    public void Organize_CopyMode_CreatesArtistDirectory()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration");

        FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        Directory.Exists(Path.Combine(_destDir, "Bonobo")).Should().BeTrue();
    }

    [Fact]
    public void Organize_CopyMode_CreatesAlbumSubdirectory()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration");

        FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        Directory.Exists(Path.Combine(_destDir, "Bonobo", "Migration")).Should().BeTrue();
    }

    [Fact]
    public void Organize_CopyMode_PreservesOriginalFile()
    {
        var sourcePath = CreateWavWithMetadata(artist: "Bonobo", album: "Migration");

        FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        File.Exists(sourcePath).Should().BeTrue();
    }

    [Fact]
    public void Organize_CopyMode_ReturnsCorrectFilesCopiedCount()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration");

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        result.FilesCopied.Should().Be(1);
        result.FilesMoved.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Organize_CopyMode_HandlesMultipleFiles()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration", title: "Kerala");
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration", title: "Bambro");
        CreateWavWithMetadata(artist: "Kiasmos", album: "Blurred", title: "Blurred");

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        result.FilesCopied.Should().Be(3);
        Directory.GetFiles(Path.Combine(_destDir, "Bonobo", "Migration")).Should().HaveCount(2);
        Directory.GetFiles(Path.Combine(_destDir, "Kiasmos", "Blurred")).Should().HaveCount(1);
    }

    [Fact]
    public void Organize_CopyMode_HandlesFilesWithNoMetadata()
    {
        CreateWavWithMetadata(); // no metadata at all

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        result.FilesCopied.Should().Be(1);
        Directory.Exists(Path.Combine(_destDir, "Unknown Artist", "Unknown Album")).Should().BeTrue();
        Directory.GetFiles(Path.Combine(_destDir, "Unknown Artist", "Unknown Album")).Should().HaveCount(1);
    }

    #endregion

    #region Organize Move Mode

    [Fact]
    public void Organize_MoveMode_MovesFileToCorrectLocation()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration");

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Move);

        var targetDir = Path.Combine(_destDir, "Bonobo", "Migration");
        Directory.Exists(targetDir).Should().BeTrue();
        Directory.GetFiles(targetDir).Should().HaveCount(1);
    }

    [Fact]
    public void Organize_MoveMode_RemovesFileFromSource()
    {
        var sourcePath = CreateWavWithMetadata(artist: "Bonobo", album: "Migration");

        FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Move);

        File.Exists(sourcePath).Should().BeFalse();
    }

    [Fact]
    public void Organize_MoveMode_CreatesTargetDirectories()
    {
        CreateWavWithMetadata(artist: "Kiasmos", album: "Blurred");

        FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Move);

        Directory.Exists(Path.Combine(_destDir, "Kiasmos")).Should().BeTrue();
        Directory.Exists(Path.Combine(_destDir, "Kiasmos", "Blurred")).Should().BeTrue();
    }

    [Fact]
    public void Organize_MoveMode_ReturnsCorrectFilesMovedCount()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration");
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration");

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Move);

        result.FilesMoved.Should().Be(2);
        result.FilesCopied.Should().Be(0);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Organize_MoveMode_CleansUpEmptySourceDirectories()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration", subDir: "subA");

        FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Move);

        Directory.Exists(Path.Combine(_sourceDir, "subA")).Should().BeFalse();
    }

    [Fact]
    public void Organize_MoveMode_DoesNotDeleteNonEmptySourceDirectories()
    {
        CreateWavWithMetadata(artist: "Bonobo", album: "Migration", subDir: "subB");
        // Create a non-audio file that won't be moved
        File.WriteAllText(Path.Combine(_sourceDir, "subB", "readme.txt"), "keep me");

        FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Move);

        Directory.Exists(Path.Combine(_sourceDir, "subB")).Should().BeTrue();
    }

    #endregion

    #region Organize — Input Validation

    [Fact]
    public void Organize_ThrowsOnNullSourcePath()
    {
        var act = () => FileOrganizer.Organize(null!, _destDir, OrganizeMode.Copy);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Organize_ThrowsOnNullDestinationPath()
    {
        var act = () => FileOrganizer.Organize(_sourceDir, null!, OrganizeMode.Copy);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Organize_ThrowsOnNonExistentSourceDirectory()
    {
        var act = () => FileOrganizer.Organize(@"C:\nonexistent_dir_xyz", _destDir, OrganizeMode.Copy);

        act.Should().Throw<DirectoryNotFoundException>();
    }

    #endregion
}