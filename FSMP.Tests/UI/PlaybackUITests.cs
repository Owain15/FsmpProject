using FluentAssertions;
using FsmpConsole;
using FSMP.Core.Models;

namespace FSMP.Tests.UI;

public class PlaybackUITests
{
    // ========== Constructor Tests ==========

    [Fact]
    public void Constructor_WithNullOutput_ShouldThrow()
    {
        var act = () => new PlaybackUI(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    // ========== DisplayNowPlaying Tests ==========

    [Fact]
    public void DisplayNowPlaying_WithNullTrack_ShouldThrow()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);

        var act = () => ui.DisplayNowPlaying(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("track");
    }

    [Fact]
    public void DisplayNowPlaying_ShouldShowTitle()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track { Title = "Kerala", FilePath = @"C:\test.mp3", FileHash = "abc" };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().Contain("Kerala");
    }

    [Fact]
    public void DisplayNowPlaying_ShouldShowArtistAndAlbum()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var artist = new Artist { Name = "Bonobo" };
        var album = new Album { Title = "Migration" };
        var track = new Track
        {
            Title = "Kerala",
            FilePath = @"C:\test.mp3",
            FileHash = "abc",
            Artist = artist,
            Album = album,
        };

        ui.DisplayNowPlaying(track);

        var text = output.ToString();
        text.Should().Contain("Bonobo");
        text.Should().Contain("Migration");
    }

    [Fact]
    public void DisplayNowPlaying_WithDuration_ShouldShowDuration()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track
        {
            Title = "Kerala",
            FilePath = @"C:\test.mp3",
            FileHash = "abc",
            Duration = TimeSpan.FromSeconds(195),
        };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().Contain("3:15");
    }

    [Fact]
    public void DisplayNowPlaying_WithoutDuration_ShouldOmitDuration()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track { Title = "Kerala", FilePath = @"C:\test.mp3", FileHash = "abc" };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().NotContain("Duration:");
    }

    [Fact]
    public void DisplayNowPlaying_WithBitRate_ShouldShowBitRate()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track
        {
            Title = "Kerala",
            FilePath = @"C:\test.mp3",
            FileHash = "abc",
            BitRate = 320,
        };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().Contain("320kbps");
    }

    [Fact]
    public void DisplayNowPlaying_WithoutBitRate_ShouldOmitBitRate()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track { Title = "Kerala", FilePath = @"C:\test.mp3", FileHash = "abc" };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().NotContain("BitRate:");
    }

    [Fact]
    public void DisplayNowPlaying_ShouldShowPlayCount()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track
        {
            Title = "Kerala",
            FilePath = @"C:\test.mp3",
            FileHash = "abc",
            PlayCount = 42,
        };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().Contain("Plays:  42");
    }

    [Fact]
    public void DisplayNowPlaying_WithRating_ShouldShowRating()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track
        {
            Title = "Kerala",
            FilePath = @"C:\test.mp3",
            FileHash = "abc",
            Rating = 4,
        };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().Contain("Rating: 4/5");
    }

    [Fact]
    public void DisplayNowPlaying_WithoutRating_ShouldOmitRating()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track { Title = "Kerala", FilePath = @"C:\test.mp3", FileHash = "abc" };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().NotContain("Rating:");
    }

    [Fact]
    public void DisplayNowPlaying_WithCustomTitle_ShouldShowCustomTitle()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track
        {
            Title = "Original Title",
            CustomTitle = "Custom Title",
            FilePath = @"C:\test.mp3",
            FileHash = "abc",
        };

        ui.DisplayNowPlaying(track);

        var text = output.ToString();
        text.Should().Contain("Custom Title");
        text.Should().NotContain("Original Title");
    }

    [Fact]
    public void DisplayNowPlaying_ShouldContainNowPlayingHeader()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);
        var track = new Track { Title = "Kerala", FilePath = @"C:\test.mp3", FileHash = "abc" };

        ui.DisplayNowPlaying(track);

        output.ToString().Should().Contain("Now Playing");
    }

    // ========== DisplayControls Tests ==========

    [Fact]
    public void DisplayControls_ShouldShowStopOption()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);

        ui.DisplayControls();

        output.ToString().Should().Contain("Stop");
    }

    [Fact]
    public void DisplayControls_ShouldShowPauseOption()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);

        ui.DisplayControls();

        output.ToString().Should().Contain("Pause");
    }

    [Fact]
    public void DisplayControls_ShouldShowNextOption()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);

        ui.DisplayControls();

        output.ToString().Should().Contain("Next");
    }

    [Fact]
    public void DisplayControls_ShouldShowFavoriteOption()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);

        ui.DisplayControls();

        output.ToString().Should().Contain("Favorite");
    }

    [Fact]
    public void DisplayControls_ShouldShowEditMetadataOption()
    {
        var output = new StringWriter();
        var ui = new PlaybackUI(output);

        ui.DisplayControls();

        output.ToString().Should().Contain("Edit Metadata");
    }
}
