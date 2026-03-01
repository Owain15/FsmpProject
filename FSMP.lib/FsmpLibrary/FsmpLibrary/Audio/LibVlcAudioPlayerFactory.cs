using FSMP.Core.Audio;
using FSMP.Core.Interfaces;

namespace FsmpLibrary.Audio;

/// <summary>
/// Factory for creating LibVlcAudioPlayer instances.
/// </summary>
public class LibVlcAudioPlayerFactory : IAudioPlayerFactory
{
    private readonly Func<IMediaPlayerAdapter>? _adapterFactory;
    private bool _initFailed;
    private string? _initError;

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
    public Task<bool> InitializeAsync()
    {
        if (_adapterFactory != null)
        {
            // Custom adapter — no native init needed
            return Task.FromResult(true);
        }

        try
        {
            LibVlcMediaPlayerAdapter.EnsureCoreInitialized();
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _initFailed = true;
            _initError = ex.Message;
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Returns the initialization error message, if initialization failed.
    /// </summary>
    public string? InitializationError => _initError;

    /// <inheritdoc/>
    public IAudioPlayer CreatePlayer()
    {
        if (_initFailed)
            throw new InvalidOperationException($"Audio engine not available: {_initError}");

        return _adapterFactory != null
            ? new LibVlcAudioPlayer(_adapterFactory())
            : new LibVlcAudioPlayer();
    }
}
