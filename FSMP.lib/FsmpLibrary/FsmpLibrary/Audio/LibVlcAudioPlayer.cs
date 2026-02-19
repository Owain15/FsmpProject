using FSMP.Core.Audio;
using FSMP.Core.Interfaces;
using FSMP.Core.Interfaces.EventArgs;

namespace FsmpLibrary.Audio;

/// <summary>
/// Audio player implementation using an IMediaPlayerAdapter for platform-specific playback.
/// Contains all business logic: state management, validation, event translation, disposal.
/// </summary>
public class LibVlcAudioPlayer : IAudioPlayer
{
    private readonly IMediaPlayerAdapter _adapter;
    private string _currentFilePath = string.Empty;
    private bool _disposed;

    /// <summary>
    /// Creates a player with the default LibVLC-based adapter.
    /// </summary>
    public LibVlcAudioPlayer() : this(new LibVlcMediaPlayerAdapter()) { }

    /// <summary>
    /// Creates a player with the specified adapter (enables unit testing).
    /// </summary>
    public LibVlcAudioPlayer(IMediaPlayerAdapter adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

        _adapter.Playing += OnPlaying;
        _adapter.Paused += OnPaused;
        _adapter.Stopped += OnStopped;
        _adapter.EndReached += OnEndReached;
        _adapter.EncounteredError += OnError;
        _adapter.TimeChanged += OnTimeChanged;
    }

    /// <inheritdoc/>
    public PlaybackState State { get; private set; } = PlaybackState.Stopped;

    /// <inheritdoc/>
    public TimeSpan Position => TimeSpan.FromMilliseconds(_adapter.TimeMs);

    /// <inheritdoc/>
    public TimeSpan Duration => _adapter.DurationMs > 0
        ? TimeSpan.FromMilliseconds(_adapter.DurationMs)
        : TimeSpan.Zero;

    /// <inheritdoc/>
    public float Volume
    {
        get => _adapter.Volume / 100f;
        set => _adapter.Volume = (int)(Math.Clamp(value, 0f, 1f) * 100);
    }

    /// <inheritdoc/>
    public bool IsMuted
    {
        get => _adapter.Mute;
        set => _adapter.Mute = value;
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
            _adapter.Stop();
        }

        // Dispose previous media if exists
        _adapter.DisposeCurrentMedia();

        _currentFilePath = filePath;
        SetState(PlaybackState.Loading);

        // Load and parse new media
        await _adapter.LoadMediaAsync(filePath, cancellationToken);

        SetState(PlaybackState.Stopped);
    }

    /// <inheritdoc/>
    public Task PlayAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!_adapter.HasMedia)
            throw new InvalidOperationException("No media loaded. Call LoadAsync first.");

        _adapter.SetMedia();
        _adapter.Play();

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task PauseAsync()
    {
        ThrowIfDisposed();
        _adapter.Pause();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync()
    {
        ThrowIfDisposed();
        _adapter.Stop();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task SeekAsync(TimeSpan position)
    {
        ThrowIfDisposed();
        _adapter.TimeMs = (long)position.TotalMilliseconds;
        return Task.CompletedTask;
    }

    // Event handlers
    private void OnPlaying(object? sender, EventArgs e) => SetState(PlaybackState.Playing);

    private void OnPaused(object? sender, EventArgs e) => SetState(PlaybackState.Paused);

    private void OnStopped(object? sender, EventArgs e) => SetState(PlaybackState.Stopped);

    private void OnEndReached(object? sender, EventArgs e)
    {
        SetState(PlaybackState.Stopped);
        PlaybackCompleted?.Invoke(this, new PlaybackCompletedEventArgs
        {
            FilePath = _currentFilePath,
            CompletedSuccessfully = true
        });
    }

    private void OnError(object? sender, EventArgs e)
    {
        SetState(PlaybackState.Error);
        PlaybackError?.Invoke(this, new PlaybackErrorEventArgs
        {
            FilePath = _currentFilePath,
            ErrorMessage = "LibVLC encountered a playback error."
        });
    }

    private void OnTimeChanged(object? sender, long timeMs)
    {
        PositionChanged?.Invoke(this, new PositionChangedEventArgs
        {
            Position = TimeSpan.FromMilliseconds(timeMs),
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
        _adapter.Playing -= OnPlaying;
        _adapter.Paused -= OnPaused;
        _adapter.Stopped -= OnStopped;
        _adapter.EndReached -= OnEndReached;
        _adapter.EncounteredError -= OnError;
        _adapter.TimeChanged -= OnTimeChanged;

        // Dispose adapter (which disposes LibVLC resources)
        _adapter.Dispose();
    }
}
