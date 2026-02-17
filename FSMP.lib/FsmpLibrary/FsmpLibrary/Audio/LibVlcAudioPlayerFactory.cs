using FsmpLibrary.Interfaces;

namespace FsmpLibrary.Audio;

/// <summary>
/// Factory for creating LibVlcAudioPlayer instances.
/// </summary>
public class LibVlcAudioPlayerFactory : IAudioPlayerFactory
{
    private readonly Func<IMediaPlayerAdapter>? _adapterFactory;

    /// <summary>
    /// Creates a factory that uses the default LibVLC adapter.
    /// </summary>
    public LibVlcAudioPlayerFactory() { }

    /// <summary>
    /// Creates a factory with a custom adapter factory (enables unit testing).
    /// </summary>
    public LibVlcAudioPlayerFactory(Func<IMediaPlayerAdapter> adapterFactory)
    {
        _adapterFactory = adapterFactory;
    }

    /// <inheritdoc/>
    public IAudioPlayer CreatePlayer() => _adapterFactory != null
        ? new LibVlcAudioPlayer(_adapterFactory())
        : new LibVlcAudioPlayer();
}
