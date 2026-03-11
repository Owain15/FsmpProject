using FSMP.Core.Audio;
using FSMP.Core.Interfaces;

namespace FSMP.Platform.Android.Audio;

/// <summary>
/// Factory for creating Android LibVLC audio players.
/// </summary>
public class LibVlcAndroidAudioPlayerFactory : IAudioPlayerFactory
{
    private readonly Func<IMediaPlayerAdapter>? _adapterFactory;
    private bool _initFailed;
    private string? _initError;

    public LibVlcAndroidAudioPlayerFactory() { }

    public LibVlcAndroidAudioPlayerFactory(Func<IMediaPlayerAdapter> adapterFactory)
    {
        _adapterFactory = adapterFactory;
    }

    public Task<bool> InitializeAsync()
    {
        if (_adapterFactory != null)
            return Task.FromResult(true);

        try
        {
            LibVlcAndroidMediaPlayerAdapter.EnsureCoreInitialized();
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _initFailed = true;
            _initError = ex.Message;
            return Task.FromResult(false);
        }
    }

    public string? InitializationError => _initError;

    public IAudioPlayer CreatePlayer()
    {
        if (_initFailed)
            throw new InvalidOperationException($"Audio engine not available: {_initError}");

        return _adapterFactory != null
            ? new LibVlcAndroidAudioPlayer(_adapterFactory())
            : new LibVlcAndroidAudioPlayer();
    }
}
