using FSMP.Core.Audio;
using FSMP.Core.Interfaces;
using FSMP.Core.Interfaces.EventArgs;

namespace FSMP.Platform.Android.Audio;

/// <summary>
/// Android audio player using LibVLC. Same business logic as the Windows version
/// but with the Android-specific media player adapter.
/// </summary>
public class LibVlcAndroidAudioPlayer : IAudioPlayer
{
    private readonly IMediaPlayerAdapter _adapter;
    private string _currentFilePath = string.Empty;
    private bool _disposed;

    public LibVlcAndroidAudioPlayer() : this(new LibVlcAndroidMediaPlayerAdapter()) { }

    public LibVlcAndroidAudioPlayer(IMediaPlayerAdapter adapter)
    {
        _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));

        _adapter.Playing += OnPlaying;
        _adapter.Paused += OnPaused;
        _adapter.Stopped += OnStopped;
        _adapter.EndReached += OnEndReached;
        _adapter.EncounteredError += OnError;
        _adapter.TimeChanged += OnTimeChanged;
    }

    public PlaybackState State { get; private set; } = PlaybackState.Stopped;
    public TimeSpan Position => TimeSpan.FromMilliseconds(_adapter.TimeMs);
    public TimeSpan Duration => _adapter.DurationMs > 0
        ? TimeSpan.FromMilliseconds(_adapter.DurationMs)
        : TimeSpan.Zero;

    public float Volume
    {
        get => _adapter.Volume / 100f;
        set => _adapter.Volume = (int)(Math.Clamp(value, 0f, 1f) * 100);
    }

    public bool IsMuted
    {
        get => _adapter.Mute;
        set => _adapter.Mute = value;
    }

    public event EventHandler<PlaybackStateChangedEventArgs>? StateChanged;
    public event EventHandler<PlaybackCompletedEventArgs>? PlaybackCompleted;
    public event EventHandler<PlaybackErrorEventArgs>? PlaybackError;
    public event EventHandler<PositionChangedEventArgs>? PositionChanged;

    public async Task LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException("Audio file not found.", filePath);

        if (State == PlaybackState.Playing || State == PlaybackState.Paused)
            await _adapter.StopAndWaitAsync(cancellationToken);

        _adapter.DisposeCurrentMedia();
        _currentFilePath = filePath;
        SetState(PlaybackState.Loading);

        await _adapter.LoadMediaAsync(filePath, cancellationToken);
        SetState(PlaybackState.Stopped);
    }

    public async Task PlayAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        if (!_adapter.HasMedia)
            throw new InvalidOperationException("No media loaded. Call LoadAsync first.");

        await _adapter.PlayAndWaitAsync(cancellationToken);
    }

    public Task ResumeAsync()
    {
        ThrowIfDisposed();
        _adapter.Resume();
        return Task.CompletedTask;
    }

    public Task PauseAsync()
    {
        ThrowIfDisposed();
        _adapter.Pause();
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        ThrowIfDisposed();
        _adapter.Stop();
        return Task.CompletedTask;
    }

    public Task SeekAsync(TimeSpan position)
    {
        ThrowIfDisposed();
        _adapter.TimeMs = (long)position.TotalMilliseconds;
        return Task.CompletedTask;
    }

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

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _adapter.Playing -= OnPlaying;
        _adapter.Paused -= OnPaused;
        _adapter.Stopped -= OnStopped;
        _adapter.EndReached -= OnEndReached;
        _adapter.EncounteredError -= OnError;
        _adapter.TimeChanged -= OnTimeChanged;

        _adapter.Dispose();
    }
}
