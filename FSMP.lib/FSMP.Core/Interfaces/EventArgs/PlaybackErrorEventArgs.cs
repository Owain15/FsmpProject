namespace FSMP.Core.Interfaces.EventArgs;

/// <summary>
/// Event arguments for playback errors.
/// </summary>
public class PlaybackErrorEventArgs : System.EventArgs
{
    /// <summary>Gets the file path that caused the error.</summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>Gets the exception that occurred, if any.</summary>
    public Exception? Exception { get; init; }

    /// <summary>Gets the error message.</summary>
    public string ErrorMessage { get; init; } = string.Empty;
}