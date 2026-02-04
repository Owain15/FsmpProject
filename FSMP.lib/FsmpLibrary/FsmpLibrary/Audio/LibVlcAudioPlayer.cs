using LibVLCSharp.Shared;
using FsmpLibrary.Interfaces;
using FsmpLibrary.Interfaces.EventArgs;

namespace FsmpLibrary.Audio;

/// <summary>
/// LibVLCSharp-based audio player implementation.
/// Supports WAV, WMA, MP3 and virtually any audio format supported by VLC.
/// </summary>
public class LibVlcAudioPlayer : IAudioPlayer
{
    private static bool _coreInitialized;
    private static readonly object _initLock = new();

    private readonly LibVLC _libVLC;
    private readonly MediaPlayer _mediaPlayer;
    private Media? _currentMedia;
    private string _currentFilePath = string.Empty;
    private bool _disposed;

    public LibVlcAudioPlayer()
    {
        // Initialize LibVLC core once (thread-safe)
        lock (_initLock)
        {
            if (!_coreInitialized)
            {
                // Specify path to native LibVLC libraries (in libvlc/win-x64 subdirectory)
                var libvlcPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "libvlc", "win-x64"));

                // Set VLC_PLUGIN_PATH environment variable for plugins discovery
                var pluginsPath = Path.Combine(libvlcPath, "plugins");
                Environment.SetEnvironmentVariable("VLC_PLUGIN_PATH", pluginsPath);

                Core.Initialize(libvlcPath);
                _coreInitialized = true;
            }
        }

        // Create LibVLC instance with minimal logging
        _libVLC = new LibVLC("--quiet");
        _mediaPlayer = new MediaPlayer(_libVLC);

        // Wire up events
        _mediaPlayer.Playing += OnPlaying;
        _mediaPlayer.Paused += OnPaused;
        _mediaPlayer.Stopped += OnStopped;
        _mediaPlayer.EndReached += OnEndReached;
        _mediaPlayer.EncounteredError += OnError;
        _mediaPlayer.TimeChanged += OnTimeChanged;
    }

    /// <inheritdoc/>
    public PlaybackState State { get; private set; } = PlaybackState.Stopped;

    /// <inheritdoc/>
    public TimeSpan Position => TimeSpan.FromMilliseconds(_mediaPlayer.Time);

    /// <inheritdoc/>
    public TimeSpan Duration => _currentMedia?.Duration > 0
        ? TimeSpan.FromMilliseconds(_currentMedia.Duration)
        : TimeSpan.Zero;

    /// <inheritdoc/>
    public float Volume
    {
        get => _mediaPlayer.Volume / 100f;
        set => _mediaPlayer.Volume = (int)(Math.Clamp(value, 0f, 1f) * 100);
    }

    /// <inheritdoc/>
    public bool IsMuted
    {
        get => _mediaPlayer.Mute;
        set => _mediaPlayer.Mute = value;
    }

    /// <inheritdoc/>
    public event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;

    /// <inheritdoc/>
    public event EventHandler<PlaybackCompletedEventArgs>? PlaybackCompleted;

    /// <inheritdoc/>
    public event EventHandler<PlaybackErrorEventArgs>? PlaybackError;

    /// <inheritdoc/>
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;

    /// <inheritdoc/>
    public async Task LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        // Stop current playback
        if (State == PlaybackState.Playing || State == PlaybackState.Paused)
        {
            _mediaPlayer.Stop();
        }

        // Dispose previous media if exists
        _currentMedia?.Dispose();

        _currentFilePath = filePath;
        SetState(PlaybackState.Loading);

        // Create new media from file path
        _currentMedia = new Media(_libVLC, filePath, FromType.FromPath);

        // Parse media to get duration (async operation)
        await _currentMedia.Parse(MediaParseOptions.ParseLocal, cancellationToken: cancellationToken);

        SetState(PlaybackState.Stopped);
    }

    /// <inheritdoc/>
    public Task PlayAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (_currentMedia == null)
            throw new InvalidOperationException("No media loaded. Call LoadAsync first.");

        _mediaPlayer.Media = _currentMedia;
        _mediaPlayer.Play();

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task PauseAsync()
    {
        ThrowIfDisposed();
        _mediaPlayer.Pause();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync()
    {
        ThrowIfDisposed();
        _mediaPlayer.Stop();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SeekAsync(TimeSpan position)
    {
        ThrowIfDisposed();
        _mediaPlayer.Time = (long)position.TotalMilliseconds;
        return Task.CompletedTask;
    }

    // Event handlers
    private void OnPlaying(object? sender, System.EventArgs e) => SetState(PlaybackState.Playing);

    private void OnPaused(object? sender, System.EventArgs e) => SetState(PlaybackState.Paused);

    private void OnStopped(object? sender, System.EventArgs e) => SetState(PlaybackState.Stopped);

    private void OnEndReached(object? sender, System.EventArgs e)
    {
        SetState(PlaybackState.Stopped);
        PlaybackCompleted?.Invoke(this, new PlaybackCompletedEventArgs
        {
            FilePath = _currentFilePath,
            CompletedSuccessfully = true
        });
    }

    private void OnError(object? sender, System.EventArgs e)
    {
        SetState(PlaybackState.Error);
        PlaybackError?.Invoke(this, new PlaybackErrorEventArgs
        {
            FilePath = _currentFilePath,
            ErrorMessage = "LibVLC encountered a playback error."
        });
    }

    private void OnTimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        PositionChanged?.Invoke(this, new PositionChangedEventArgs
        {
            Position = TimeSpan.FromMilliseconds(e.Time),
            Duration = Duration
        });
    }

    private void SetState(PlaybackState newState)
    {
        var oldState = State;
        if (oldState != newState)
        {
            State = newState;
            StateChanged?.Invoke(this, new PlaybackStateChangedEventArgs
            {
                OldState = oldState,
                NewState = newState
            });
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        // Unsubscribe events
        _mediaPlayer.Playing -= OnPlaying;
        _mediaPlayer.Paused -= OnPaused;
        _mediaPlayer.Stopped -= OnStopped;
        _mediaPlayer.EndReached -= OnEndReached;
        _mediaPlayer.EncounteredError -= OnError;
        _mediaPlayer.TimeChanged -= OnTimeChanged;

        // Dispose resources
        _currentMedia?.Dispose();
        _mediaPlayer.Dispose();
        _libVLC.Dispose();
    }
}
