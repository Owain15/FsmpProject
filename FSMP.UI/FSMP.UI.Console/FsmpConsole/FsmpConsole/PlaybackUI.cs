using FsmpLibrary.Models;

namespace FsmpConsole;

/// <summary>
/// Displays now-playing track information and playback controls.
/// Uses TextWriter for testability.
/// </summary>
public class PlaybackUI
{
    private readonly TextWriter _output;

    public PlaybackUI(TextWriter output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    /// <summary>
    /// Displays detailed track information for the currently playing track.
    /// </summary>
    public void DisplayNowPlaying(Track track)
    {
        ArgumentNullException.ThrowIfNull(track);

        var fields = new List<(string Label, string Value)>
        {
            ("Title:", track.DisplayTitle),
            ("Artist:", track.DisplayArtist),
            ("Album:", track.DisplayAlbum)
        };
        if (track.Duration.HasValue)
            fields.Add(("Duration:", $"{track.Duration.Value.Minutes}:{track.Duration.Value.Seconds:D2}"));
        if (track.BitRate.HasValue)
            fields.Add(("BitRate:", $"{track.BitRate}kbps"));
        fields.Add(("Plays:", track.PlayCount.ToString()));
        if (track.Rating.HasValue)
            fields.Add(("Rating:", $"{track.Rating}/5"));

        Print.WriteDetailCard(_output, "Now Playing", fields);
    }

    /// <summary>
    /// Displays available playback controls.
    /// </summary>
    public void DisplayControls()
    {
        _output.WriteLine();
        _output.WriteLine("  [S] Stop  [P] Pause  [N] Next  [F] Favorite  [E] Edit Metadata");
    }
}
