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

        _output.WriteLine();
        _output.WriteLine("== Now Playing ==");
        _output.WriteLine($"  Title:  {track.DisplayTitle}");
        _output.WriteLine($"  Artist: {track.DisplayArtist}");
        _output.WriteLine($"  Album:  {track.DisplayAlbum}");

        if (track.Duration.HasValue)
            _output.WriteLine($"  Duration: {track.Duration.Value.Minutes}:{track.Duration.Value.Seconds:D2}");

        if (track.BitRate.HasValue)
            _output.WriteLine($"  BitRate:  {track.BitRate}kbps");

        _output.WriteLine($"  Plays:  {track.PlayCount}");

        if (track.Rating.HasValue)
            _output.WriteLine($"  Rating: {track.Rating}/5");
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
