namespace FSMP.Core.Audio;

/// <summary>
/// Thin adapter wrapping platform-specific media player operations.
/// Allows LibVlcAudioPlayer business logic to be unit-tested without real LibVLC.
/// </summary>
public interface IMediaPlayerAdapter : IDisposable
{
    // Playback control
    void Play();
    void Pause();
    void Stop();

    // Media management
    Task LoadMediaAsync(string filePath, CancellationToken cancellationToken = default);
    void SetMedia();
    void DisposeCurrentMedia();
    bool HasMedia { get; }

    // Position/Duration (milliseconds)
    long TimeMs { get; set; }
    long DurationMs { get; }

    // Volume (0-100 integer scale)
    int Volume { get; set; }
    bool Mute { get; set; }

    // Events
    event EventHandler? Playing;
    event EventHandler? Paused;
    event EventHandler? Stopped;
    event EventHandler? EndReached;
    event EventHandler? EncounteredError;
    event EventHandler<long>? TimeChanged;
}