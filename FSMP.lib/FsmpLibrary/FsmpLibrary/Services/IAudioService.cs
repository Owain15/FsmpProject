using FsmpLibrary.Interfaces;
using FsmpLibrary.Models;

namespace FsmpLibrary.Services;

/// <summary>
/// High-level audio service for playback operations with track metadata integration.
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>Gets the underlying audio player instance.</summary>
    IAudioPlayer Player { get; }

    /// <summary>Gets the currently loaded track, if any.</summary>
    Track? CurrentTrack { get; }

    /// <summary>Gets or sets the volume level (0.0 to 1.0).</summary>
    float Volume { get; set; }

    /// <summary>Gets or sets whether audio is muted.</summary>
    bool IsMuted { get; set; }

    /// <summary>
    /// Plays a track from the database.
    /// </summary>
    /// <param name="track">The track to play.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PlayTrackAsync(Track track, CancellationToken cancellationToken = default);

    /// <summary>
    /// Plays an audio file directly by path.
    /// </summary>
    /// <param name="filePath">The path to the audio file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PlayFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses playback.
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// Resumes playback after pause.
    /// </summary>
    Task ResumeAsync();

    /// <summary>
    /// Stops playback.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Seeks to a specific position.
    /// </summary>
    /// <param name="position">The position to seek to.</param>
    Task SeekAsync(TimeSpan position);

    /// <summary>Raised when the current track changes.</summary>
    event EventHandler<TrackChangedEventArgs>? TrackChanged;
}

/// <summary>
/// Event arguments for when the current track changes.
/// </summary>
public class TrackChangedEventArgs : EventArgs
{
    /// <summary>Gets the previous track, if any.</summary>
    public Track? PreviousTrack { get; init; }

    /// <summary>Gets the new track, if any.</summary>
    public Track? NewTrack { get; init; }
}
