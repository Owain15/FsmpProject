namespace FsmpLibrary.Interfaces.EventArgs;

/// <summary>
/// Event arguments for playback position changes.
/// </summary>
public class PositionChangedEventArgs : System.EventArgs
{
    /// <summary>Gets the current playback position.</summary>
    public TimeSpan Position { get; init; }

    /// <summary>Gets the total duration of the media.</summary>
    public TimeSpan Duration { get; init; }
}
