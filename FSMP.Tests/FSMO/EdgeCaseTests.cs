using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class EdgeCaseTests : IDisposable
{
    private readonly string _sourceDir;
    private readonly string _destDir;
    private int _fileCounter;

    public EdgeCaseTests()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"fsmo_edge_test_{Guid.NewGuid():N}");
        _sourceDir = Path.Combine(baseDir, "source");
        _destDir = Path.Combine(baseDir, "dest");
        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_destDir);
    }

    public void Dispose()
    {
        var baseDir = Path.GetDirectoryName(_sourceDir)!;
        if (Directory.Exists(baseDir))
        {
            // Clear read-only attributes before deleting (needed for read-only file tests)
            foreach (var file in Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories))
                File.SetAttributes(file, FileAttributes.Normal);
            Directory.Delete(baseDir, true);
        }
    }

    #region Helpers

    private string CreateWavFile(string? artist = null, string? album = null, string? fileName = null)
    {
        fileName ??= $"track_{_fileCounter++}.wav";
        var filePath = Path.Combine(_sourceDir, fileName);

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

    private string CreateCorruptMp3()
    {
        var fileName = $"corrupt_{_fileCounter++}.mp3";
        var filePath = Path.Combine(_sourceDir, fileName);
        var random = new Random(42);
        var bytes = new byte[256];
        random.NextBytes(bytes);
        File.WriteAllBytes(filePath, bytes);
        return filePath;
    }

    #endregion

    #region PathBuilder — Long Path Truncation

    [Fact]
    public void PathBuilder_TruncatesLongArtistName()
    {
        var longArtist = new string('A', 300);
        var metadata = new AudioMetadata { Artist = longArtist, Album = "Album" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        var artistDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(result)!)!);
        (artistDir.Length <= 200).Should().BeTrue($"artist folder name should be at most 200 chars but was {artistDir.Length}");
    }

    [Fact]
    public void PathBuilder_TruncatesLongAlbumName()
    {
        var longAlbum = new string('B', 300);
        var metadata = new AudioMetadata { Artist = "Artist", Album = longAlbum };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        var albumDir = Path.GetFileName(Path.GetDirectoryName(result)!);
        (albumDir.Length <= 200).Should().BeTrue($"album folder name should be at most 200 chars but was {albumDir.Length}");
    }

    [Fact]
    public void PathBuilder_TruncatedNameDoesNotEndWithWhitespace()
    {
        // Create a name that will have whitespace at position 200 after truncation
        var name = new string('A', 199) + " " + new string('B', 100);
        var metadata = new AudioMetadata { Artist = name, Album = "Album" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        var artistDir = Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(result)!)!);
        artistDir.Should().NotEndWith(" ");
    }

    [Fact]
    public void PathBuilder_ArtistOfAllInvalidChars_FallsBackToUnknownArtist()
    {
        // A name consisting entirely of invalid filename chars should fall back
        var invalidChars = new string(Path.GetInvalidFileNameChars());
        var metadata = new AudioMetadata { Artist = invalidChars, Album = "Album" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().Contain("Unknown Artist");
    }

    #endregion

    #region FileOrganizer — Read-Only Source Files

    [Fact]
    public void Organize_CopyMode_HandlesReadOnlySourceFile()
    {
        var filePath = CreateWavFile(artist: "TestArtist", album: "TestAlbum");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        try
        {
            var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

            result.FilesCopied.Should().Be(1);
            result.Errors.Should().BeEmpty();
            var targetDir = Path.Combine(_destDir, "TestArtist", "TestAlbum");
            Directory.GetFiles(targetDir).Should().HaveCount(1);
        }
        finally
        {
            // Remove read-only so Dispose can clean up
            if (File.Exists(filePath))
                File.SetAttributes(filePath, FileAttributes.Normal);
        }
    }

    #endregion

    #region FileOrganizer — Corrupt Files

    [Fact]
    public void Organize_CorruptAudioFile_IsHandledGracefully()
    {
        CreateCorruptMp3();

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        // Corrupt file should still be processed — MetadataReader returns empty AudioMetadata
        // and the file gets organized under Unknown Artist/Unknown Album.
        // If the corrupt file causes an exception during copy, it goes to Errors.
        // Either way, the organizer should not throw.
        (result.FilesCopied + result.Errors.Count).Should().BeGreaterThan(0);
    }

    [Fact]
    public void Organize_MixedValidAndCorruptFiles_ValidFilesStillOrganized()
    {
        CreateWavFile(artist: "ValidArtist", album: "ValidAlbum");
        CreateCorruptMp3();

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        // The valid WAV file should be copied successfully
        var validDir = Path.Combine(_destDir, "ValidArtist", "ValidAlbum");
        Directory.Exists(validDir).Should().BeTrue();
        Directory.GetFiles(validDir).Should().HaveCount(1);
    }

    #endregion

    #region FileOrganizer — Empty Artist AND Album

    [Fact]
    public void Organize_EmptyArtistAndAlbum_GoesToUnknownArtistUnknownAlbum()
    {
        CreateWavFile(); // No metadata at all

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        result.FilesCopied.Should().Be(1);
        var targetDir = Path.Combine(_destDir, "Unknown Artist", "Unknown Album");
        Directory.Exists(targetDir).Should().BeTrue();
        Directory.GetFiles(targetDir).Should().HaveCount(1);
    }

    #endregion

    #region FileOrganizer — Special Characters in Metadata

    [Fact]
    public void Organize_SpecialCharsInArtist_SanitizedAndOrganized()
    {
        // AC/DC has a forward slash which is invalid in folder names
        CreateWavFile(artist: "AC/DC", album: "Highway to Hell");

        var result = FileOrganizer.Organize(_sourceDir, _destDir, OrganizeMode.Copy);

        result.FilesCopied.Should().Be(1);
        // The slash should be stripped, resulting in "ACDC"
        var targetDir = Path.Combine(_destDir, "ACDC", "Highway to Hell");
        Directory.Exists(targetDir).Should().BeTrue();
    }

    #endregion
}