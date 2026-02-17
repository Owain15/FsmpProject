using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class PathBuilderTests
{
    #region BuildTargetPath — Correct Path

    [Fact]
    public void BuildTargetPath_WithFullMetadata_ReturnsCorrectPath()
    {
        var metadata = new AudioMetadata { Artist = "Bonobo", Album = "Migration" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "08 - Kerala.mp3");

        result.Should().Be(Path.Combine(@"C:\Music", "Bonobo", "Migration", "08 - Kerala.mp3"));
    }

    [Fact]
    public void BuildTargetPath_PreservesOriginalFileName()
    {
        var metadata = new AudioMetadata { Artist = "AC-DC", Album = "Highway to Hell" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "01 Highway to Hell.wma");

        Path.GetFileName(result).Should().Be("01 Highway to Hell.wma");
    }

    #endregion

    #region BuildTargetPath — Unknown Artist Fallback

    [Fact]
    public void BuildTargetPath_NullArtist_FallsBackToUnknownArtist()
    {
        var metadata = new AudioMetadata { Artist = null, Album = "SomeAlbum" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().Contain("Unknown Artist");
    }

    [Fact]
    public void BuildTargetPath_EmptyArtist_FallsBackToUnknownArtist()
    {
        var metadata = new AudioMetadata { Artist = "", Album = "SomeAlbum" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().Contain("Unknown Artist");
    }

    [Fact]
    public void BuildTargetPath_WhitespaceArtist_FallsBackToUnknownArtist()
    {
        var metadata = new AudioMetadata { Artist = "   ", Album = "SomeAlbum" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().Contain("Unknown Artist");
    }

    #endregion

    #region BuildTargetPath — Unknown Album Fallback

    [Fact]
    public void BuildTargetPath_NullAlbum_FallsBackToUnknownAlbum()
    {
        var metadata = new AudioMetadata { Artist = "SomeArtist", Album = null };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().Contain("Unknown Album");
    }

    [Fact]
    public void BuildTargetPath_EmptyAlbum_FallsBackToUnknownAlbum()
    {
        var metadata = new AudioMetadata { Artist = "SomeArtist", Album = "" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().Contain("Unknown Album");
    }

    [Fact]
    public void BuildTargetPath_WhitespaceAlbum_FallsBackToUnknownAlbum()
    {
        var metadata = new AudioMetadata { Artist = "SomeArtist", Album = "   " };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().Contain("Unknown Album");
    }

    #endregion

    #region BuildTargetPath — Sanitization

    [Fact]
    public void BuildTargetPath_SanitizesInvalidCharsFromArtist()
    {
        var metadata = new AudioMetadata { Artist = "AC/DC", Album = "Highway to Hell" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().NotContain("AC/DC");
        result.Should().Contain("ACDC");
    }

    [Fact]
    public void BuildTargetPath_SanitizesInvalidCharsFromAlbum()
    {
        var metadata = new AudioMetadata { Artist = "Test", Album = "What?!" };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().NotContain("?");
    }

    [Fact]
    public void BuildTargetPath_TrimsWhitespaceFromArtistAndAlbum()
    {
        var metadata = new AudioMetadata { Artist = "  Bonobo  ", Album = "  Migration  " };

        var result = PathBuilder.BuildTargetPath(@"C:\Music", metadata, "track.mp3");

        result.Should().Be(Path.Combine(@"C:\Music", "Bonobo", "Migration", "track.mp3"));
    }

    #endregion

    #region BuildTargetPath — Input Validation

    [Fact]
    public void BuildTargetPath_ThrowsOnNullDestinationRoot()
    {
        var metadata = new AudioMetadata { Artist = "Test", Album = "Test" };

        var act = () => PathBuilder.BuildTargetPath(null!, metadata, "track.mp3");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BuildTargetPath_ThrowsOnNullOriginalFileName()
    {
        var metadata = new AudioMetadata { Artist = "Test", Album = "Test" };

        var act = () => PathBuilder.BuildTargetPath(@"C:\Music", metadata, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion
}