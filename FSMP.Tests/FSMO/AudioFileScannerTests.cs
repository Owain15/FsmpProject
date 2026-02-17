using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class AudioFileScannerTests : IDisposable
{
    private readonly string _tempDir;

    public AudioFileScannerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"fsmo_scanner_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    #region ScanDirectory

    [Fact]
    public void ScanDirectory_ShouldFindMp3Files()
    {
        File.WriteAllText(Path.Combine(_tempDir, "song.mp3"), "fake");

        var results = AudioFileScanner.ScanDirectory(_tempDir);

        results.Should().ContainSingle()
            .Which.Name.Should().Be("song.mp3");
    }

    [Fact]
    public void ScanDirectory_ShouldFindWavFiles()
    {
        File.WriteAllText(Path.Combine(_tempDir, "song.wav"), "fake");

        var results = AudioFileScanner.ScanDirectory(_tempDir);

        results.Should().ContainSingle()
            .Which.Name.Should().Be("song.wav");
    }

    [Fact]
    public void ScanDirectory_ShouldFindWmaFiles()
    {
        File.WriteAllText(Path.Combine(_tempDir, "song.wma"), "fake");

        var results = AudioFileScanner.ScanDirectory(_tempDir);

        results.Should().ContainSingle()
            .Which.Name.Should().Be("song.wma");
    }

    [Fact]
    public void ScanDirectory_ShouldIgnoreUnsupportedFormats()
    {
        File.WriteAllText(Path.Combine(_tempDir, "readme.txt"), "text");
        File.WriteAllText(Path.Combine(_tempDir, "cover.jpg"), "image");
        File.WriteAllText(Path.Combine(_tempDir, "song.flac"), "audio");

        var results = AudioFileScanner.ScanDirectory(_tempDir);

        results.Should().BeEmpty();
    }

    [Fact]
    public void ScanDirectory_ShouldSearchSubdirectoriesRecursively()
    {
        var subDir = Path.Combine(_tempDir, "Artist", "Album");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "track.mp3"), "fake");
        File.WriteAllText(Path.Combine(_tempDir, "root.wav"), "fake");

        var results = AudioFileScanner.ScanDirectory(_tempDir);

        results.Should().HaveCount(2);
        results.Select(f => f.Name).Should().Contain("track.mp3").And.Contain("root.wav");
    }

    [Fact]
    public void ScanDirectory_ShouldReturnEmptyForEmptyDirectory()
    {
        var results = AudioFileScanner.ScanDirectory(_tempDir);

        results.Should().BeEmpty();
    }

    [Fact]
    public void ScanDirectory_ShouldThrowOnNullPath()
    {
        var act = () => AudioFileScanner.ScanDirectory(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ScanDirectory_ShouldThrowOnEmptyPath()
    {
        var act = () => AudioFileScanner.ScanDirectory("");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ScanDirectory_ShouldThrowOnNonExistentDirectory()
    {
        var act = () => AudioFileScanner.ScanDirectory(Path.Combine(_tempDir, "nonexistent"));

        act.Should().Throw<DirectoryNotFoundException>();
    }

    #endregion

    #region IsSupportedFormat

    [Theory]
    [InlineData(".mp3")]
    [InlineData(".MP3")]
    [InlineData(".Mp3")]
    [InlineData(".wav")]
    [InlineData(".WAV")]
    [InlineData(".wma")]
    [InlineData(".WMA")]
    public void IsSupportedFormat_ShouldReturnTrue_ForSupportedExtensions(string extension)
    {
        AudioFileScanner.IsSupportedFormat(extension).Should().BeTrue();
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".jpg")]
    [InlineData(".flac")]
    [InlineData(".ogg")]
    [InlineData("")]
    public void IsSupportedFormat_ShouldReturnFalse_ForUnsupportedExtensions(string extension)
    {
        AudioFileScanner.IsSupportedFormat(extension).Should().BeFalse();
    }

    #endregion
}
