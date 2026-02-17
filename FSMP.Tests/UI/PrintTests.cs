using FSMP.Core;
using FluentAssertions;
using FsmpConsole;
using FsmpLibrary.Models;

namespace FSMP.Tests.UI;

public class PrintTests
{
    // ========== FormatTable Tests ==========

    [Fact]
    public void FormatTable_WithHeadersAndRows_ShouldCreateAlignedColumns()
    {
        var headers = new List<string> { "Name", "Count" };
        var rows = new List<string[]>
        {
            new[] { "Rock", "42" },
            new[] { "Jazz", "7" },
        };

        var result = Print.FormatTable(rows, headers);

        result.Should().Contain("Name");
        result.Should().Contain("Count");
        result.Should().Contain("Rock");
        result.Should().Contain("42");
        result.Should().Contain("Jazz");
        result.Should().Contain("7");
        // Verify separator line exists
        result.Should().Contain("----");
    }

    [Fact]
    public void FormatTable_ShouldPadColumnsToWidestEntry()
    {
        var headers = new List<string> { "Title" };
        var rows = new List<string[]>
        {
            new[] { "Short" },
            new[] { "A Much Longer Title" },
        };

        var result = Print.FormatTable(rows, headers);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // All lines should be same width (padded)
        lines[0].TrimEnd().Length.Should().Be(lines[2].TrimEnd().Length);
    }

    [Fact]
    public void FormatTable_EmptyRows_ShouldShowHeadersOnly()
    {
        var headers = new List<string> { "Name", "Value" };
        var rows = new List<string[]>();

        var result = Print.FormatTable(rows, headers);

        result.Should().Contain("Name");
        result.Should().Contain("Value");
        // Header + separator = 2 lines
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(2);
    }

    [Fact]
    public void FormatTable_EmptyHeaders_ShouldReturnEmpty()
    {
        var headers = new List<string>();
        var rows = new List<string[]>();

        var result = Print.FormatTable(rows, headers);

        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatTable_NullRows_ShouldThrow()
    {
        var act = () => Print.FormatTable(null!, new List<string> { "H" });
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FormatTable_NullHeaders_ShouldThrow()
    {
        var act = () => Print.FormatTable(new List<string[]>(), null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ========== FormatProgressBar Tests ==========

    [Fact]
    public void FormatProgressBar_HalfFull_ShouldRenderCorrectly()
    {
        var result = Print.FormatProgressBar(50, 100, 10);

        result.Should().Contain("#####");
        result.Should().Contain("50%");
    }

    [Fact]
    public void FormatProgressBar_Full_ShouldShowAllHashes()
    {
        var result = Print.FormatProgressBar(100, 100, 10);

        result.Should().Contain("##########");
        result.Should().Contain("100%");
    }

    [Fact]
    public void FormatProgressBar_Empty_ShouldShowAllSpaces()
    {
        var result = Print.FormatProgressBar(0, 100, 10);

        result.Should().Contain("[          ]");
        result.Should().Contain("0%");
    }

    [Fact]
    public void FormatProgressBar_ZeroTotal_ShouldShowEmpty()
    {
        var result = Print.FormatProgressBar(5, 0, 10);

        result.Should().Contain("0%");
    }

    [Fact]
    public void FormatProgressBar_OverMax_ShouldClampTo100()
    {
        var result = Print.FormatProgressBar(200, 100, 10);

        result.Should().Contain("100%");
        result.Should().Contain("##########");
    }

    // ========== FormatMetadataCard Tests ==========

    [Fact]
    public void FormatMetadataCard_BasicTrack_ShouldShowTitleArtistAlbum()
    {
        var track = new Track
        {
            Title = "Test Song",
            Artist = new Artist { Name = "Test Artist" },
            Album = new Album { Title = "Test Album" },
            PlayCount = 3,
        };

        var result = Print.FormatMetadataCard(track);

        result.Should().Contain("Title:    Test Song");
        result.Should().Contain("Artist:   Test Artist");
        result.Should().Contain("Album:    Test Album");
        result.Should().Contain("Plays:    3");
    }

    [Fact]
    public void FormatMetadataCard_WithDuration_ShouldShowDuration()
    {
        var track = new Track
        {
            Title = "Song",
            Duration = TimeSpan.FromMinutes(3).Add(TimeSpan.FromSeconds(45)),
        };

        var result = Print.FormatMetadataCard(track);

        result.Should().Contain("Duration: 03:45");
    }

    [Fact]
    public void FormatMetadataCard_WithBitRate_ShouldShowBitRate()
    {
        var track = new Track { Title = "Song", BitRate = 320 };

        var result = Print.FormatMetadataCard(track);

        result.Should().Contain("BitRate:  320 kbps");
    }

    [Fact]
    public void FormatMetadataCard_WithRating_ShouldShowStars()
    {
        var track = new Track { Title = "Song", Rating = 4 };

        var result = Print.FormatMetadataCard(track);

        result.Should().Contain("Rating:   ****/-");
    }

    [Fact]
    public void FormatMetadataCard_WithFavorite_ShouldShowYes()
    {
        var track = new Track { Title = "Song", IsFavorite = true };

        var result = Print.FormatMetadataCard(track);

        result.Should().Contain("Favorite: Yes");
    }

    [Fact]
    public void FormatMetadataCard_NotFavorite_ShouldOmitFavorite()
    {
        var track = new Track { Title = "Song", IsFavorite = false };

        var result = Print.FormatMetadataCard(track);

        result.Should().NotContain("Favorite:");
    }

    [Fact]
    public void FormatMetadataCard_WithCustomOverrides_ShouldUseCustom()
    {
        var track = new Track
        {
            Title = "Original",
            CustomTitle = "Custom Title",
            CustomArtist = "Custom Artist",
            CustomAlbum = "Custom Album",
        };

        var result = Print.FormatMetadataCard(track);

        result.Should().Contain("Title:    Custom Title");
        result.Should().Contain("Artist:   Custom Artist");
        result.Should().Contain("Album:    Custom Album");
    }

    [Fact]
    public void FormatMetadataCard_NullTrack_ShouldThrow()
    {
        var act = () => Print.FormatMetadataCard(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ========== NewDisplay Tests ==========

    [Fact]
    public void NewDisplay_NullOutput_ShouldThrow()
    {
        var act = () => Print.NewDisplay(null!, null, false, new List<string>(), RepeatMode.None, false);
        act.Should().Throw<ArgumentNullException>().WithParameterName("output");
    }

    [Fact]
    public void NewDisplay_NullQueueItems_ShouldThrow()
    {
        var act = () => Print.NewDisplay(new StringWriter(), null, false, null!, RepeatMode.None, false);
        act.Should().Throw<ArgumentNullException>().WithParameterName("queueItems");
    }

    [Fact]
    public void NewDisplay_WithTrack_ShouldShowTrackInfo()
    {
        var output = new StringWriter();
        var track = new Track
        {
            Title = "Kerala",
            Artist = new Artist { Name = "Bonobo" },
            Album = new Album { Title = "Migration" },
        };

        Print.NewDisplay(output, track, true, new List<string>(), RepeatMode.None, false);

        var text = output.ToString();
        text.Should().Contain("Kerala");
        text.Should().Contain("Bonobo");
        text.Should().Contain("Migration");
    }

    [Fact]
    public void NewDisplay_NullTrack_ShouldShowNone()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, false, new List<string>(), RepeatMode.None, false);

        output.ToString().Should().Contain("(none)");
    }

    [Fact]
    public void NewDisplay_IsPlaying_ShouldShowPlaying()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, true, new List<string>(), RepeatMode.None, false);

        output.ToString().Should().Contain("Playing");
    }

    [Fact]
    public void NewDisplay_NotPlaying_ShouldShowStopped()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, false, new List<string>(), RepeatMode.None, false);

        output.ToString().Should().Contain("Stopped");
    }

    [Fact]
    public void NewDisplay_RepeatModeOne_ShouldDisplay()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, false, new List<string>(), RepeatMode.One, false);

        output.ToString().Should().Contain("Repeat: One");
    }

    [Fact]
    public void NewDisplay_RepeatModeAll_ShouldDisplay()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, false, new List<string>(), RepeatMode.All, false);

        output.ToString().Should().Contain("Repeat: All");
    }

    [Fact]
    public void NewDisplay_ShuffleOn_ShouldShowOn()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, false, new List<string>(), RepeatMode.None, true);

        output.ToString().Should().Contain("Shuffle: On");
    }

    [Fact]
    public void NewDisplay_ShuffleOff_ShouldShowOff()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, false, new List<string>(), RepeatMode.None, false);

        output.ToString().Should().Contain("Shuffle: Off");
    }

    [Fact]
    public void NewDisplay_EmptyQueue_ShouldShowEmpty()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, false, new List<string>(), RepeatMode.None, false);

        output.ToString().Should().Contain("Queue: (empty)");
    }

    [Fact]
    public void NewDisplay_WithQueueItems_ShouldListThem()
    {
        var output = new StringWriter();
        var queue = new List<string> { "> 1) Kerala - Bonobo [3:20]", "  2) Cirrus - Bonobo [4:15]" };

        Print.NewDisplay(output, null, false, queue, RepeatMode.None, false);

        var text = output.ToString();
        text.Should().Contain("Queue (2 tracks):");
        text.Should().Contain("Kerala - Bonobo");
        text.Should().Contain("Cirrus - Bonobo");
    }

    [Fact]
    public void NewDisplay_ShouldShowControls()
    {
        var output = new StringWriter();

        Print.NewDisplay(output, null, false, new List<string>(), RepeatMode.None, false);

        var text = output.ToString();
        text.Should().Contain("[N] Next");
        text.Should().Contain("[S] Stop");
        text.Should().Contain("[Q] Back");
    }
}
