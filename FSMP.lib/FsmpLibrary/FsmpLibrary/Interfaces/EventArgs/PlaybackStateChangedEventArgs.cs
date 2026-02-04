namespace FsmpLibrary.Interfaces.EventArgs;

/// <summary>
/// Event arguments for playback state changes.
/// </summary>
public class PlaybackStateChangedEventArgs : System.EventArgs
{
    /// <summary>Gets the previous playback state.</summary>
    public PlaybackState OldState { get; init; }

    /// <summary>Gets the new playback state.</summary>
    public PlaybackState NewState { get; init; }
}
