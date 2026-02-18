using FSMP.Core.Interfaces.EventArgs;

namespace FSMP.Core.Interfaces;

/// <summary>
/// Platform-agnostic audio playback interface supporting play, pause, stop, seek, and volume control.
/// </summary>
public interface IAudioPlayer : IDisposable
{
    /// <summary>Gets the current playback state.</summary>
    PlaybackState State { get; }

    /// <summary>Gets the current playback position.</summary>
    TimeSpan Position { get; }

    /// <summary>Gets the total duration of the loaded media.</summary>
    TimeSpan Duration { get; }

    /// <summary>Gets or sets the volume level (0.0 to 1.0).</summary>
    float Volume { get; set; }

    /// <summary>Gets or sets whether audio is muted.</summary>
    bool IsMuted { get; set; }

    /// <summary>
    /// Loads a media file for playback.
    /// </summary>
    /// <param name="filePath">The path to the audio file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts or resumes playback.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PlayAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses playback.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task PauseAsync();

    /// <summary>
    /// Stops playback.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopAsync();

    /// <summary>
    /// Seeks to a specific position in the media.
    /// </summary>
    /// <param name="position">The position to seek to.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeekAsync(TimeSpan position);

    /// <summary>Raised when the playback state changes.</summary>
    event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;

    /// <summary>Raised when playback completes (end of media or stopped).</summary>
    event EventHandler<PlaybackCompletedEventArgs>? PlaybackCompleted;

    /// <summary>Raised when a playback error occurs.</summary>
    event EventHandler<PlaybackErrorEventArgs>? PlaybackError;

    /// <summary>Raised when the playback position changes.</summary>
    event EventHandler<PositionChangedEventArgs>? PositionChanged;
}