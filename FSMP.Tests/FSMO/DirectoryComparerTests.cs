using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class DirectoryComparerTests : IDisposable
{
    private readonly string _appDir;
    private readonly string _targetDir;

    public DirectoryComparerTests()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"fsmo_comparer_test_{Guid.NewGuid():N}");
        _appDir = Path.Combine(baseDir, "app");
        _targetDir = Path.Combine(baseDir, "target");
        Directory.CreateDirectory(_appDir);
        Directory.CreateDirectory(_targetDir);
    }

    public void Dispose()
    {
        var baseDir = Path.GetDirectoryName(_appDir)!;
        if (Directory.Exists(baseDir))
            Directory.Delete(baseDir, true);
    }

    #region Helpers

    private string CreateWavFile(string directory, string fileName, string? artist = null, string? album = null, string? subDir = null)
    {
        var dir = subDir != null ? Path.Combine(directory, subDir) : directory;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

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

    #endregion

    #region FindMissingTracks

    [Fact]
    public void FindMissingTracks_ReturnsFilesInTargetNotInApp()
    {
        CreateWavFile(_appDir, "song1.wav");
        CreateWavFile(_targetDir, "song1.wav");
        CreateWavFile(_targetDir, "song2.wav");

        var missing = DirectoryComparer.FindMissingTracks(_appDir, _targetDir);

        missing.Should().ContainSingle().Which.Name.Should().Be("song2.wav");
    }

    [Fact]
    public void FindMissingTracks_ReturnsEmptyWhenAllPresent()
    {
        CreateWavFile(_appDir, "song1.wav");
        CreateWavFile(_targetDir, "song1.wav");

        var missing = DirectoryComparer.FindMissingTracks(_appDir, _targetDir);

        missing.Should().BeEmpty();
    }

    [Fact]
    public void FindMissingTracks_HandlesEmptyAppDirectory()
    {
        CreateWavFile(_targetDir, "song1.wav");
        CreateWavFile(_targetDir, "song2.wav");

        var missing = DirectoryComparer.FindMissingTracks(_appDir, _targetDir);

        missing.Should().HaveCount(2);
    }

    [Fact]
    public void FindMissingTracks_HandlesEmptyTargetDirectory()
    {
        CreateWavFile(_appDir, "song1.wav");

        var missing = DirectoryComparer.FindMissingTracks(_appDir, _targetDir);

        missing.Should().BeEmpty();
    }

    [Fact]
    public void FindMissingTracks_CaseInsensitiveComparison()
    {
        CreateWavFile(_appDir, "Song1.wav");
        CreateWavFile(_targetDir, "SONG1.wav");

        var missing = DirectoryComparer.FindMissingTracks(_appDir, _targetDir);

        missing.Should().BeEmpty();
    }

    [Fact]
    public void FindMissingTracks_FindsFilesInSubdirectories()
    {
        CreateWavFile(_appDir, "song1.wav", subDir: "Artist/Album");
        CreateWavFile(_targetDir, "song2.wav", subDir: "deep/nested");

        var missing = DirectoryComparer.FindMissingTracks(_appDir, _targetDir);

        missing.Should().ContainSingle().Which.Name.Should().Be("song2.wav");
    }

    [Fact]
    public void FindMissingTracks_ThrowsOnNullAppPath()
    {
        var act = () => DirectoryComparer.FindMissingTracks(null!, _targetDir);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FindMissingTracks_ThrowsOnNullTargetPath()
    {
        var act = () => DirectoryComparer.FindMissingTracks(_appDir, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FindMissingTracks_ThrowsOnEmptyAppPath()
    {
        var act = () => DirectoryComparer.FindMissingTracks("  ", _targetDir);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FindMissingTracks_ThrowsOnNonExistentAppPath()
    {
        var act = () => DirectoryComparer.FindMissingTracks(@"C:\nonexistent_fsmo_test", _targetDir);
        act.Should().Throw<DirectoryNotFoundException>();
    }

    [Fact]
    public void FindMissingTracks_ThrowsOnNonExistentTargetPath()
    {
        var act = () => DirectoryComparer.FindMissingTracks(_appDir, @"C:\nonexistent_fsmo_test");
        act.Should().Throw<DirectoryNotFoundException>();
    }

    #endregion

    #region CopyMissingToApp

    [Fact]
    public void CopyMissingToApp_CopiesMissingFilesToCorrectStructure()
    {
        CreateWavFile(_appDir, "song1.wav", artist: "Artist1", album: "Album1");
        CreateWavFile(_targetDir, "song2.wav", artist: "Artist2", album: "Album2");

        var result = DirectoryComparer.CopyMissingToApp(_appDir, _targetDir);

        result.FilesCopied.Should().Be(1);
        var expectedPath = Path.Combine(_appDir, "Artist2", "Album2", "song2.wav");
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public void CopyMissingToApp_SkipsFilesAlreadyInApp()
    {
        CreateWavFile(_appDir, "song1.wav");
        CreateWavFile(_targetDir, "song1.wav");

        var result = DirectoryComparer.CopyMissingToApp(_appDir, _targetDir);

        result.FilesCopied.Should().Be(0);
    }

    [Fact]
    public void CopyMissingToApp_HandlesFilesWithNoMetadata()
    {
        CreateWavFile(_targetDir, "song1.wav");

        var result = DirectoryComparer.CopyMissingToApp(_appDir, _targetDir);

        result.FilesCopied.Should().Be(1);
        var expectedPath = Path.Combine(_appDir, "Unknown Artist", "Unknown Album", "song1.wav");
        File.Exists(expectedPath).Should().BeTrue();
    }

    [Fact]
    public void CopyMissingToApp_ReturnsCorrectCounts()
    {
        CreateWavFile(_targetDir, "song1.wav", artist: "A", album: "B");
        CreateWavFile(_targetDir, "song2.wav", artist: "A", album: "B");
        CreateWavFile(_targetDir, "song3.wav", artist: "A", album: "B");

        var result = DirectoryComparer.CopyMissingToApp(_appDir, _targetDir);

        result.FilesCopied.Should().Be(3);
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void CopyMissingToApp_PreservesOriginalFiles()
    {
        var targetFile = CreateWavFile(_targetDir, "song1.wav");

        DirectoryComparer.CopyMissingToApp(_appDir, _targetDir);

        File.Exists(targetFile).Should().BeTrue();
    }

    [Fact]
    public void CopyMissingToApp_ThrowsOnNullAppPath()
    {
        var act = () => DirectoryComparer.CopyMissingToApp(null!, _targetDir);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CopyMissingToApp_ThrowsOnNullTargetPath()
    {
        var act = () => DirectoryComparer.CopyMissingToApp(_appDir, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CopyMissingToApp_ThrowsOnNonExistentAppPath()
    {
        var act = () => DirectoryComparer.CopyMissingToApp(@"C:\nonexistent_fsmo_test", _targetDir);
        act.Should().Throw<DirectoryNotFoundException>();
    }

    #endregion
}
