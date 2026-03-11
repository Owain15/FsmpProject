using FSMP.Core.Audio;
using LibVLCSharp.Shared;

namespace FSMP.Platform.Android.Audio;

/// <summary>
/// LibVLC media player adapter for Android.
/// Nearly identical to the Windows adapter — only EnsureCoreInitialized() differs
/// because Android bundles the native libraries automatically.
/// </summary>
public class LibVlcAndroidMediaPlayerAdapter : IMediaPlayerAdapter
{
    private static bool _coreInitialized;
    private static readonly object _initLock = new();

    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private Media? _currentMedia;
    private bool _disposed;

    /// <summary>
    /// Eagerly initializes the LibVLC core for Android.
    /// Android bundles native libs automatically — no path needed.
    /// </summary>
    public static void EnsureCoreInitialized()
    {
        lock (_initLock)
        {
            if (!_coreInitialized)
            {
                LibVLCSharp.Shared.Core.Initialize();
                _coreInitialized = true;
            }
        }
    }

    public LibVlcAndroidMediaPlayerAdapter() : this("--quiet") { }

    public LibVlcAndroidMediaPlayerAdapter(params string[] vlcOptions)
    {
        EnsureCoreInitialized();

        _libVLC = new LibVLC(vlcOptions);
        _mediaPlayer = new MediaPlayer(_libVLC);

        _mediaPlayer.Playing += (s, e) => Playing?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.Paused += (s, e) => Paused?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.Stopped += (s, e) => Stopped?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.EndReached += (s, e) => EndReached?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.EncounteredError += (s, e) => EncounteredError?.Invoke(this, EventArgs.Empty);
        _mediaPlayer.TimeChanged += (s, e) => TimeChanged?.Invoke(this, e.Time);
    }

    public bool HasMedia => _currentMedia != null;
    public long TimeMs { get => _mediaPlayer.Time; set => _mediaPlayer.Time = value; }
    public long DurationMs => _currentMedia?.Duration ?? 0;
    public int Volume { get => _mediaPlayer.Volume; set => _mediaPlayer.Volume = value; }
    public bool Mute { get => _mediaPlayer.Mute; set => _mediaPlayer.Mute = value; }

    public event EventHandler? Playing;
    public event EventHandler? Paused;
    public event EventHandler? Stopped;
    public event EventHandler? EndReached;
    public event EventHandler? EncounteredError;
    public event EventHandler<long>? TimeChanged;

    public bool Play()
    {
        _mediaPlayer.Media = _currentMedia;
        return _mediaPlayer.Play();
    }
    public void Pause() => _mediaPlayer.Pause();
    public void Resume() => _mediaPlayer.SetPause(false);
    public void Stop() => _mediaPlayer.Stop();

    public async Task PlayAndWaitAsync(CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnPlaying(object? s, EventArgs e) => tcs.TrySetResult();
        void OnError(object? s, EventArgs e) => tcs.TrySetException(
            new InvalidOperationException("LibVLC encountered a playback error."));

        _mediaPlayer.Playing += OnPlaying;
        _mediaPlayer.EncounteredError += OnError;

        try
        {
            if (_mediaPlayer.Media != _currentMedia)
                _mediaPlayer.Media = _currentMedia;

            if (!_mediaPlayer.Play())
                throw new InvalidOperationException("LibVLC failed to start playback.");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5));
            cts.Token.Register(() => tcs.TrySetCanceled(cts.Token));

            await tcs.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout — Playing event didn't fire within 5s, proceed anyway
        }
        finally
        {
            _mediaPlayer.Playing -= OnPlaying;
            _mediaPlayer.EncounteredError -= OnError;
        }
    }

    public async Task StopAndWaitAsync(CancellationToken cancellationToken = default)
    {
        if (!_mediaPlayer.IsPlaying)
            return;

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        void OnStopped(object? s, EventArgs e) => tcs.TrySetResult();
        _mediaPlayer.Stopped += OnStopped;

        try
        {
            _mediaPlayer.Stop();

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(2));
            cts.Token.Register(() => tcs.TrySetCanceled(cts.Token));

            await tcs.Task.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // Timeout — stop didn't fire within 2s, proceed anyway
        }
        finally
        {
            _mediaPlayer.Stopped -= OnStopped;
        }
    }

    public void SetMedia() => _mediaPlayer.Media = _currentMedia;

    public void DisposeCurrentMedia()
    {
        _currentMedia?.Dispose();
        _currentMedia = null;
    }

    public async Task LoadMediaAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _currentMedia?.Dispose();
        _currentMedia = new Media(_libVLC, filePath, FromType.FromPath);
        await _currentMedia.Parse(MediaParseOptions.ParseLocal, cancellationToken: cancellationToken);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _currentMedia?.Dispose();
        _mediaPlayer.Dispose();
        _libVLC.Dispose();
    }
}
