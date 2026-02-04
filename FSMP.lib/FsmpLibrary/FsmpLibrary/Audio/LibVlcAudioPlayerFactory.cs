using FsmpLibrary.Interfaces;

namespace FsmpLibrary.Audio;

/// <summary>
/// Factory for creating LibVlcAudioPlayer instances.
/// </summary>
public class LibVlcAudioPlayerFactory : IAudioPlayerFactory
{
    /// <inheritdoc/>
    public IAudioPlayer CreatePlayer() => new LibVlcAudioPlayer();
}
