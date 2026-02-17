using FluentAssertions;
using FSMO;

namespace FSMP.Tests.FSMO;

public class AudioMetadataTests
{
    #region Default Initialization

    [Fact]
    public void DefaultInitialization_AllPropertiesAreNull()
    {
        var metadata = new AudioMetadata();

        metadata.Title.Should().BeNull();
        metadata.Artist.Should().BeNull();
        metadata.Album.Should().BeNull();
        metadata.TrackNumber.Should().BeNull();
        metadata.Year.Should().BeNull();
        metadata.Duration.Should().BeNull();
    }

    #endregion

    #region Property Setters

    [Fact]
    public void Title_SetAndGet_ReturnsValue()
    {
        var metadata = new AudioMetadata { Title = "Kerala" };

        metadata.Title.Should().Be("Kerala");
    }

    [Fact]
    public void Artist_SetAndGet_ReturnsValue()
    {
        var metadata = new AudioMetadata { Artist = "Bonobo" };

        metadata.Artist.Should().Be("Bonobo");
    }

    [Fact]
    public void Album_SetAndGet_ReturnsValue()
    {
        var metadata = new AudioMetadata { Album = "Migration" };

        metadata.Album.Should().Be("Migration");
    }

    [Fact]
    public void TrackNumber_SetAndGet_ReturnsValue()
    {
        var metadata = new AudioMetadata { TrackNumber = 8 };

        metadata.TrackNumber.Should().Be(8);
    }

    [Fact]
    public void Year_SetAndGet_ReturnsValue()
    {
        var metadata = new AudioMetadata { Year = 2017 };

        metadata.Year.Should().Be(2017);
    }

    [Fact]
    public void Duration_SetAndGet_ReturnsValue()
    {
        var duration = TimeSpan.FromMinutes(3.5);
        var metadata = new AudioMetadata { Duration = duration };

        metadata.Duration.Should().Be(duration);
    }

    #endregion
}