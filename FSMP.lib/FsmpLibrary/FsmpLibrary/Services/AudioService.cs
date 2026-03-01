using FSMP.Core.Interfaces;
using FSMP.Core.Models;

namespace FsmpLibrary.Services;

/// <summary>
/// Audio service implementation using IAudioPlayer for playback.
/// </summary>
public class AudioService : IAudioService
{
    private readonly IAudioPlayerFactory _playerFactory;
    private readonly float _initialVolume;
    private IAudioPlayer? _player;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AudioService with default volume (0.75).
    /// </summary>
    public AudioService(IAudioPlayerFactory playerFactory)
        : this(playerFactory, 0.75f)
    {
    }

    /// <summary>
    /// Initializes a new instance of the AudioService with the specified initial volume.
    /// </summary>
    public AudioService(IAudioPlayerFactory playerFactory, float initialVolume)
    {
        _playerFactory = playerFactory ?? throw new ArgumentNullException(nameof(playerFactory));
        _initialVolume = Math.Clamp(initialVolume, 0f, 1f);
    }

    /// <inheritdoc/>
    public IAudioPlayer Player
    {
        get
        {
            if (_player == null)
            {
                try
                {
                    _player = _playerFactory.CreatePlayer();
                    _player.Volume = _initialVolume;
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Audio engine not available.", ex);
                }
            }
            return _player;
        }
    }

    /// <inheritdoc/>
    public Track? CurrentTrack { get; private set; }

    /// <inheritdoc/>
    public float Volume
    {
        get => Player.Volume;
        set
        {
            Player.Volume = value;
            VolumeChanged?.Invoke(this, value);
        }
    }

    /// <inheritdoc/>
    public bool IsMuted
    {
        get => Player.IsMuted;
        set => Player.IsMuted = value;
    }

    /// <inheritdoc/>
    public event EventHandler<TrackChangedEventArgs>? TrackChanged;

    /// <inheritdoc/>
    public event EventHandler<float>? VolumeChanged;

    /// <inheritdoc/>
    public async Task PlayTrackAsync(Track track, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(track);

        if (string.IsNullOrWhiteSpace(track.FilePath))
            throw new ArgumentException("Track has no file path.", nameof(track));

        var previousTrack = CurrentTrack;
        CurrentTrack = track;

        await Player.LoadAsync(track.FilePath, cancellationToken);
        await Player.PlayAsync(cancellationToken);

        TrackChanged?.Invoke(this, new TrackChangedEventArgs
        {
            PreviousTrack = previousTrack,
            NewTrack = track
        });
    }

    /// <inheritdoc/>
    public async Task PlayFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var previousTrack = CurrentTrack;
        CurrentTrack = null; // No track metadata for raw file playback

        await Player.LoadAsync(filePath, cancellationToken);
        await Player.PlayAsync(cancellationToken);

        TrackChanged?.Invoke(this, new TrackChangedEventArgs
        {
            PreviousTrack = previousTrack,
            NewTrack = null
        });
    }

    /// <inheritdoc/>
    public Task PauseAsync() => Player.PauseAsync();

    /// <inheritdoc/>
    public Task ResumeAsync() => Player.PlayAsync();

    /// <inheritdoc/>
    public Task StopAsync() => Player.StopAsync();

    /// <inheritdoc/>
    public Task SeekAsync(TimeSpan position) => Player.SeekAsync(position);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _player?.Dispose();
        GC.SuppressFinalize(this);
    }
}
