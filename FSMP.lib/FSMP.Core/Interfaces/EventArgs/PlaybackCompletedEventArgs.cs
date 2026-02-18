namespace FSMP.Core.Interfaces.EventArgs;

/// <summary>
/// Event arguments for when playback completes.
/// </summary>
public class PlaybackCompletedEventArgs : System.EventArgs
{
    /// <summary>Gets the file path of the completed media.</summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>Gets whether playback completed successfully (vs error/stop).</summary>
    public bool CompletedSuccessfully { get; init; }
}